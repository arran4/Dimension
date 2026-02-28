import 'dart:async';
import 'connection.dart';
import 'commands/command.dart';

abstract class OutgoingConnection extends Connection {
  bool rateLimiterDisabled = false;
  int rate = 0;
  void Function(Command c)? commandReceived;
  Future<void> send(Command c);
  bool get connected;
}
