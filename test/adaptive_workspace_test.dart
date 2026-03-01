import 'package:dimension/ui/adaptive_workspace.dart';
import 'package:dimension/ui/core_screens.dart';
import 'package:dimension/ui/platform_plan_infra.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  CoreScreensController seededController() {
    final controller = CoreScreensController();
    for (final section in CoreScreenSection.values) {
      controller.setItems(section, <String>['${section.name}-1']);
    }
    return controller;
  }

  testWidgets('renders compact bottom-tab workspace', (tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 420,
          child: AdaptiveWorkspace(
            planController: PlatformPlanController(),
            screensController: seededController(),
          ),
        ),
      ),
    );

    expect(find.text('Dimension Mobile'), findsOneWidget);
    expect(find.byType(TabBar), findsOneWidget);
  });

  testWidgets('renders medium rail workspace', (tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 900,
          child: AdaptiveWorkspace(
            planController: PlatformPlanController(),
            screensController: seededController(),
          ),
        ),
      ),
    );

    expect(find.text('Dimension Desktop'), findsOneWidget);
    expect(find.byType(NavigationRail), findsOneWidget);
  });

  testWidgets('renders expanded split-view workspace', (tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 1400,
          child: AdaptiveWorkspace(
            planController: PlatformPlanController(),
            screensController: seededController(),
          ),
        ),
      ),
    );

    expect(find.text('Dimension Web/Desktop Split'), findsOneWidget);
    expect(find.byType(ListTile), findsWidgets);
  });
}
