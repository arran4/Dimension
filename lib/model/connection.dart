import 'dart:async';
import 'commands/command.dart';

abstract class Connection {
  Future<void> send(Command c);
}
