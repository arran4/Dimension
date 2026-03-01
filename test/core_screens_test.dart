import 'dart:async';

import 'package:dimension/ui/core_screens.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

class _Backend implements CoreScreensBackend {
  bool failJoin = false;
  bool failQueue = false;

  @override
  Future<void> joinCircle(String name) async {
    if (failJoin) {
      throw StateError('join failed');
    }
  }

  @override
  Future<void> queueDownload(String itemName) async {
    if (failQueue) {
      throw StateError('queue failed');
    }
  }

  @override
  Future<List<String>> refreshPeers() async => const <String>['alice', 'bob'];

  @override
  Future<List<String>> runSearch(String query) async => <String>['$query.bin'];
}

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

  test('optimistic circle join succeeds and keeps item', () async {
    final controller = CoreScreensController(backend: _Backend());
    controller.setItems(CoreScreenSection.circles, const <String>[]);

    await controller.joinCircle('LAN');

    expect(controller.stateFor(CoreScreenSection.circles).items, contains('LAN'));
    expect(controller.sectionMessage(CoreScreenSection.circles), 'Joined LAN');
    expect(controller.sectionBusy(CoreScreenSection.circles), isFalse);
  });

  test('optimistic circle join rolls back on failure', () async {
    final backend = _Backend()..failJoin = true;
    final controller = CoreScreensController(backend: backend);
    controller.setItems(CoreScreenSection.circles, const <String>['Existing']);

    await controller.joinCircle('LAN');

    expect(controller.stateFor(CoreScreenSection.circles).items, <String>['Existing']);
    expect(
      controller.sectionMessage(CoreScreenSection.circles),
      'Failed to join LAN',
    );
  });

  test('search and peer refresh use backend results', () async {
    final controller = CoreScreensController(backend: _Backend());

    await controller.refreshPeers();
    await controller.runSearch('movie');

    expect(controller.stateFor(CoreScreenSection.peers).items, <String>['alice', 'bob']);
    expect(controller.stateFor(CoreScreenSection.search).items, <String>['movie.bin']);
  });

  testWidgets('status feedback is shown in UI after action', (tester) async {
    final controller = CoreScreensController(backend: _Backend());
    for (final section in CoreScreenSection.values) {
      controller.setItems(section, const <String>[]);
    }

    await tester.pumpWidget(
      MaterialApp(home: CoreScreensView(controller: controller)),
    );

    await tester.tap(find.text('Join LAN'));
    await tester.pumpAndSettle();

    expect(find.byKey(const Key('core-screen-status.Circles')), findsOneWidget);
    expect(find.text('Joined LAN'), findsOneWidget);
    expect(find.text('LAN'), findsOneWidget);
  });
}
