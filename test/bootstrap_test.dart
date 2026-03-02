import 'dart:io';

import 'package:dimension/model/bootstrap.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('Bootstrap.join', () {
    test('appends control port query parameter and parses valid rows', () async {
      final fakeHttpClient = _FakeBootstrapHttpClient(
        body: '203.0.113.10 1234\n203.0.113.10 1234\n198.51.100.7 99\n',
      );
      final bootstrap = Bootstrap(httpClient: fakeHttpClient);
      bootstrap.publicControlPort = 4321;

      final endpoints =
          await bootstrap.join('https://example.com/bootstrap.php');

      expect(
        fakeHttpClient.requestedUris.single,
        Uri.parse('https://example.com/bootstrap.php?port=4321'),
      );
      expect(
        endpoints,
        <InternetAddressEndpoint>[
          InternetAddressEndpoint(InternetAddress('203.0.113.10'), 1234),
          InternetAddressEndpoint(InternetAddress('198.51.100.7'), 99),
        ],
      );
    });

    test('keeps existing query params and ignores invalid rows', () async {
      final fakeHttpClient = _FakeBootstrapHttpClient(
        body: 'bad-data\nnot.an.ip 100\n203.0.113.4 bad\n203.0.113.5 8080\n',
      );
      final bootstrap = Bootstrap(httpClient: fakeHttpClient);

      final endpoints = await bootstrap.join(
        'https://example.com/bootstrap.php?token=abc',
      );

      expect(
        fakeHttpClient.requestedUris.single,
        Uri.parse('https://example.com/bootstrap.php?token=abc'),
      );
      expect(
        endpoints,
        <InternetAddressEndpoint>[
          InternetAddressEndpoint(InternetAddress('203.0.113.5'), 8080),
        ],
      );
    });
  });

  group('Bootstrap NAT launch', () {
    test('launch uses STUN/public address and detects double NAT mismatch', () async {
      final bootstrap = Bootstrap(
        natAdapter: _NatAdapter(
          BootstrapNatProbeResult(
            upnpActive: true,
            externalIp: InternetAddress('10.0.0.5'),
            publicControlPort: 1111,
          ),
        ),
        stunClient: _StunClient(
          BootstrapStunResult(
            publicAddress: InternetAddress('203.0.113.20'),
            publicControlPort: 2222,
          ),
        ),
      );

      await bootstrap.launch();

      expect(bootstrap.upnpActive, isTrue);
      expect(bootstrap.externalIpFromUpnp?.address, '10.0.0.5');
      expect(bootstrap.publicAddress?.address, '203.0.113.20');
      expect(bootstrap.publicControlPort, 2222);
      expect(bootstrap.behindDoubleNAT, isTrue);
    });

    test('launch falls back to UPnP address when STUN unavailable', () async {
      final bootstrap = Bootstrap(
        natAdapter: _NatAdapter(
          BootstrapNatProbeResult(
            upnpActive: true,
            externalIp: InternetAddress('198.51.100.10'),
            publicControlPort: 3333,
          ),
        ),
        stunClient: _StunClient(null),
      );

      await bootstrap.launch();

      expect(bootstrap.publicAddress?.address, '198.51.100.10');
      expect(bootstrap.publicControlPort, 3333);
      expect(bootstrap.behindDoubleNAT, isFalse);
    });
  });

  group('Bootstrap logging and disposal shims', () {
    test('Write and WriteLine delegate to logger and Dispose marks disposed', () {
      final messages = <String>[];
      final bootstrap = Bootstrap(logger: messages.add);

      bootstrap.Write('first');
      bootstrap.WriteLine('second');
      bootstrap.Dispose();

      expect(messages, <String>['first', 'second']);
      expect(bootstrap.disposed, isTrue);
    });
  });
}

class _FakeBootstrapHttpClient implements BootstrapHttpClient {
  _FakeBootstrapHttpClient({required this.body});

  final String body;
  final List<Uri> requestedUris = <Uri>[];

  @override
  Future<String> get(Uri uri) async {
    requestedUris.add(uri);
    return body;
  }
}

class _NatAdapter implements BootstrapNatAdapter {
  _NatAdapter(this.result);

  final BootstrapNatProbeResult result;

  @override
  Future<BootstrapNatProbeResult> probe() async => result;
}

class _StunClient implements BootstrapStunClient {
  _StunClient(this.result);

  final BootstrapStunResult? result;

  @override
  Future<BootstrapStunResult?> probe() async => result;
}
