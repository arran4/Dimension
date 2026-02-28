import 'package:dimension/ui/finished_transfers_panel.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('shows empty state', (tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(body: FinishedTransfersPanel(items: [])),
      ),
    );

    expect(find.text('No completed transfers yet.'), findsOneWidget);
  });

  testWidgets('renders completed transfers list', (tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(
          body: FinishedTransfersPanel(
            items: [
              FinishedTransferRow(name: 'song.mp3', status: 'Finished in 8s'),
              FinishedTransferRow(name: 'movie.mkv', status: 'Cancelled'),
            ],
          ),
        ),
      ),
    );

    expect(find.text('song.mp3'), findsOneWidget);
    expect(find.text('Finished in 8s'), findsOneWidget);
    expect(find.text('movie.mkv'), findsOneWidget);
  });
}
