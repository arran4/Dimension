import 'dart:collection';

import 'commands/command.dart';
import 'commands/cancel_command.dart';
import 'commands/file_chunk.dart';
import 'commands/file_listing.dart';
import 'commands/get_file_listing.dart';
import 'commands/request_chunks.dart';
import 'commands/room_chat_command.dart';
import 'commands/search_command.dart';
import 'commands/search_result_command.dart';
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

abstract class CoreFileListingProvider {
  Future<FileListing?> generateFileListing(String path);
}

abstract class CoreSearchResultSink {
  void addSearchResult(
      SearchResultCommand command, IncomingConnection connection);
}

abstract class CoreTransferRouter {
  Future<void> handleFileChunk(
      FileChunk command, IncomingConnection connection);

  Future<void> handleRequestChunks(
    RequestChunks command,
    IncomingConnection connection,
  );
}

typedef CoreIdleTimeProvider = Duration Function();
typedef CoreChatReceivedHandler = void Function(
  String message,
  int roomId,
  CorePeer? peer,
);

enum CoreTransferEventType { chunkReceived, chunkRequested }

class CoreTransferEvent {
  const CoreTransferEvent({
    required this.type,
    required this.path,
    this.start,
    this.totalSize,
    this.allChunks,
  });

  final CoreTransferEventType type;
  final String path;
  final int? start;
  final int? totalSize;
  final bool? allChunks;
}

class Core {
  Core({
    required CorePeerDirectory peerDirectory,
    required CoreSettingsStore settings,
    required int localPeerId,
    CoreIdleTimeProvider? idleTimeProvider,
    CoreFileListingProvider? fileListingProvider,
    CoreSearchResultSink? searchResultSink,
    CoreTransferRouter? transferRouter,
  })  : _peerDirectory = peerDirectory,
        _settings = settings,
        _localPeerId = localPeerId,
        _idleTimeProvider = idleTimeProvider ?? (() => Duration.zero),
        _fileListingProvider = fileListingProvider,
        _searchResultSink = searchResultSink,
        _transferRouter = transferRouter;

  final CorePeerDirectory _peerDirectory;
  final CoreSettingsStore _settings;
  final int _localPeerId;
  final CoreIdleTimeProvider _idleTimeProvider;
  final CoreFileListingProvider? _fileListingProvider;
  final CoreSearchResultSink? _searchResultSink;
  final CoreTransferRouter? _transferRouter;

  final Set<String> _cancelledPaths = <String>{};
  final List<SearchResultCommand> _searchResults = <SearchResultCommand>[];
  final Map<String, SearchResultCommand> _searchResultsByKeyword =
      <String, SearchResultCommand>{};
  final Map<String, CoreTransferEvent> _latestTransferEvents =
      <String, CoreTransferEvent>{};

  bool disposed = false;
  int incomingTcpConnections = 0;
  int incomingUdtConnections = 0;

  final Set<String> _circles = <String>{};
  final List<IncomingConnection> _incomings = <IncomingConnection>[];
  final List<CoreChatReceivedHandler> _chatListeners =
      <CoreChatReceivedHandler>[];

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

  bool isPathCancelled(String path) => _cancelledPaths.contains(path);

  bool clearCancelledPath(String path) => _cancelledPaths.remove(path);

  UnmodifiableListView<SearchResultCommand> get searchResults =>
      UnmodifiableListView<SearchResultCommand>(_searchResults);

  SearchResultCommand? searchResultForKeyword(String keyword) {
    final normalized = keyword.trim().toLowerCase();
    if (normalized.isEmpty) {
      return null;
    }
    return _searchResultsByKeyword[normalized];
  }

  CoreTransferEvent? latestTransferEventForPath(String path) {
    final normalized = path.trim();
    if (normalized.isEmpty) {
      return null;
    }
    return _latestTransferEvents[normalized];
  }

  void _commandReceived(Command command, IncomingConnection connection) {
    if (command is CancelCommand) {
      _cancelledPaths.add(command.path);
      return;
    }

    if (command is GetFileListing) {
      connection.lastFolder = command.path;
      final provider = _fileListingProvider;
      if (provider == null) {
        return;
      }

      provider.generateFileListing(command.path).then((listing) {
        if (listing != null) {
          connection.send(listing);
        }
      });
      return;
    }

    if (command is SearchResultCommand) {
      _searchResults.add(command);
      final normalizedKeyword = command.keyword.trim().toLowerCase();
      if (normalizedKeyword.isNotEmpty) {
        _searchResultsByKeyword[normalizedKeyword] = command;
      }
      _searchResultSink?.addSearchResult(command, connection);
      return;
    }

    if (command is FileChunk) {
      final event = CoreTransferEvent(
        type: CoreTransferEventType.chunkReceived,
        path: command.path.trim(),
        start: command.start,
        totalSize: command.totalSize,
      );
      if (event.path.isNotEmpty) {
        _latestTransferEvents[event.path] = event;
      }
      _transferRouter?.handleFileChunk(command, connection);
      return;
    }

    if (command is RequestChunks) {
      final event = CoreTransferEvent(
        type: CoreTransferEventType.chunkRequested,
        path: command.path.trim(),
        start: command.startingByte,
        allChunks: command.allChunks,
      );
      if (event.path.isNotEmpty) {
        _latestTransferEvents[event.path] = event;
      }
      _transferRouter?.handleRequestChunks(command, connection);
      return;
    }

    // Remaining parity work: wire command-side effects to final transfer/search
    // orchestration adapters when App/Core runtime bootstrap is completed.
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
