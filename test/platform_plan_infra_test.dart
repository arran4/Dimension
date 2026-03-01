import 'package:dimension/ui/platform_plan_infra.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('infers compact mobile layout with bottom tabs', () {
    final snapshot = inferPlatformLayout(
      width: 390,
      isWebOverride: false,
      targetPlatformOverride: TargetPlatform.android,
    );

    expect(snapshot.mode, PlatformLayoutMode.compact);
    expect(snapshot.navigationPattern, NavigationPattern.bottomTabs);
    expect(snapshot.capabilities.prefersTouch, isTrue);
    expect(snapshot.capabilities.supportsHover, isFalse);
  });

  test('infers medium/expanded desktop capabilities', () {
    final medium = inferPlatformLayout(
      width: 900,
      isWebOverride: false,
      targetPlatformOverride: TargetPlatform.windows,
    );
    final expanded = inferPlatformLayout(
      width: 1440,
      isWebOverride: false,
      targetPlatformOverride: TargetPlatform.windows,
    );

    expect(medium.mode, PlatformLayoutMode.medium);
    expect(medium.navigationPattern, NavigationPattern.rail);
    expect(medium.capabilities.supportsKeyboardShortcuts, isTrue);
    expect(expanded.mode, PlatformLayoutMode.expanded);
    expect(expanded.navigationPattern, NavigationPattern.splitView);
  });

  test('infers web hover + keyboard support', () {
    final snapshot = inferPlatformLayout(
      width: 1024,
      isWebOverride: true,
      targetPlatformOverride: TargetPlatform.iOS,
    );

    expect(snapshot.capabilities.isWeb, isTrue);
    expect(snapshot.capabilities.supportsHover, isTrue);
    expect(snapshot.capabilities.supportsKeyboardShortcuts, isTrue);
  });

  test('controller recompute updates snapshot', () {
    final controller = PlatformPlanController();

    controller.recompute(
      width: 460,
      isWebOverride: false,
      targetPlatformOverride: TargetPlatform.iOS,
    );

    expect(controller.snapshot.mode, PlatformLayoutMode.compact);
    expect(controller.snapshot.capabilities.prefersTouch, isTrue);
  });
}
