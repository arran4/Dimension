import 'commands/command.dart';

abstract class Connection {
  void send(Command c);
}
