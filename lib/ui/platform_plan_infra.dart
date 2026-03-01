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
  double? height,
  bool? isWebOverride,
  TargetPlatform? targetPlatformOverride,
}) {
  final isWeb = isWebOverride ?? kIsWeb;
  final platform = targetPlatformOverride ?? defaultTargetPlatform;

  final prefersTouch =
      platform == TargetPlatform.android || platform == TargetPlatform.iOS;

  final shortestSide = height == null ? width : (width < height ? width : height);

  final baseMode = shortestSide < 700
      ? PlatformLayoutMode.compact
      : shortestSide < 1100
      ? PlatformLayoutMode.medium
      : PlatformLayoutMode.expanded;

  final mode = prefersTouch && height != null && width >= 700 && height < 500
      ? PlatformLayoutMode.compact
      : baseMode;

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
  PlatformPlanController({
    PlatformLayoutSnapshot? initialSnapshot,
    this.isWebOverride,
    this.targetPlatformOverride,
  }) : _snapshot =
           initialSnapshot ??
           inferPlatformLayout(
             width: 1024,
             isWebOverride: isWebOverride ?? false,
             targetPlatformOverride: targetPlatformOverride,
           );

  final bool? isWebOverride;
  final TargetPlatform? targetPlatformOverride;

  PlatformLayoutSnapshot _snapshot;

  PlatformLayoutSnapshot get snapshot => _snapshot;

  void recompute({
    required double width,
    double? height,
    bool? isWebOverride,
    TargetPlatform? targetPlatformOverride,
  }) {
    _snapshot = inferPlatformLayout(
      width: width,
      height: height,
      isWebOverride: isWebOverride ?? this.isWebOverride,
      targetPlatformOverride: targetPlatformOverride ?? this.targetPlatformOverride,
    );
    notifyListeners();
  }
}
