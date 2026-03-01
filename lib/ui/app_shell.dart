import 'package:flutter/material.dart';

/// UI state-management approach for the current port: keep things simple with
/// framework-native notifiers only (`ChangeNotifier` / `ValueNotifier`) and
/// avoid external state packages until a concrete need appears.
enum AppBreakpoint { compact, medium, expanded }

class AppSpacingTokens {
  const AppSpacingTokens._();

  static const double xs = 4;
  static const double sm = 8;
  static const double md = 12;
  static const double lg = 16;
  static const double xl = 24;
}

class DimensionTheme {
  static ThemeData light() {
    final scheme = ColorScheme.fromSeed(seedColor: Colors.indigo);
    return _baseTheme(scheme);
  }

  static ThemeData dark() {
    final scheme = ColorScheme.fromSeed(
      seedColor: Colors.indigo,
      brightness: Brightness.dark,
    );
    return _baseTheme(scheme);
  }

  static ThemeData _baseTheme(ColorScheme colorScheme) {
    return ThemeData(
      colorScheme: colorScheme,
      useMaterial3: true,
      visualDensity: VisualDensity.comfortable,
      inputDecorationTheme: const InputDecorationTheme(
        border: OutlineInputBorder(),
      ),
      cardTheme: const CardThemeData(
        margin: EdgeInsets.all(AppSpacingTokens.md),
      ),
    );
  }
}

AppBreakpoint breakpointForWidth(double width) {
  if (width < 600) {
    return AppBreakpoint.compact;
  }
  if (width < 1024) {
    return AppBreakpoint.medium;
  }
  return AppBreakpoint.expanded;
}

class AppRouteState {
  const AppRouteState({required this.path});

  final String path;

  static const AppRouteState home = AppRouteState(path: '/');

  factory AppRouteState.fromLocation(String location) {
    if (location.trim().isEmpty) {
      return home;
    }
    final normalized = location.startsWith('/') ? location : '/$location';
    return AppRouteState(path: normalized);
  }
}

class AppShellController extends ChangeNotifier {
  AppShellController({AppRouteState initialRoute = AppRouteState.home})
    : _route = initialRoute;

  AppRouteState _route;

  AppRouteState get route => _route;

  void navigateTo(String path) {
    final next = AppRouteState.fromLocation(path);
    if (next.path == _route.path) {
      return;
    }
    _route = next;
    notifyListeners();
  }
}

typedef AppShellContentBuilder = Widget Function(
  BuildContext context,
  AppBreakpoint breakpoint,
  AppRouteState route,
);

class AppShell extends StatelessWidget {
  const AppShell({
    super.key,
    required this.controller,
    required this.contentBuilder,
  });

  final AppShellController controller;
  final AppShellContentBuilder contentBuilder;

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: controller,
      builder: (context, _) {
        return LayoutBuilder(
          builder: (context, constraints) {
            final breakpoint = breakpointForWidth(constraints.maxWidth);
            return contentBuilder(context, breakpoint, controller.route);
          },
        );
      },
    );
  }
}
