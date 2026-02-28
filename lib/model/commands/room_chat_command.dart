import 'command.dart';

class RoomChatCommand extends Command {
  String content = '';
  int sequenceId = 0;
  int roomId = 0;
  int? userId;
}
