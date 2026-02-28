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
