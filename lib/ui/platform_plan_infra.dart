import 'package:flutter/foundation.dart';

enum PlatformLayoutMode { compact, medium, expanded }

enum NavigationPattern { bottomTabs, rail, splitView }

class PlatformCapabilities {
  const PlatformCapabilities({
    required this.isWeb,
    required this.supportsHover,
    required this.supportsKeyboardShortcuts,
    required this.prefersTouch,
  });

  final bool isWeb;
  final bool supportsHover;
  final bool supportsKeyboardShortcuts;
  final bool prefersTouch;
}

class PlatformLayoutSnapshot {
  const PlatformLayoutSnapshot({
    required this.mode,
    required this.navigationPattern,
    required this.capabilities,
  });

  final PlatformLayoutMode mode;
  final NavigationPattern navigationPattern;
  final PlatformCapabilities capabilities;
}

PlatformLayoutSnapshot inferPlatformLayout({
  required double width,
  bool? isWebOverride,
  TargetPlatform? targetPlatformOverride,
}) {
  final isWeb = isWebOverride ?? kIsWeb;
  final platform = targetPlatformOverride ?? defaultTargetPlatform;

  final mode = width < 700
      ? PlatformLayoutMode.compact
      : width < 1100
      ? PlatformLayoutMode.medium
      : PlatformLayoutMode.expanded;

  final prefersTouch =
      platform == TargetPlatform.android || platform == TargetPlatform.iOS;

  final supportsHover = isWeb ||
      platform == TargetPlatform.macOS ||
      platform == TargetPlatform.windows ||
      platform == TargetPlatform.linux;

  final supportsKeyboardShortcuts =
      supportsHover || platform == TargetPlatform.fuchsia;

  final navigationPattern = switch (mode) {
    PlatformLayoutMode.compact => NavigationPattern.bottomTabs,
    PlatformLayoutMode.medium => NavigationPattern.rail,
    PlatformLayoutMode.expanded => NavigationPattern.splitView,
  };

  return PlatformLayoutSnapshot(
    mode: mode,
    navigationPattern: navigationPattern,
    capabilities: PlatformCapabilities(
      isWeb: isWeb,
      supportsHover: supportsHover,
      supportsKeyboardShortcuts: supportsKeyboardShortcuts,
      prefersTouch: prefersTouch,
    ),
  );
}

class PlatformPlanController extends ChangeNotifier {
  PlatformPlanController({PlatformLayoutSnapshot? initialSnapshot})
    : _snapshot =
          initialSnapshot ??
          inferPlatformLayout(width: 1024, isWebOverride: false);

  PlatformLayoutSnapshot _snapshot;

  PlatformLayoutSnapshot get snapshot => _snapshot;

  void recompute({
    required double width,
    bool? isWebOverride,
    TargetPlatform? targetPlatformOverride,
  }) {
    _snapshot = inferPlatformLayout(
      width: width,
      isWebOverride: isWebOverride,
      targetPlatformOverride: targetPlatformOverride,
    );
    notifyListeners();
  }
}
