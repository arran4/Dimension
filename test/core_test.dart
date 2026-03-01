import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/commands/cancel_command.dart';
import 'package:dimension/model/commands/file_listing.dart';
import 'package:dimension/model/commands/get_file_listing.dart';
import 'package:dimension/model/commands/search_command.dart';
import 'package:dimension/model/core.dart';
import 'package:dimension/model/incoming_connection.dart';
import 'package:flutter_test/flutter_test.dart';

class _Settings implements CoreSettingsStore {
  final Map<String, String> values = <String, String>{};

  @override
  void setString(String key, String value) {
    values[key] = value;
  }
}

class _Peer implements CorePeer {
  _Peer({
    required this.id,
    required this.quit,
    required Set<int> circles,
  }) : circles = circles;

  @override
  final int id;

  @override
  final bool quit;

  @override
  final Set<int> circles;

  final List<Object> sentCommands = <Object>[];

  @override
  void sendCommand(Command command) {
    sentCommands.add(command);
  }
}

class _PeerDirectory implements CorePeerMutableDirectory {
  _PeerDirectory(this._peers);

  final List<_Peer> _peers;

  @override
  Iterable<CorePeer> get allPeers => _peers;

  @override
  Iterable<CorePeer> peersInCircle(int circleId) {
    return _peers.where((peer) => peer.circles.contains(circleId));
  }

  @override
  bool addPeer(CorePeer peer) {
    if (peer is! _Peer || _peers.any((existing) => existing.id == peer.id)) {
      return false;
    }
    _peers.add(peer);
    return true;
  }
}

class _SearchCommand extends SearchCommand {}



class _FileListingProvider implements CoreFileListingProvider {
  String? requestedPath;

  @override
  Future<FileListing?> generateFileListing(String path) async {
    requestedPath = path;
    return FileListing()..path = path;
  }
}

class _Incoming extends IncomingConnection {
  final List<Command> sent = <Command>[];

  @override
  bool get connected => true;

  @override
  Future<void> send(Command c) async {
    sent.add(c);
  }
}

class _ReadOnlyPeerDirectory implements CorePeerDirectory {
  _ReadOnlyPeerDirectory(this._peers);

  final List<_Peer> _peers;

  @override
  Iterable<CorePeer> get allPeers => _peers;

  @override
  Iterable<CorePeer> peersInCircle(int circleId) {
    return _peers.where((peer) => peer.circles.contains(circleId));
  }
}

void main() {
  test('beginSearch broadcasts only to non-quit peers', () {
    final peer1 = _Peer(id: 1, quit: false, circles: {11});
    final peer2 = _Peer(id: 2, quit: true, circles: {11});

    final core = Core(
      peerDirectory: _PeerDirectory([peer1, peer2]),
      settings: _Settings(),
      localPeerId: 9,
    );

    core.beginSearch(_SearchCommand());

    expect(peer1.sentCommands.length, 1);
    expect(peer2.sentCommands, isEmpty);
  });

  test('addPeer delegates to mutable directories and deduplicates ids', () {
    final directory = _PeerDirectory([_Peer(id: 1, quit: false, circles: {7})]);
    final core = Core(
      peerDirectory: directory,
      settings: _Settings(),
      localPeerId: 9,
    );

    final added = core.addPeer(_Peer(id: 2, quit: false, circles: {7}));
    final duplicate = core.addPeer(_Peer(id: 1, quit: false, circles: {7}));

    expect(added, isTrue);
    expect(duplicate, isFalse);
    expect(directory.allPeers.length, 2);
  });

  test('addPeer returns false when directory is read-only', () {
    final core = Core(
      peerDirectory: _ReadOnlyPeerDirectory([_Peer(id: 1, quit: false, circles: {7})]),
      settings: _Settings(),
      localPeerId: 9,
    );

    expect(core.addPeer(_Peer(id: 2, quit: false, circles: {7})), isFalse);
  });

  test('sendChat handles /nick locally and clips username', () {
    final settings = _Settings();
    final peer = _Peer(id: 1, quit: false, circles: {77});
    final core = Core(
      peerDirectory: _PeerDirectory([peer]),
      settings: settings,
      localPeerId: 9,
    );

    core.sendChat('/nick super-long-username-value', 77);

    expect(settings.values['Username'], 'super-long-userna');
    expect(peer.sentCommands, isEmpty);
  });

  test('sendChat dispatches room message to peers in circle', () {
    final peerIn = _Peer(id: 1, quit: false, circles: {77});
    final peerOut = _Peer(id: 2, quit: false, circles: {42});
    final core = Core(
      peerDirectory: _PeerDirectory([peerIn, peerOut]),
      settings: _Settings(),
      localPeerId: 999,
    );

    core.sendChat('hello world', 77);

    expect(peerIn.sentCommands.length, 1);
    expect(peerOut.sentCommands, isEmpty);
  });

  test('commandReceived handles CancelCommand cancellation tracking', () async {
    final incoming = _Incoming();
    final core = Core(
      peerDirectory: _PeerDirectory([]),
      settings: _Settings(),
      localPeerId: 1,
    );

    core.addIncomingConnection(incoming);
    incoming.commandReceived?.call(CancelCommand()..path = '/x.bin', incoming);

    expect(core.isPathCancelled('/x.bin'), isTrue);
    expect(core.clearCancelledPath('/x.bin'), isTrue);
    expect(core.isPathCancelled('/x.bin'), isFalse);
  });

  test('commandReceived handles GetFileListing via injected provider', () async {
    final incoming = _Incoming();
    final provider = _FileListingProvider();
    final core = Core(
      peerDirectory: _PeerDirectory([]),
      settings: _Settings(),
      localPeerId: 1,
      fileListingProvider: provider,
    );

    core.addIncomingConnection(incoming);
    incoming.commandReceived?.call(GetFileListing()..path = '/Public', incoming);
    await Future<void>.delayed(Duration.zero);

    expect(provider.requestedPath, '/Public');
    expect(incoming.lastFolder, '/Public');
    expect(incoming.sent.single, isA<FileListing>());
    expect((incoming.sent.single as FileListing).path, '/Public');
  });

  test('chatReceived invokes listeners and getIdleTime uses injected provider', () {
    final core = Core(
      peerDirectory: _PeerDirectory([]),
      settings: _Settings(),
      localPeerId: 1,
      idleTimeProvider: () => const Duration(seconds: 90),
    );

    String? line;
    core.addChatReceivedListener((message, roomId, peer) {
      line = '$roomId:$message';
    });

    core.chatReceived('ping', 5, null);

    expect(line, '5:ping');
    expect(core.getIdleTime(), const Duration(seconds: 90));
  });
}
