import 'peer.dart';
import 'connection.dart';

class Transfer {
  String originalPath = "";
  String path = "";
  Peer? thePeer;
  Connection? con;
  bool download = false;
  String protocol = "";
  String filename = "";
  int size = 0;
  int startingByte = 0;
  int completed = 0;
  String username = "";
  int rate = 0;
  int userId = 0;
  DateTime timeCreated = DateTime.now();

  static List<Transfer> transfers = [];
}
