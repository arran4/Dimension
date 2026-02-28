import 'dart:typed_data';
import 'command.dart';

class DataCommand extends Command {
  Uint8List data = Uint8List(0);
  int dataLength = 0;
}
