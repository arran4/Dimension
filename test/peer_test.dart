import 'package:dimension/model/peer.dart';
import 'package:flutter_test/flutter_test.dart';

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
}
