import 'package:dimension/model/incoming_connection.dart';
import 'package:dimension/model/commands/command.dart';
import 'package:dimension/model/transfer.dart';
import 'package:dimension/ui/transfers_panel.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

class _FakeIncomingConnection extends IncomingConnection {
  @override
  bool get connected => true;

  @override
  Future<void> send(Command c) async {}
}

void main() {
  test('logic maps transfer fields and computes rate/limit/eta', () {
    final transfer = Transfer()
      ..filename = 'movie.mkv'
      ..download = true
      ..username = 'alice'
      ..rate = 1024
      ..size = 4096
      ..startingByte = 0
      ..completed = 2048
      ..timeCreated = DateTime(2026, 1, 1, 0, 0, 0)
      ..protocol = 'UDT';

    final rows = const TransfersPanelLogic().buildRows(
      transfers: [transfer],
      limitProvider: InMemoryTransferLimitProvider(
        uploadLimitBytesPerSecond: 0,
        downloadLimitBytesPerSecond: 4096,
      ),
      now: DateTime(2026, 1, 1, 0, 0, 10),
    );

    expect(rows, hasLength(1));
    expect(rows.first.direction, 'Downloading');
    expect(rows.first.speed, '1024.0B/s');
    expect(rows.first.limit, '4.0KB/s');
    expect(rows.first.percent, '50%');
    expect(rows.first.eta, '00:00:10');
  });

  test('logic reports bypassed limiter from incoming connection', () {
    final connection = _FakeIncomingConnection()..rateLimiterDisabled = true;
    final transfer = Transfer()
      ..filename = 'a.bin'
      ..con = connection
      ..size = 100
      ..completed = 20;

    final rows = const TransfersPanelLogic().buildRows(
      transfers: [transfer],
      limitProvider: InMemoryTransferLimitProvider(
        uploadLimitBytesPerSecond: 100,
      ),
    );

    expect(rows.first.limit, 'Bypassed');
  });

  testWidgets('panel shows empty message and then list content', (tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: TransfersPanel(
            transfers: const [],
            limitProvider: InMemoryTransferLimitProvider(),
            isMono: false,
          ),
        ),
      ),
    );

    expect(find.text('No active transfers.'), findsOneWidget);

    final populatedTransfer = Transfer()
      ..filename = 'song.mp3'
      ..download = false
      ..username = 'bob'
      ..size = 1024
      ..completed = 512;

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: TransfersPanel(
            transfers: [populatedTransfer],
            limitProvider: InMemoryTransferLimitProvider(),
            isMono: true,
          ),
        ),
      ),
    );

    expect(find.text('song.mp3'), findsOneWidget);
    expect(find.textContaining('Uploading'), findsOneWidget);
  });
}
