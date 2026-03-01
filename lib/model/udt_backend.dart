import 'dart:async';
import 'dart:typed_data';

import 'serializer.dart';
import 'udt_connection.dart';

class UdtEndpoint {
  const UdtEndpoint({required this.host, required this.port});

  final String host;
  final int port;

  @override
  bool operator ==(Object other) =>
      other is UdtEndpoint && other.host == host && other.port == port;

  @override
  int get hashCode => Object.hash(host, port);
}

abstract class UdtBackend {
  Future<UdtTransport> connect(UdtEndpoint endpoint);
}

/// In-memory backend used for tests and temporary mock wiring while dart-udt is
/// being finalized.
class InMemoryUdtBackend implements UdtBackend {
  final Map<UdtEndpoint, _InMemoryUdtTransport> _listeners =
      <UdtEndpoint, _InMemoryUdtTransport>{};

  UdtTransport registerListener(UdtEndpoint endpoint) {
    return _listeners.putIfAbsent(endpoint, () => _InMemoryUdtTransport());
  }

  @override
  Future<UdtTransport> connect(UdtEndpoint endpoint) async {
    final listener = _listeners[endpoint];
    if (listener == null) {
      throw StateError('No in-memory UDT listener for $endpoint');
    }

    final client = _InMemoryUdtTransport();
    client._peer = listener;
    listener._peer = client;
    client._isConnecting = false;
    listener._isConnecting = false;
    return client;
  }
}

class UdtConnectionFactory {
  const UdtConnectionFactory({required this.backend, required this.serializer});

  final UdtBackend backend;
  final Serializer serializer;

  Future<UdtOutgoingConnection> createOutgoing(UdtEndpoint endpoint) async {
    final transport = await backend.connect(endpoint);
    return UdtOutgoingConnection(transport: transport, serializer: serializer);
  }

  UdtIncomingConnection createIncoming(UdtTransport transport) {
    return UdtIncomingConnection(transport: transport, serializer: serializer);
  }
}

class _InMemoryUdtTransport implements UdtTransport {
  final StreamController<Uint8List> _incoming =
      StreamController<Uint8List>.broadcast();
  _InMemoryUdtTransport? _peer;
  bool _isConnecting = false;
  bool _isClosed = false;

  @override
  Stream<Uint8List> get incomingPackets => _incoming.stream;

  @override
  bool get isConnected => !_isClosed && _peer != null;

  @override
  bool get isConnecting => _isConnecting;

  @override
  Future<void> close() async {
    _isClosed = true;
    await _incoming.close();
  }

  @override
  Future<void> send(Uint8List packet) async {
    if (_isClosed || _peer == null || _peer!._isClosed) {
      return;
    }
    _peer!._incoming.add(Uint8List.fromList(packet));
  }
}
