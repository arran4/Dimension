import 'package:dimension/ui/desktop_window_state.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('restoreOrDefault returns defaults when no state persisted', () async {
    final controller = DesktopWindowStateController(
      store: InMemoryDesktopWindowStateStore(),
    );

    final restored = await controller.restoreOrDefault();
    expect(restored.width, 1200);
    expect(restored.height, 800);
  });

  test('persist and restore round trips geometry', () async {
    final store = InMemoryDesktopWindowStateStore();
    final controller = DesktopWindowStateController(store: store);
    const geometry = DesktopWindowGeometry(
      width: 1600,
      height: 900,
      offsetX: 20,
      offsetY: 30,
    );

    await controller.persist(geometry);
    final restored = await controller.restoreOrDefault();

    expect(restored.width, 1600);
    expect(restored.height, 900);
    expect(restored.offsetX, 20);
    expect(restored.offsetY, 30);
  });

  test('json conversion keeps fields', () {
    const geometry = DesktopWindowGeometry(
      width: 1400,
      height: 850,
      offsetX: 11,
      offsetY: 22,
    );

    final restored = DesktopWindowGeometry.fromJson(geometry.toJson());
    expect(restored.width, 1400);
    expect(restored.height, 850);
    expect(restored.offsetX, 11);
    expect(restored.offsetY, 22);
  });
}
