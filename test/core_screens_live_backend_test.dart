import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/commands/hello_command.dart';
import 'package:dimension/model/commands/search_result_command.dart';
import 'package:dimension/model/commands/fs_listing.dart' as command_fs_listing;
import 'package:dimension/model/core.dart';
import 'package:dimension/model/incoming_connection.dart';
import 'package:dimension/ui/core_screens_live_backend.dart';
import 'package:flutter_test/flutter_test.dart';

class _Settings implements CoreSettingsStore {
  @override
  void setString(String key, String value) {}
}

class _Peer implements CorePeer {
  _Peer(this.id, this.quit);

  @override
  final int id;

  @override
  final bool quit;

  @override
  Set<int> circles = <int>{};

  final List<Command> sent = <Command>[];

  @override
  void sendCommand(Command command) {
    sent.add(command);
  }
}

class _PeerDirectory implements CorePeerDirectory {
  _PeerDirectory(this.peers);

  final List<_Peer> peers;

  @override
  Iterable<CorePeer> get allPeers => peers;

  @override
  Iterable<CorePeer> peersInCircle(int circleId) => peers;
}

class _DownloadDispatcher implements CoreScreensDownloadDispatcher {
  final List<String> queued = <String>[];

  @override
  Future<void> queueDownload(String itemName) async {
    queued.add(itemName);
  }
}

class _FakeIncoming extends IncomingConnection {
  @override
  bool get connected => true;

  @override
  bool rateLimiterDisabled = false;

  @override
  HelloCommand? hello;

  @override
  String? lastFolder;

  @override
  void Function(Command c, IncomingConnection con)? commandReceived;

  @override
  Future<void> send(Command c) async {}
}

void main() {
  test('live backend joins circle, refreshes peers, and dispatches queue',
      () async {
    final peers = <_Peer>[_Peer(1, false), _Peer(2, true), _Peer(3, false)];
    final core = Core(
      peerDirectory: _PeerDirectory(peers),
      settings: _Settings(),
      localPeerId: 7,
    );
    final dispatcher = _DownloadDispatcher();
    final backend = CoreScreensLiveBackend(
      core: core,
      peersProvider: () => peers,
      downloadDispatcher: dispatcher,
    );

    await backend.joinCircle('LAN');
    final peerRows = await backend.refreshPeers();
    await backend.queueDownload('movie.bin');

    expect(core.circles, contains('LAN'));
    expect(peerRows, <String>['Peer 1', 'Peer 3']);
    expect(dispatcher.queued, <String>['movie.bin']);
  });

  test('live backend search returns latest cached core results', () async {
    final peer = _Peer(1, false);
    final core = Core(
      peerDirectory: _PeerDirectory(<_Peer>[peer]),
      settings: _Settings(),
      localPeerId: 7,
    );

    final result = SearchResultCommand()..keyword = 'song';
    result.folders.add(command_fs_listing.FSListing()..name = 'Albums');
    result.files.add(command_fs_listing.FSListing()..name = 'song.mp3');

    final incoming = _FakeIncoming();
    core.addIncomingConnection(incoming);
    incoming.commandReceived?.call(result, incoming);

    final backend = CoreScreensLiveBackend(
      core: core,
      peersProvider: () => <CorePeer>[peer],
    );

    final rows = await backend.runSearch('song');

    expect(rows, <String>['Folder: Albums', 'File: song.mp3']);
    expect(peer.sent.single, isA<Command>());
  });
}
