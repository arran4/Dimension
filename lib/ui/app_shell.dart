import 'package:flutter/gestures.dart';
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
  const AppRouteState({
    required this.path,
    this.queryParameters = const <String, String>{},
    this.fragment,
  });

  final String path;
  final Map<String, String> queryParameters;
  final String? fragment;

  static const AppRouteState home = AppRouteState(path: '/');

  factory AppRouteState.fromLocation(String location) {
    final trimmed = location.trim();
    if (trimmed.isEmpty) {
      return home;
    }

    final normalized = trimmed.startsWith('/') ? trimmed : '/$trimmed';
    final uri = Uri.parse(normalized);
    return AppRouteState(
      path: uri.path.isEmpty ? '/' : uri.path,
      queryParameters: Map<String, String>.unmodifiable(uri.queryParameters),
      fragment: uri.fragment.isEmpty ? null : uri.fragment,
    );
  }

  String toLocation() {
    final uri = Uri(
      path: path,
      queryParameters: queryParameters.isEmpty ? null : queryParameters,
      fragment: fragment,
    );
    return uri.toString();
  }
}

abstract class AppRouteStateStore {
  Future<void> saveLocation(String location);

  Future<String?> loadLocation();
}

class InMemoryAppRouteStateStore implements AppRouteStateStore {
  String? _location;

  @override
  Future<String?> loadLocation() async => _location;

  @override
  Future<void> saveLocation(String location) async {
    _location = location;
  }
}

class AppShellController extends ChangeNotifier {
  AppShellController({
    AppRouteState initialRoute = AppRouteState.home,
    this.routeStateStore,
  }) : _history = <AppRouteState>[initialRoute],
       _index = 0;

  final AppRouteStateStore? routeStateStore;

  final List<AppRouteState> _history;
  int _index;

  AppRouteState get route => _history[_index];

  bool get canGoBack => _index > 0;

  bool get canGoForward => _index < _history.length - 1;

  Future<void> restoreRouteState() async {
    final location = await routeStateStore?.loadLocation();
    if (location == null || location.trim().isEmpty) {
      return;
    }
    navigateTo(location);
  }

  void navigateTo(String path) {
    final next = AppRouteState.fromLocation(path);
    if (_isSameRoute(route, next)) {
      return;
    }

    if (canGoForward) {
      _history.removeRange(_index + 1, _history.length);
    }

    _history.add(next);
    _index = _history.length - 1;
    routeStateStore?.saveLocation(next.toLocation());
    notifyListeners();
  }

  void goBack() {
    if (!canGoBack) {
      return;
    }
    _index -= 1;
    routeStateStore?.saveLocation(route.toLocation());
    notifyListeners();
  }

  void goForward() {
    if (!canGoForward) {
      return;
    }
    _index += 1;
    routeStateStore?.saveLocation(route.toLocation());
    notifyListeners();
  }

  bool _isSameRoute(AppRouteState left, AppRouteState right) {
    if (left.path != right.path || left.fragment != right.fragment) {
      return false;
    }
    if (left.queryParameters.length != right.queryParameters.length) {
      return false;
    }
    for (final entry in left.queryParameters.entries) {
      if (right.queryParameters[entry.key] != entry.value) {
        return false;
      }
    }
    return true;
  }
}

typedef AppShellContentBuilder = Widget Function(
  BuildContext context,
  AppBreakpoint breakpoint,
  AppRouteState route,
);

class AppShellWebInputWrapper extends StatelessWidget {
  const AppShellWebInputWrapper({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return ScrollConfiguration(
      behavior: const MaterialScrollBehavior().copyWith(
        dragDevices: const <PointerDeviceKind>{
          PointerDeviceKind.touch,
          PointerDeviceKind.mouse,
          PointerDeviceKind.trackpad,
          PointerDeviceKind.stylus,
          PointerDeviceKind.unknown,
        },
      ),
      child: FocusTraversalGroup(
        policy: OrderedTraversalPolicy(),
        child: SelectionArea(child: child),
      ),
    );
  }
}

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
            return AppShellWebInputWrapper(
              child: contentBuilder(context, breakpoint, controller.route),
            );
          },
        );
      },
    );
  }
}
