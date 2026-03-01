import 'dart:async';

import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/serializer.dart';
import 'package:dimension/model/udt_backend.dart';
import 'package:flutter_test/flutter_test.dart';

class _TestCommand extends Command {
  _TestCommand(this.value);
  final String value;
}

Serializer _buildSerializer() {
  return Serializer()
    ..register<_TestCommand>(
      typeName: 'TestCommand',
      encode: (command) => <String, dynamic>{'value': command.value},
      decode: (json) => _TestCommand(json['value'] as String),
    );
}

void main() {
  test('in-memory backend connects to registered listener', () async {
    final backend = InMemoryUdtBackend();
    final endpoint = const UdtEndpoint(host: '127.0.0.1', port: 9000);

    final listenerTransport = backend.registerListener(endpoint);
    final clientTransport = await backend.connect(endpoint);

    expect(listenerTransport.isConnected, isTrue);
    expect(clientTransport.isConnected, isTrue);
  });

  test('connection factory sends command through in-memory transports', () async {
    final backend = InMemoryUdtBackend();
    final serializer = _buildSerializer();
    final endpoint = const UdtEndpoint(host: 'localhost', port: 7001);
    final listenerTransport = backend.registerListener(endpoint);

    final factory = UdtConnectionFactory(backend: backend, serializer: serializer);
    final incoming = factory.createIncoming(listenerTransport);
    final outgoing = await factory.createOutgoing(endpoint);

    final completer = Completer<String>();
    incoming.commandReceived = (command, _) {
      if (command is _TestCommand && !completer.isCompleted) {
        completer.complete(command.value);
      }
    };

    await outgoing.send(_TestCommand('hello-udt'));

    expect(await completer.future, 'hello-udt');
  });
}
