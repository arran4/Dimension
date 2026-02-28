import 'dart:io';

import 'incoming_connection.dart';
import 'outgoing_connection.dart';
import 'commands/command.dart';

// Note: Dart does not have a native UDT library equivalent to the C# Udt.Socket out of the box.
// These are stub classes that outline the structure.
// If actual UDT is required, a Dart package or FFI wrapper will be needed.
// For now, these throw UnimplementedError.

class UdtIncomingConnection extends IncomingConnection {
  // ignore: unused_field
  final dynamic _socket; // Replace with actual UDT Socket type if available
  // ignore: unused_field
  final Socket _underlying;

  UdtIncomingConnection(this._socket, this._underlying) {
    _startReceiveLoop();
  }

  void _startReceiveLoop() {
    // Stub: Would listen to the UDT socket in a loop
    throw UnimplementedError("UDT not implemented in Dart yet");
  }

  @override
  Future<void> send(Command c) async {
    throw UnimplementedError("UDT not implemented in Dart yet");
  }

  bool get connecting {
    // try { return socket.State == Udt.SocketState.Connecting; }
    throw UnimplementedError("UDT not implemented in Dart yet");
  }

  @override
  bool get connected {
    // try { return socket.State != Udt.SocketState.Closed; }
    return false;
  }
}


class UdtOutgoingConnection extends OutgoingConnection {
  static int successfulConnections = 0;
  // ignore: unused_field
  final dynamic _socket; // Replace with actual UDT Socket type
  // ignore: unused_field
  final Socket _underlying;

  UdtOutgoingConnection(this._socket, this._underlying) {
    // send(App.theCore.generateHello());
    successfulConnections++;
    _startReceiveLoop();
  }

  void _startReceiveLoop() {
    throw UnimplementedError("UDT not implemented in Dart yet");
  }

  @override
  Future<void> send(Command c) async {
    throw UnimplementedError("UDT not implemented in Dart yet");
  }

  bool get connecting {
    throw UnimplementedError("UDT not implemented in Dart yet");
  }

  @override
  bool get connected {
    return false;
  }
}
