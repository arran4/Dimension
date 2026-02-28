import 'dart:convert';
import 'dart:typed_data';

import 'commands/command.dart';

typedef CommandEncoder<T extends Command> = Map<String, dynamic> Function(T command);
typedef CommandDecoder<T extends Command> = T Function(Map<String, dynamic> json);

class Serializer {
  final Map<String, _CodecEntry<Command>> _codecsByType =
      <String, _CodecEntry<Command>>{};
  final Map<Type, String> _typeNamesByCommandType = <Type, String>{};

  void register<T extends Command>({
    required String typeName,
    required CommandEncoder<T> encode,
    required CommandDecoder<T> decode,
  }) {
    _codecsByType[typeName] = _CodecEntry<Command>(
      encode: (Command command) => encode(command as T),
      decode: (Map<String, dynamic> json) => decode(json),
    );
    _typeNamesByCommandType[T] = typeName;
  }

  Uint8List serialize(Command command) {
    final typeName = _typeNamesByCommandType[command.runtimeType];
    if (typeName == null) {
      throw StateError('No command codec registered for ${command.runtimeType}.');
    }

    final codec = _codecsByType[typeName]!;
    final typeBytes = utf8.encode(typeName);
    final payloadText = jsonEncode(codec.encode(command));
    final payloadBytes = utf8.encode(payloadText);

    final output = Uint8List(8 + typeBytes.length + payloadBytes.length);
    final byteData = ByteData.view(output.buffer);
    byteData.setInt32(0, typeBytes.length, Endian.little);
    output.setRange(4, 4 + typeBytes.length, typeBytes);
    byteData.setInt32(4 + typeBytes.length, payloadBytes.length, Endian.little);
    output.setRange(8 + typeBytes.length, output.length, payloadBytes);
    return output;
  }

  String getType(Uint8List data) {
    final reader = _PacketReader(data);
    return reader.readString();
  }

  String getText(Uint8List data) {
    final reader = _PacketReader(data);
    reader.readString(); // command type
    return reader.readString();
  }

  Command? deserialize(Uint8List data) {
    try {
      final typeName = getType(data);
      final codec = _codecsByType[typeName];
      if (codec == null) {
        return null;
      }

      final jsonText = getText(data);
      final dynamic decoded = jsonDecode(jsonText);
      if (decoded is! Map<String, dynamic>) {
        return null;
      }
      return codec.decode(decoded);
    } on FormatException {
      return null;
    }
  }
}

class _CodecEntry<T extends Command> {
  const _CodecEntry({required this.encode, required this.decode});

  final Map<String, dynamic> Function(T command) encode;
  final T Function(Map<String, dynamic> json) decode;
}

class _PacketReader {
  _PacketReader(this._data);

  final Uint8List _data;
  int _offset = 0;

  String readString() {
    final length = _readLength();
    if (_offset + length > _data.length) {
      throw const FormatException('Packet data is truncated.');
    }

    final bytes = _data.sublist(_offset, _offset + length);
    _offset += length;
    return utf8.decode(bytes);
  }

  int _readLength() {
    if (_offset + 4 > _data.length) {
      throw const FormatException('Packet data is truncated.');
    }

    final value = ByteData.sublistView(_data, _offset, _offset + 4)
        .getInt32(0, Endian.little);
    _offset += 4;

    if (value < 0) {
      throw const FormatException('Negative field length in packet.');
    }
    return value;
  }
}
