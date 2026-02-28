import 'dart:convert';
import 'dart:io';

import 'package:crypto/crypto.dart';

abstract class KademliaBackend {
  Future<void> initialize();
  Future<void> dispose();
  Future<void> announce({required List<int> keyHash, required int port});
  Future<List<InternetAddressEndpoint>> lookup({required List<int> keyHash});
}

class Kademlia {
  Kademlia({required KademliaBackend backend}) : _backend = backend;

  final KademliaBackend _backend;
  bool ready = false;
  bool _disposed = false;

  Future<void> initialize() async {
    if (_disposed) {
      return;
    }
    await _backend.initialize();
    ready = true;
  }

  Future<void> announce(String key, {required int publicControlPort}) async {
    if (_disposed) {
      return;
    }
    await _backend.announce(
      keyHash: _hashKey(key),
      port: publicControlPort,
    );
  }

  Future<List<InternetAddressEndpoint>> doLookup(String key) async {
    if (_disposed) {
      return const [];
    }

    final results = await _backend.lookup(keyHash: _hashKey(key));
    final deduped = <String, InternetAddressEndpoint>{};
    for (final endpoint in results) {
      deduped['${endpoint.address.address}:${endpoint.port}'] = endpoint;
    }
    return deduped.values.toList(growable: false);
  }

  Future<void> dispose() async {
    if (_disposed) {
      return;
    }
    _disposed = true;
    await _backend.dispose();
  }

  List<int> _hashKey(String key) {
    final hash = sha512.convert(utf8.encode(key.toLowerCase()));
    return hash.bytes.sublist(0, 20);
  }
}
