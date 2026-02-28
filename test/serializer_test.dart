import 'dart:typed_data';

import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/serializer.dart';
import 'package:flutter_test/flutter_test.dart';

class _TestCommand extends Command {
  _TestCommand(this.value);

  final String value;
}

void main() {
  group('Serializer', () {
    test('serializes type and JSON payload in little-endian packet format', () {
      final serializer = Serializer()
        ..register<_TestCommand>(
          typeName: 'Dimension.Model.Commands.TestCommand',
          encode: (command) => <String, dynamic>{'value': command.value},
          decode: (json) => _TestCommand(json['value'] as String? ?? ''),
        );

      final packet = serializer.serialize(_TestCommand('hello'));

      expect(
        serializer.getType(packet),
        'Dimension.Model.Commands.TestCommand',
      );
      expect(serializer.getText(packet), '{"value":"hello"}');
    });

    test('deserialize rebuilds command via registered decoder', () {
      final serializer = Serializer()
        ..register<_TestCommand>(
          typeName: 'Dimension.Model.Commands.TestCommand',
          encode: (command) => <String, dynamic>{'value': command.value},
          decode: (json) => _TestCommand(json['value'] as String? ?? ''),
        );

      final command = _TestCommand('rebuilt');
      final packet = serializer.serialize(command);

      final decoded = serializer.deserialize(packet);
      expect(decoded, isA<_TestCommand>());
      expect((decoded as _TestCommand).value, 'rebuilt');
    });

    test('deserialize returns null for unknown command types', () {
      final serializer = Serializer()
        ..register<_TestCommand>(
          typeName: 'KnownCommand',
          encode: (command) => <String, dynamic>{'value': command.value},
          decode: (json) => _TestCommand(json['value'] as String? ?? ''),
        );

      final bytes = _buildPacket('UnknownCommand', '{"value":"x"}');
      expect(serializer.deserialize(bytes), isNull);
    });

    test('deserialize returns null when packet is truncated', () {
      final serializer = Serializer();
      final truncated = Uint8List.fromList(<int>[1, 0, 0, 0, 65]);

      expect(serializer.deserialize(truncated), isNull);
    });
  });
}

Uint8List _buildPacket(String type, String payload) {
  final typeBytes = type.codeUnits;
  final payloadBytes = payload.codeUnits;

  final result = Uint8List(8 + typeBytes.length + payloadBytes.length);
  final header = ByteData.sublistView(result);
  header.setInt32(0, typeBytes.length, Endian.little);
  result.setRange(4, 4 + typeBytes.length, typeBytes);
  header.setInt32(4 + typeBytes.length, payloadBytes.length, Endian.little);
  result.setRange(8 + typeBytes.length, result.length, payloadBytes);

  return result;
}
