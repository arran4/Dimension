import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/commands/private_chat_command.dart';
import 'package:dimension/model/commands/request_chunks.dart';
import 'package:dimension/model/commands/request_folder_contents.dart';
import 'package:dimension/model/commands/reverse_connection_type.dart';
import 'package:dimension/model/fs_listing.dart';
import 'package:dimension/model/outgoing_connection.dart';
import 'package:dimension/model/peer.dart';
import 'package:dimension/model/transfer.dart';
import 'package:flutter_test/flutter_test.dart';

class _FakeOutgoingConnection extends OutgoingConnection {
  final List<Command> sent = <Command>[];
  bool connectedValue = true;

  @override
  bool get connected => connectedValue;

  @override
  Future<void> send(Command c) async {
    sent.add(c);
  }
}

void main() {
  test('addEndpointToHistory stores unique endpoints', () {
    final peer = Peer();
    const endpoint = PeerEndpoint(address: '127.0.0.1', port: 8080);

    peer.addEndpointToHistory(endpoint);
    peer.addEndpointToHistory(endpoint);

    expect(peer.endpointIsInHistory(endpoint), isTrue);
  });

  test('endpoint history is capped to a small rolling window', () {
    final peer = Peer();
    for (var i = 0; i < 20; i++) {
      peer.addEndpointToHistory(PeerEndpoint(address: '127.0.0.$i', port: i));
    }

    expect(
      peer.endpointIsInHistory(
        const PeerEndpoint(address: '127.0.0.0', port: 0),
      ),
      isFalse,
    );
    expect(
      peer.endpointIsInHistory(
        const PeerEndpoint(address: '127.0.0.19', port: 19),
      ),
      isTrue,
    );
  });

  test('sendCommand queues when not connected and flushes on createConnection', () async {
    final peer = Peer();
    final queued = RequestChunks()..path = '/queued.bin';

    await peer.sendCommand(queued);
    expect(peer.queuedCommands, hasLength(1));

    final control = _FakeOutgoingConnection();
    await peer.createConnection(control: control);

    expect(peer.queuedCommands, isEmpty);
    expect(control.sent.single, queued);
  });

  test('downloadElement sends request commands based on listing type', () async {
    final peer = Peer();
    final control = _FakeOutgoingConnection();
    await peer.createConnection(control: control);

    final folder = FSListing()
      ..isFolder = true
      ..name = 'folder';
    final file = FSListing()
      ..isFolder = false
      ..name = 'file.bin';

    await peer.downloadElement(folder);
    await peer.downloadElement(file, startingByte: 9);

    expect(control.sent[0], isA<RequestFolderContents>());
    expect((control.sent[0] as RequestFolderContents).path, '/folder');
    expect(control.sent[1], isA<RequestChunks>());
    expect((control.sent[1] as RequestChunks).path, '/file.bin');
    expect((control.sent[1] as RequestChunks).startingByte, 9);
  });

  test('commandReceived routes private chat and emits generic callbacks', () {
    final peer = Peer();
    PrivateChatCommand? chat;
    Command? seen;
    peer.onPrivateChatReceived = (command) => chat = command;
    peer.onCommandReceived = (command) => seen = command;

    final command = PrivateChatCommand()..content = 'hello';
    peer.commandReceived(command);

    expect(chat?.content, 'hello');
    expect(seen, same(command));
  });

  test('reverseConnect emits reverse connection command', () async {
    final peer = Peer()..id = 77;
    final control = _FakeOutgoingConnection();
    await peer.createConnection(control: control);

    await peer.reverseConnect(makeControl: true, makeData: false);

    final sent = control.sent.single as ReverseConnectionType;
    expect(sent.id, 77);
    expect(sent.makeControl, isTrue);
    expect(sent.makeData, isFalse);
  });

  test('updateTransfers summarizes upload and download rates by peer id', () {
    final peer = Peer()..id = 7;
    final download = Transfer()
      ..userId = 7
      ..download = true
      ..rate = 123;
    final upload = Transfer()
      ..userId = 7
      ..download = false
      ..rate = 456;
    final other = Transfer()
      ..userId = 8
      ..download = true
      ..rate = 999;

    peer.updateTransfers([download, upload, other]);

    expect(peer.downloadRate, 123);
    expect(peer.uploadRate, 456);
  });

  test('endPunch and releasePunch clear punch state', () {
    final peer = Peer()..punchActive = true;

    peer.endPunch();
    expect(peer.punchActive, isFalse);

    peer.punchActive = true;
    peer.releasePunch();
    expect(peer.punchActive, isFalse);
  });
}
