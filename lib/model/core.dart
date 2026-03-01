import 'dart:collection';

import 'commands/command.dart';
import 'commands/room_chat_command.dart';
import 'commands/search_command.dart';
import 'incoming_connection.dart';
import 'reliable_connection.dart';
import 'udt_connection.dart';

abstract class CoreSettingsStore {
  void setString(String key, String value);
}

abstract class CorePeer {
  int get id;

  bool get quit;

  Set<int> get circles;

  void sendCommand(Command command);
}

abstract class CorePeerDirectory {
  Iterable<CorePeer> get allPeers;

  Iterable<CorePeer> peersInCircle(int circleId);
}

abstract class CorePeerMutableDirectory implements CorePeerDirectory {
  bool addPeer(CorePeer peer);
}

typedef CoreIdleTimeProvider = Duration Function();
typedef CoreChatReceivedHandler = void Function(
  String message,
  int roomId,
  CorePeer? peer,
);

class Core {
  Core({
    required CorePeerDirectory peerDirectory,
    required CoreSettingsStore settings,
    required int localPeerId,
    CoreIdleTimeProvider? idleTimeProvider,
  }) : _peerDirectory = peerDirectory,
       _settings = settings,
       _localPeerId = localPeerId,
       _idleTimeProvider = idleTimeProvider ?? (() => Duration.zero);

  final CorePeerDirectory _peerDirectory;
  final CoreSettingsStore _settings;
  final int _localPeerId;
  final CoreIdleTimeProvider _idleTimeProvider;

  bool disposed = false;
  int incomingTcpConnections = 0;
  int incomingUdtConnections = 0;

  final Set<String> _circles = <String>{};
  final List<IncomingConnection> _incomings = <IncomingConnection>[];
  final List<CoreChatReceivedHandler> _chatListeners = <CoreChatReceivedHandler>[];

  int _lastSequenceId = 0;

  UnmodifiableListView<String> get circles =>
      UnmodifiableListView<String>(_circles.toList(growable: false));

  UnmodifiableListView<IncomingConnection> get incomings =>
      UnmodifiableListView<IncomingConnection>(_incomings);

  void dispose() {
    disposed = true;
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void Dispose() => dispose();

  void beginSearch(SearchCommand command) {
    for (final peer in _peerDirectory.allPeers) {
      if (!peer.quit) {
        peer.sendCommand(command);
      }
    }
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void beginSearchCompat(SearchCommand command) => beginSearch(command);

  void joinCircle(String name) {
    _circles.add(name);
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void joinCircleCompat(String name) => joinCircle(name);

  void leaveCircle(String name) {
    _circles.remove(name);
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void leaveCircleCompat(String name) => leaveCircle(name);


  bool addPeer(CorePeer peer) {
    final directory = _peerDirectory;
    if (directory is CorePeerMutableDirectory) {
      return directory.addPeer(peer);
    }

    // If the active peer directory is read-only, treat this as a no-op for now.
    return false;
  }

  // Temporary compatibility shim for line-by-line migration parity.
  bool addPeerCompat(CorePeer peer) => addPeer(peer);

  void sendChat(String content, int roomId) {
    if (_handleSlashCommand(content)) {
      return;
    }

    final command = RoomChatCommand()
      ..content = content
      ..userId = _localPeerId
      ..roomId = roomId
      ..sequenceId = _nextSequenceId();

    for (final peer in _peerDirectory.peersInCircle(roomId)) {
      if (!peer.quit) {
        peer.sendCommand(command);
      }
    }
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void sendChatCompat(String content, int roomId) => sendChat(content, roomId);

  bool _handleSlashCommand(String content) {
    if (!content.startsWith('/')) {
      return false;
    }
    final lower = content.toLowerCase();
    if (!lower.startsWith('/nick ')) {
      return false;
    }

    final nick = content.substring('/nick '.length).trim();
    if (nick.isEmpty) {
      return true;
    }

    final clipped = nick.length > 16 ? nick.substring(0, 16) : nick;
    _settings.setString('Username', clipped);
    return true;
  }

  int _nextSequenceId() {
    final sequence = _lastSequenceId;
    _lastSequenceId++;
    if (_lastSequenceId >= 0x7ffffffe) {
      _lastSequenceId = 0;
    }
    return sequence;
  }

  void addIncomingConnection(IncomingConnection connection) {
    connection.commandReceived = _commandReceived;
    if (connection is ReliableIncomingConnection) {
      incomingTcpConnections++;
    }
    if (connection is UdtIncomingConnection) {
      incomingUdtConnections++;
    }
    _incomings.add(connection);
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void addIncomingConnectionCompat(IncomingConnection connection) =>
      addIncomingConnection(connection);

  void removeIncomingConnection(IncomingConnection connection) {
    _incomings.remove(connection);
    connection.commandReceived = null;
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void removeIncomingConnectionCompat(IncomingConnection connection) =>
      removeIncomingConnection(connection);

  void _commandReceived(Command command, IncomingConnection connection) {
    // Remaining parity work: command routing/search result handling/file sharing
    // flow will be ported as App/Core orchestration is completed.
  }

  void addChatReceivedListener(CoreChatReceivedHandler listener) {
    _chatListeners.add(listener);
  }

  void removeChatReceivedListener(CoreChatReceivedHandler listener) {
    _chatListeners.remove(listener);
  }

  void chatReceived(String message, int roomId, CorePeer? peer) {
    for (final listener in _chatListeners) {
      listener(message, roomId, peer);
    }
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void chatReceivedCompat(String message, int roomId, CorePeer? peer) =>
      chatReceived(message, roomId, peer);

  Duration getIdleTime() => _idleTimeProvider();

  // Temporary compatibility shim for line-by-line migration parity.
  Duration getIdleTimeCompat() => getIdleTime();
}
