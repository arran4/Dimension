import 'dart:async';
import 'dart:typed_data';

import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/serializer.dart';
import 'package:dimension/model/udt_connection.dart';
import 'package:dimension/model/udt_incoming_connection.dart' as legacy_incoming;
import 'package:dimension/model/udt_outgoing_connection.dart' as legacy_outgoing;
import 'package:flutter_test/flutter_test.dart';

class _FakeTransport implements UdtTransport {
  final StreamController<Uint8List> controller = StreamController<Uint8List>();
  final List<Uint8List> sentPackets = <Uint8List>[];

  @override
  Stream<Uint8List> get incomingPackets => controller.stream;

  @override
  bool get isConnected => true;

  @override
  bool get isConnecting => false;

  @override
  Future<void> close() async {
    await controller.close();
  }

  @override
  Future<void> send(Uint8List packet) async {
    sentPackets.add(packet);
  }
}

class _TestCommand extends Command {
  _TestCommand(this.value);

  final String value;
}

void main() {
  Serializer buildSerializer() {
    return Serializer()
      ..register<_TestCommand>(
        typeName: 'TestCommand',
        encode: (_TestCommand command) => <String, dynamic>{'value': command.value},
        decode: (Map<String, dynamic> json) => _TestCommand(json['value'] as String),
      );
  }

  test('legacy incoming shim exports the pure-Dart implementation', () async {
    final transport = _FakeTransport();
    final incoming = legacy_incoming.UdtIncomingConnection(
      transport: transport,
      serializer: buildSerializer(),
    );

    await incoming.send(_TestCommand('hello'));

    expect(transport.sentPackets, hasLength(1));
    await incoming.dispose();
  });

  test('legacy outgoing shim exports the pure-Dart implementation', () async {
    final transport = _FakeTransport();
    final outgoing = legacy_outgoing.UdtOutgoingConnection(
      transport: transport,
      serializer: buildSerializer(),
    );

    await outgoing.send(_TestCommand('hello'));

    expect(transport.sentPackets, hasLength(1));
    await outgoing.dispose();
  });
}
