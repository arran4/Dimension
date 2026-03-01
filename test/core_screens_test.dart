import 'package:dimension/ui/core_screens.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('renders all primary tabs', (tester) async {
    final controller = CoreScreensController();
    for (final section in CoreScreenSection.values) {
      controller.setItems(section, const <String>[]);
    }

    await tester.pumpWidget(
      MaterialApp(home: CoreScreensView(controller: controller)),
    );

    expect(find.text('Circles'), findsOneWidget);
    expect(find.text('Peers'), findsOneWidget);
    expect(find.text('Chat'), findsOneWidget);
    expect(find.text('Search'), findsOneWidget);
    expect(find.text('Transfers'), findsOneWidget);
    expect(find.text('Settings'), findsOneWidget);
    expect(find.text('Diagnostics'), findsOneWidget);
  });

  testWidgets('shows loading/empty/error states across sections', (tester) async {
    final controller = CoreScreensController();
    controller.setLoading(CoreScreenSection.circles);
    controller.setItems(CoreScreenSection.peers, const <String>[]);
    controller.setError(CoreScreenSection.chat, 'chat failed');

    for (final section in CoreScreenSection.values) {
      if (section != CoreScreenSection.circles &&
          section != CoreScreenSection.peers &&
          section != CoreScreenSection.chat) {
        controller.setItems(section, const <String>['item']);
      }
    }

    await tester.pumpWidget(
      MaterialApp(home: CoreScreensView(controller: controller)),
    );

    expect(find.byType(CircularProgressIndicator), findsOneWidget);

    await tester.tap(find.text('Peers'));
    await tester.pumpAndSettle();
    expect(find.text('No peers connected.'), findsOneWidget);

    await tester.tap(find.text('Chat'));
    await tester.pumpAndSettle();
    expect(find.text('chat failed'), findsOneWidget);
  });
}
