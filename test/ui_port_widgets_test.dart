import 'package:dimension/ui/about_form.dart';
import 'package:dimension/ui/download_queue_panel.dart';
import 'package:dimension/ui/html_panel.dart';
import 'package:dimension/ui/rename_share_form.dart';
import 'package:dimension/ui/loading_form.dart';
import 'package:dimension/ui/network_status_panel.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

class _FakeLoadingStatusSource implements LoadingStatusSource {
  var startCalls = 0;
  var _done = false;

  @override
  bool get isDone => _done;

  @override
  String get statusLine => _done ? 'Ready' : 'Loading';

  @override
  Future<void> startLoading() async {
    startCalls++;
    _done = true;
  }
}

class _FakeNetworkStatusProvider implements NetworkStatusProvider {
  @override
  NetworkStatusSnapshot get snapshot =>
      const NetworkStatusSnapshot(
        ports: NetworkPortsInfo(
          internalIpAddresses: ['192.168.1.10'],
          externalIpAddress: '44.55.66.77',
          internalTcpPort: 1001,
          externalTcpPort: 2001,
          internalUdpPort: 1002,
          externalUdpPort: 2002,
          internalKademliaPort: 1003,
          externalKademliaPort: 2003,
        ),
        events: NetworkEventsInfo(
          udpCommandsNotFromUs: 8,
          incomingTcpConnections: 9,
          incomingUdtConnections: 10,
          successfulOutgoingTcpConnections: 11,
          successfulOutgoingUdtConnections: 12,
        ),
        incomingTrafficByCommand: {'HelloCommand': 2048},
        outgoingTrafficByCommand: {'SearchResultCommand': 3072},
        systemLog: 'system ok',
      );
}

void main() {
  testWidgets('about form renders and closes', (tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(
          body: AboutForm(
            applicationName: 'Dimension',
            versionLabel: '1.2.3',
            description: 'About text',
          ),
        ),
      ),
    );

    expect(find.text('About Dimension'), findsOneWidget);
    expect(find.text('1.2.3'), findsOneWidget);
    expect(find.text('About text'), findsOneWidget);
  });

  testWidgets('loading form starts once and triggers completion callback', (
    tester,
  ) async {
    final source = _FakeLoadingStatusSource();
    var finishedCalls = 0;

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: LoadingForm(
            statusSource: source,
            pollInterval: const Duration(milliseconds: 10),
            onFinished: () {
              finishedCalls++;
            },
          ),
        ),
      ),
    );

    await tester.pump(const Duration(milliseconds: 20));

    expect(source.startCalls, 1);
    expect(finishedCalls, 1);
    expect(find.textContaining('Dimension - Ready'), findsOneWidget);
  });

  test('network status formatter includes key values', () {
    final portsText = NetworkStatusFormatter.formatPorts(
      const NetworkPortsInfo(
        internalIpAddresses: ['127.0.0.1', '192.168.1.2'],
        externalIpAddress: '11.12.13.14',
        internalTcpPort: 999,
        externalTcpPort: 1999,
        internalUdpPort: 888,
        externalUdpPort: 1888,
        internalKademliaPort: 777,
        externalKademliaPort: 1777,
      ),
    );
    expect(portsText, contains('11.12.13.14'));
    expect(portsText, contains('External Kademlia (UDP) Port: 1777'));

    final trafficText = NetworkStatusFormatter.formatTraffic(
      incoming: const {'HelloCommand': 2048},
      outgoing: const {'SearchCommand': 4096},
    );
    expect(trafficText, contains('HelloCommand: 2.0KB'));
    expect(trafficText, contains('SearchCommand: 4.0KB'));
  });

  testWidgets('network status panel renders all sections', (tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: NetworkStatusPanel(provider: _FakeNetworkStatusProvider()),
        ),
      ),
    );

    expect(find.text('Ports'), findsOneWidget);
    expect(find.text('Events'), findsOneWidget);
    expect(find.text('Traffic'), findsOneWidget);
    expect(find.text('System Log'), findsOneWidget);
    expect(find.textContaining('system ok'), findsOneWidget);
  });

  test('parseDimensionJoinLink converts known schemes', () {
    final bootstrap = parseDimensionJoinLink(
      'DimensionBootstrap://www.example.com/bootstrap.php',
    );
    expect(bootstrap, isNotNull);
    expect(bootstrap!.url, 'http://www.example.com/bootstrap.php');
    expect(bootstrap.type, JoinCircleType.bootstrap);

    final lan = parseDimensionJoinLink('DimensionLAN://LAN');
    expect(lan, isNotNull);
    expect(lan!.url, 'http://LAN');
    expect(lan.type, JoinCircleType.lan);

    expect(parseDimensionJoinLink('https://example.com'), isNull);
  });

  testWidgets('html panel forwards join requests when links are tapped', (tester) async {
    JoinCircleRequest? request;

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: HTMLPanel(
            onJoinCircle: (value) {
              request = value;
            },
          ),
        ),
      ),
    );

    await tester.tap(find.text('Your local network'));
    await tester.pump();

    expect(request, isNotNull);
    expect(request!.type, JoinCircleType.lan);
    expect(request!.url, 'http://LAN');
  });

  testWidgets('rename share form returns entered value', (tester) async {
    String? result;

    await tester.pumpWidget(
      MaterialApp(
        home: Builder(
          builder: (context) => Scaffold(
            body: Center(
              child: FilledButton(
                onPressed: () async {
                  result = await RenameShareForm.show(
                    context,
                    initialName: 'Old Name',
                  );
                },
                child: const Text('Open'),
              ),
            ),
          ),
        ),
      ),
    );

    await tester.tap(find.text('Open'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextField), 'New Name');
    await tester.tap(find.text('OK'));
    await tester.pumpAndSettle();

    expect(result, 'New Name');
  });

  testWidgets('download queue panel shows empty message', (tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(body: DownloadQueuePanel()),
      ),
    );

    expect(find.text('No queued downloads.'), findsOneWidget);
  });

}
