import 'dart:async';
import 'dart:typed_data';

import 'commands/command.dart';
import 'incoming_connection.dart';
import 'outgoing_connection.dart';
import 'serializer.dart';

/// Temporary Dart-native UDT bridge that keeps command flow testable while the
/// real UDT transport is still being ported.
abstract class UdtTransport {
  Stream<Uint8List> get incomingPackets;

  Future<void> send(Uint8List packet);

  bool get isConnecting;

  bool get isConnected;

  Future<void> close();
}

class UdtIncomingConnection extends IncomingConnection {
  UdtIncomingConnection({
    required UdtTransport transport,
    required Serializer serializer,
  }) : _transport = transport,
       _serializer = serializer {
    _subscription = _transport.incomingPackets.listen(_onPacket);
  }

  final UdtTransport _transport;
  final Serializer _serializer;
  StreamSubscription<Uint8List>? _subscription;

  void _onPacket(Uint8List packet) {
    final command = _serializer.deserialize(packet);
    if (command == null) {
      return;
    }
    commandReceived?.call(command, this);
  }

  @override
  Future<void> send(Command c) {
    return _transport.send(_serializer.serialize(c));
  }

  bool get connecting => _transport.isConnecting;

  @override
  bool get connected => _transport.isConnected;

  Future<void> dispose() async {
    await _subscription?.cancel();
    await _transport.close();
  }
}

class UdtOutgoingConnection extends OutgoingConnection {
  static int successfulConnections = 0;

  UdtOutgoingConnection({
    required UdtTransport transport,
    required Serializer serializer,
  }) : _transport = transport,
       _serializer = serializer {
    successfulConnections++;
    _subscription = _transport.incomingPackets.listen(_onPacket);
  }

  final UdtTransport _transport;
  final Serializer _serializer;
  StreamSubscription<Uint8List>? _subscription;

  void _onPacket(Uint8List packet) {
    final command = _serializer.deserialize(packet);
    if (command == null) {
      return;
    }
    commandReceived?.call(command);
  }

  @override
  Future<void> send(Command c) {
    return _transport.send(_serializer.serialize(c));
  }

  bool get connecting => _transport.isConnecting;

  @override
  bool get connected => _transport.isConnected;

  Future<void> dispose() async {
    await _subscription?.cancel();
    await _transport.close();
  }
}
