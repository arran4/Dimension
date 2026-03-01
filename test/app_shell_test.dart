import 'package:dimension/ui/app_shell.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('breakpointForWidth returns compact/medium/expanded buckets', () {
    expect(breakpointForWidth(320), AppBreakpoint.compact);
    expect(breakpointForWidth(800), AppBreakpoint.medium);
    expect(breakpointForWidth(1400), AppBreakpoint.expanded);
  });

  test('AppRouteState normalizes missing leading slash', () {
    final route = AppRouteState.fromLocation('settings/profile');
    expect(route.path, '/settings/profile');
  });

  test('AppRouteState parses query and fragment and serializes location', () {
    final route = AppRouteState.fromLocation('/search?q=dimension#results');

    expect(route.path, '/search');
    expect(route.queryParameters['q'], 'dimension');
    expect(route.fragment, 'results');
    expect(route.toLocation(), '/search?q=dimension#results');
  });

  test('AppShellController keeps history for back/forward navigation', () {
    final controller = AppShellController();

    controller.navigateTo('/search?q=abc');
    controller.navigateTo('/transfers');

    expect(controller.route.path, '/transfers');
    expect(controller.canGoBack, isTrue);

    controller.goBack();
    expect(controller.route.path, '/search');
    expect(controller.route.queryParameters['q'], 'abc');

    controller.goForward();
    expect(controller.route.path, '/transfers');
  });

  test('AppShellController can restore persisted location from route store', () async {
    final store = InMemoryAppRouteStateStore();
    await store.saveLocation('/settings?tab=network');
    final controller = AppShellController(routeStateStore: store);

    await controller.restoreRouteState();

    expect(controller.route.path, '/settings');
    expect(controller.route.queryParameters['tab'], 'network');
  });


  testWidgets('AppShell applies web input wrappers for focus/scroll/selection', (
    tester,
  ) async {
    final controller = AppShellController();

    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 1000,
          child: AppShell(
            controller: controller,
            contentBuilder: (context, breakpoint, route) {
              return TextField(
                key: const Key('webInputField'),
                controller: TextEditingController(text: 'copy me'),
              );
            },
          ),
        ),
      ),
    );

    expect(find.byType(AppShellWebInputWrapper), findsOneWidget);
    expect(find.byType(FocusTraversalGroup), findsOneWidget);
    expect(find.byType(SelectionArea), findsOneWidget);
    expect(find.byType(ScrollConfiguration), findsWidgets);
    expect(find.byKey(const Key('webInputField')), findsOneWidget);
  });

  testWidgets('AppShell rebuilds for route and layout changes', (tester) async {
    final controller = AppShellController();

    await tester.pumpWidget(
      MaterialApp(
        theme: DimensionTheme.light(),
        darkTheme: DimensionTheme.dark(),
        home: SizedBox(
          width: 1200,
          child: AppShell(
            controller: controller,
            contentBuilder: (context, breakpoint, route) {
              return Text('${breakpoint.name}:${route.path}');
            },
          ),
        ),
      ),
    );

    expect(find.text('expanded:/'), findsOneWidget);

    controller.navigateTo('/transfers');
    await tester.pump();

    expect(find.text('expanded:/transfers'), findsOneWidget);
  });
}
