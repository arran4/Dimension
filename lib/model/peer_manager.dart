import 'dart:convert';
import 'package:crypto/crypto.dart';
import 'peer.dart';
import 'commands/mini_hello.dart';
import 'commands/hello_command.dart';

class PeerManager {
  Map<int, Peer> peers = {};

  List<Peer> get allPeers {
    return peers.values.toList();
  }

  List<Peer> allPeersInCircle(int id, [bool includeMaybeDead = true]) {
    final bytes = utf8.encode("lan");
    final digest = sha512.convert(bytes);
    // Mimic BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes("LAN".ToLower())), 0)
    // We'll take the first 8 bytes and treat as little-endian uint64
    final hashBytes = digest.bytes.sublist(0, 8);
    int lanHash = 0;
    for (int i = 0; i < 8; i++) {
      lanHash |= (hashBytes[i] << (8 * i));
    }

    List<Peer> output = [];
    for (Peer p in allPeers) {
      if (p.circles.contains(id) && !p.quit) {
        if (!p.maybeDead || includeMaybeDead) {
          if (id == lanHash) {
            if (p.isLocal) {
              output.add(p);
            }
          } else {
            output.add(p);
          }
        }
      }
    }
    return output;
  }

  void doPeerRemoved(Peer p) {
    for (int u in p.circles) {
      peerRemoved(p, u, true);
    }
  }

  bool havePeerWithAddress(List<String> i, String e) {
    // Stub core internalIPs check as we don't have Core yet.
    // Assuming 'e' is IP string.
    for (Peer p in allPeers) {
      if (p.publicAddress == e) {
        return true;
      }
    }
    return false;
  }

  // Event callback lists
  List<Function(String, Peer)> onPeerRenamed = [];
  List<Function(Peer, int, bool)> onPeerAdded = [];
  List<Function(Peer)> onPeerUpdated = [];
  List<Function(Peer, int, bool)> onPeerRemoved = [];

  void peerRemoved(Peer p, int channel, bool update) {
    for (var callback in onPeerRemoved) {
      callback(p, channel, update);
    }
  }

  void peerAdded(Peer p, int channel, bool update) {
    for (var callback in onPeerAdded) {
      callback(p, channel, update);
    }
  }

  void peerUpdated(Peer p) {
    for (var callback in onPeerUpdated) {
      callback(p);
    }
  }

  void peerRenamed(String oldName, Peer p) {
    for (var callback in onPeerRenamed) {
      callback(oldName, p);
    }
  }

  bool parseMiniHello(MiniHello h, dynamic sender) {
    Peer? p;
    if (peers.containsKey(h.id)) {
      p = peers[h.id];
    }

    if (p != null) {
      if (DateTime.now().difference(p.timeQuit).inSeconds > 3) {
        if (p.quit) {
          final bytes = utf8.encode("lan");
          final digest = sha512.convert(bytes);
          final hashBytes = digest.bytes.sublist(0, 8);
          int lanHash = 0;
          for (int i = 0; i < 8; i++) {
            lanHash |= (hashBytes[i] << (8 * i));
          }

          for (int u in p.circles) {
            if (u == lanHash) {
              peerAdded(p, u, true);
            } else {
              peerAdded(p, u, true);
            }
          }
        }
        if (h.afk != null) {
          if (p.afk != h.afk) {
            p.afk = h.afk;
            peerUpdated(peers[h.id]!);
          }
        }
        if (p.quit) {
          p.quit = false;
          peerUpdated(peers[h.id]!);
        }
        p.lastContact = DateTime.now();
      }
    } else {
      return true;
    }
    return false;
  }

  void parseHello(HelloCommand h, dynamic sender) {
    // Mimicking Mono specific fixes for username and description
    // Assuming we apply them generally in Dart as well
    String newName = "";
    for (int i = 0; i < h.username.length; i++) {
      String c = h.username[i];
      if (RegExp(r'[a-zA-Z0-9\s\p{P}]', unicode: true).hasMatch(c)) {
        newName += c;
      } else {
        newName += "?";
      }
    }
    h.username = newName;

    newName = "";
    for (int i = 0; i < h.description.length; i++) {
      String c = h.description[i];
      if (RegExp(r'[a-zA-Z0-9\s\p{P}]', unicode: true).hasMatch(c)) {
        newName += c;
      } else {
        newName += "?";
      }
    }
    h.description = newName;

    bool wasQuit = false;
    List<int> channels = [];
    List<int> oldChannels = [];
    String oldName = "";
    bool renamed = false;
    bool updated = false;
    bool added = false;

    if (peers.containsKey(h.id)) {
      if (DateTime.now().difference(peers[h.id]!.timeQuit).inSeconds < 3) return;
      wasQuit = peers[h.id]!.quit;

      peers[h.id]!.internalAddress = h.internalIPs;

      if (peers[h.id]!.quit) added = true;
      peers[h.id]!.quit = false;
      peers[h.id]!.useUDT = h.useUDT;
      peers[h.id]!.actualEndpoint = sender;

      if (h.externalIP.isNotEmpty) {
        peers[h.id]!.publicAddress = h.externalIP;
      }

      if (peers[h.id]!.share != h.myShare) {
        peers[h.id]!.share = h.myShare;
        updated = true;
      }

      String s1 = peers[h.id]!.circles.join(", ") + ", ";
      String s2 = h.myCircles.join(", ") + ", ";
      if (s1 != s2) {
        oldChannels.addAll(peers[h.id]!.circles);
        peers[h.id]!.circles = h.myCircles;
        added = true;
      }
    } else {
      peers[h.id] = Peer();
      peers[h.id]!.id = h.id;
      peers[h.id]!.actualEndpoint = sender;
      if (h.externalIP.isNotEmpty) {
        peers[h.id]!.publicAddress = h.externalIP;
      }
      peers[h.id]!.username = h.username;
      peers[h.id]!.circles = h.myCircles;
      added = true;
    }

    if (peers[h.id]!.description != h.description) {
      peers[h.id]!.description = h.description;
      updated = true;
    }

    peers[h.id]!.addEndpointToHistory(sender);
    peers[h.id]!.behindDoubleNAT = h.behindDoubleNAT;
    peers[h.id]!.externalControlPort = h.externalControlPort;
    peers[h.id]!.externalDataPort = h.externalDataPort;
    peers[h.id]!.localControlPort = h.internalControlPort;
    peers[h.id]!.localDataPort = h.internalDataPort;
    peers[h.id]!.localUDTPort = h.internalUdtPort;

    if (peers[h.id]!.username != h.username) {
      oldName = peers[h.id]!.username;
      peers[h.id]!.username = h.username;
      renamed = true;
    }

    peers[h.id]!.quit = false;
    peers[h.id]!.peerCount = h.peerCount;
    peers[h.id]!.lastContact = DateTime.now();
    peers[h.id]!.buildNumber = h.buildNumber;
    channels.addAll(peers[h.id]!.circles);

    if (updated) peerUpdated(peers[h.id]!);
    if (renamed) peerRenamed(oldName, peers[h.id]!);

    if (added) {
      final bytes = utf8.encode("lan");
      final digest = sha512.convert(bytes);
      final hashBytes = digest.bytes.sublist(0, 8);
      int lanHash = 0;
      for (int i = 0; i < 8; i++) {
        lanHash |= (hashBytes[i] << (8 * i));
      }

      if (!peers[h.id]!.quit || DateTime.now().difference(peers[h.id]!.timeQuit).inSeconds > 3) {
        for (int u in channels) {
          if (u == lanHash) {
            if (!oldChannels.contains(u) && peers[h.id]!.isLocal) {
              peerAdded(peers[h.id]!, u, true);
            }
          } else {
            if (!oldChannels.contains(u)) {
              peerAdded(peers[h.id]!, u, true);
            }
          }
        }
      }

      if (DateTime.now().difference(peers[h.id]!.timeQuit).inSeconds > 3) {
        for (int u in oldChannels) {
          if (u == lanHash) {
            if (!channels.contains(u) && peers[h.id]!.isLocal) {
              peerRemoved(peers[h.id]!, u, !wasQuit);
            }
          } else {
            if (!channels.contains(u)) {
              peerRemoved(peers[h.id]!, u, !wasQuit);
            }
          }
        }
      }
    }
  }
}
