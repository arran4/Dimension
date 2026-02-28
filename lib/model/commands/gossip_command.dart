import 'gossip_peer.dart';
import 'command.dart';

class GossipCommand extends Command {
  List<GossipPeer> peers = [];
  bool requestingGossipBack = false;
  int circleId = 0;
}
