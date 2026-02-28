import 'connection.dart';
import 'commands/command.dart';
import 'commands/hello_command.dart';

abstract class IncomingConnection extends Connection {
  bool rateLimiterDisabled = false;
  HelloCommand? hello;
  String? lastFolder;

  void Function(Command c, IncomingConnection con)? commandReceived;
  void send(Command c);
  bool get connected;
}
