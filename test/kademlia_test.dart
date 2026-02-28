import 'dart:io';

import 'package:dimension/model/kademlia.dart';
import 'package:flutter_test/flutter_test.dart';

class _Backend implements KademliaBackend {
  var initializeCalls = 0;
  var disposeCalls = 0;
  final announceCalls = <({List<int> keyHash, int port})>[];
  final lookupCalls = <List<int>>[];
  List<InternetAddressEndpoint> lookupResult = [];

  @override
  Future<void> announce({required List<int> keyHash, required int port}) async {
    announceCalls.add((keyHash: keyHash, port: port));
  }

  @override
  Future<void> dispose() async {
    disposeCalls++;
  }

  @override
  Future<void> initialize() async {
    initializeCalls++;
  }

  @override
  Future<List<InternetAddressEndpoint>> lookup({
    required List<int> keyHash,
  }) async {
    lookupCalls.add(keyHash);
    return lookupResult;
  }
}

void main() {
  test('initialize marks kademlia as ready', () async {
    final backend = _Backend();
    final kademlia = Kademlia(backend: backend);

    await kademlia.initialize();

    expect(backend.initializeCalls, 1);
    expect(kademlia.ready, isTrue);
  });

  test('announce forwards hashed key and control port', () async {
    final backend = _Backend();
    final kademlia = Kademlia(backend: backend);

    await kademlia.announce('My Circle', publicControlPort: 1234);

    expect(backend.announceCalls, hasLength(1));
    expect(backend.announceCalls.single.port, 1234);
    expect(backend.announceCalls.single.keyHash, hasLength(20));
  });

  test('doLookup deduplicates repeated endpoints', () async {
    final backend = _Backend()
      ..lookupResult = [
        InternetAddressEndpoint(InternetAddress('1.2.3.4'), 1111),
        InternetAddressEndpoint(InternetAddress('1.2.3.4'), 1111),
        InternetAddressEndpoint(InternetAddress('1.2.3.5'), 2222),
      ];
    final kademlia = Kademlia(backend: backend);

    final output = await kademlia.doLookup('My Circle');

    expect(backend.lookupCalls, hasLength(1));
    expect(output, hasLength(2));
  });
}
