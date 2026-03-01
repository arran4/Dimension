import 'package:dimension/ui/adaptive_workspace.dart';
import 'package:dimension/ui/core_screens.dart';
import 'package:dimension/ui/desktop_shell_infra.dart';
import 'package:dimension/ui/desktop_window_state.dart';
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



  testWidgets('compact landscape touch snapshot still renders mobile tabs', (
    tester,
  ) async {
    final plan = PlatformPlanController(
      targetPlatformOverride: TargetPlatform.iOS,
      isWebOverride: false,
    );

    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 844,
          height: 390,
          child: AdaptiveWorkspace(
            planController: plan,
            screensController: seededController(),
          ),
        ),
      ),
    );

    expect(find.textContaining('Dimension Mobile'), findsOneWidget);
    expect(find.byType(TabBar), findsOneWidget);
    expect(find.byType(NavigationRail), findsNothing);
  });

  testWidgets('mobile workspace applies safe-area and refresh affordances', (
    tester,
  ) async {
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

    expect(find.byType(SafeArea), findsOneWidget);
    expect(find.byType(RefreshIndicator), findsWidgets);
    expect(find.byKey(const Key('mobileRefreshFab')), findsOneWidget);
  });


  testWidgets('mobile keyboard inset updates animated padding', (tester) async {
    await tester.pumpWidget(
      MediaQuery(
        data: const MediaQueryData(viewInsets: EdgeInsets.only(bottom: 180)),
        child: MaterialApp(
          home: SizedBox(
            width: 420,
            child: AdaptiveWorkspace(
              planController: PlatformPlanController(),
              screensController: seededController(),
            ),
          ),
        ),
      ),
    );

    final animatedPadding = tester.widget<AnimatedPadding>(
      find.byKey(const Key('mobileKeyboardInsetPadding')),
    );
    expect(animatedPadding.padding, const EdgeInsets.only(bottom: 180));
  });

  testWidgets('mobile compact workspace does not overflow at very small height', (
    tester,
  ) async {
    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 320,
          height: 420,
          child: AdaptiveWorkspace(
            planController: PlatformPlanController(
              targetPlatformOverride: TargetPlatform.android,
              isWebOverride: false,
            ),
            screensController: seededController(),
          ),
        ),
      ),
    );

    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
    expect(find.byType(TabBar), findsOneWidget);
  });

  testWidgets('mobile bottom tabs meet minimum touch target height', (
    tester,
  ) async {
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

    final circlesFinder = find.text('Circles').last;
    final circlesSize = tester.getSize(circlesFinder);
    expect(circlesSize.height, greaterThanOrEqualTo(48));
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
    expect(find.byKey(const Key('desktopStatusBar')), findsOneWidget);
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

  testWidgets('desktop view shows resizable dense table for peers', (
    tester,
  ) async {
    final controller = seededController();
    final shell = DesktopShellController();
    shell.activateSection(CoreScreenSection.peers);

    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 1000,
          child: AdaptiveWorkspace(
            planController: PlatformPlanController(
              initialSnapshot: const PlatformLayoutSnapshot(
                mode: PlatformLayoutMode.medium,
                navigationPattern: NavigationPattern.rail,
                capabilities: PlatformCapabilities(
                  isWeb: false,
                  supportsHover: true,
                  supportsKeyboardShortcuts: true,
                  prefersTouch: false,
                ),
              ),
            ),
            screensController: controller,
            desktopShellController: shell,
            desktopWindowStateController: DesktopWindowStateController(
              store: InMemoryDesktopWindowStateStore(),
            ),
          ),
        ),
      ),
    );

    await tester.tap(find.text('Peers'));
    await tester.pumpAndSettle();

    expect(find.byKey(const Key('desktopColumnResizer')), findsOneWidget);
    expect(find.byKey(const Key('desktopWindowGeometryLabel')), findsOneWidget);
  });

  testWidgets('desktop shell controller hover text appears in status bar', (
    tester,
  ) async {
    final shell = DesktopShellController();

    await tester.pumpWidget(
      MaterialApp(
        home: SizedBox(
          width: 1000,
          child: AdaptiveWorkspace(
            planController: PlatformPlanController(),
            screensController: seededController(),
            desktopShellController: shell,
          ),
        ),
      ),
    );

    shell.onSectionHover(CoreScreenSection.transfers);
    await tester.pump();

    expect(find.textContaining('Hovering transfers'), findsOneWidget);
  });
}
