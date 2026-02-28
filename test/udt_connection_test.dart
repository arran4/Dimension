import 'dart:async';
import 'dart:typed_data';

import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/serializer.dart';
import 'package:dimension/model/udt_connection.dart';
import 'package:flutter_test/flutter_test.dart';

class _TestCommand extends Command {
  _TestCommand(this.value);

  final String value;
}

class _FakeTransport implements UdtTransport {
  final StreamController<Uint8List> _incomingController =
      StreamController<Uint8List>.broadcast();
  final List<Uint8List> sentPackets = <Uint8List>[];

  @override
  Stream<Uint8List> get incomingPackets => _incomingController.stream;

  @override
  bool isConnected = true;

  @override
  bool isConnecting = false;

  @override
  Future<void> close() async {
    isConnected = false;
    await _incomingController.close();
  }

  @override
  Future<void> send(Uint8List packet) async {
    sentPackets.add(packet);
  }

  Future<void> emit(Uint8List packet) => _incomingController.add(packet);
}

void main() {
  Serializer buildSerializer() {
    return Serializer()
      ..register<_TestCommand>(
        typeName: 'TestCommand',
        encode: (command) => <String, dynamic>{'value': command.value},
        decode: (json) => _TestCommand(json['value'] as String),
      );
  }

  test('UdtOutgoingConnection send serializes through transport', () async {
    final serializer = buildSerializer();
    final transport = _FakeTransport();
    final connection = UdtOutgoingConnection(
      transport: transport,
      serializer: serializer,
    );

    await connection.send(_TestCommand('ping'));

    expect(transport.sentPackets, hasLength(1));
    final decoded = serializer.deserialize(transport.sentPackets.single);
    expect(decoded, isA<_TestCommand>());
    expect((decoded as _TestCommand).value, 'ping');
  });

  test('UdtOutgoingConnection forwards inbound commands to callback', () async {
    final serializer = buildSerializer();
    final transport = _FakeTransport();
    final completer = Completer<_TestCommand>();
    final connection = UdtOutgoingConnection(
      transport: transport,
      serializer: serializer,
    );

    connection.commandReceived = (command) {
      if (!completer.isCompleted && command is _TestCommand) {
        completer.complete(command);
      }
    };

    await transport.emit(serializer.serialize(_TestCommand('inbound')));

    final received = await completer.future;
    expect(received.value, 'inbound');
  });

  test('UdtIncomingConnection forwards inbound commands to callback', () async {
    final serializer = buildSerializer();
    final transport = _FakeTransport();
    final completer = Completer<_TestCommand>();
    final connection = UdtIncomingConnection(
      transport: transport,
      serializer: serializer,
    );

    connection.commandReceived = (command, _) {
      if (!completer.isCompleted && command is _TestCommand) {
        completer.complete(command);
      }
    };

    await transport.emit(serializer.serialize(_TestCommand('hello')));

    final received = await completer.future;
    expect(received.value, 'hello');
  });
}

