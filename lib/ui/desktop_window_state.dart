import 'package:dartobjectutils/dartobjectutils.dart';
import 'dart:async';

class DesktopWindowGeometry {
  const DesktopWindowGeometry({
    required this.width,
    required this.height,
    required this.offsetX,
    required this.offsetY,
  });

  final double width;
  final double height;
  final double offsetX;
  final double offsetY;

  Map<String, double> toJson() => <String, double>{
        'width': width,
        'height': height,
        'offsetX': offsetX,
        'offsetY': offsetY,
      };

  static DesktopWindowGeometry fromJson(Map<String, Object?> json) {
    final jsonMap = json.cast<String, dynamic>();
    return DesktopWindowGeometry(
      width: getNumberPropOrDefault(jsonMap, 'width', 1200).toDouble(),
      height: getNumberPropOrDefault(jsonMap, 'height', 800).toDouble(),
      offsetX: getNumberPropOrDefault(jsonMap, 'offsetX', 100).toDouble(),
      offsetY: getNumberPropOrDefault(jsonMap, 'offsetY', 80).toDouble(),
    );
  }
}

abstract class DesktopWindowStateStore {
  Future<void> saveGeometry(DesktopWindowGeometry geometry);

  Future<DesktopWindowGeometry?> loadGeometry();
}

class InMemoryDesktopWindowStateStore implements DesktopWindowStateStore {
  DesktopWindowGeometry? _geometry;

  @override
  Future<DesktopWindowGeometry?> loadGeometry() async => _geometry;

  @override
  Future<void> saveGeometry(DesktopWindowGeometry geometry) async {
    _geometry = geometry;
  }
}

class DesktopWindowStateController {
  DesktopWindowStateController({required DesktopWindowStateStore store})
      : _store = store;

  final DesktopWindowStateStore _store;

  Future<void> persist(DesktopWindowGeometry geometry) {
    return _store.saveGeometry(geometry);
  }

  Future<DesktopWindowGeometry> restoreOrDefault() async {
    return await _store.loadGeometry() ??
        const DesktopWindowGeometry(
          width: 1200,
          height: 800,
          offsetX: 100,
          offsetY: 80,
        );
  }
}
