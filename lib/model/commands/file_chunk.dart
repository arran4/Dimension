import 'data_command.dart';

class FileChunk extends DataCommand {
  int start = 0;
  int totalSize = 0;
  String path = '';
  String originalPath = '';
}
