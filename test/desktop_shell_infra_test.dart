import 'package:dimension/ui/core_screens.dart';
import 'package:dimension/ui/desktop_shell_infra.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('provides desktop shortcut definitions', () {
    final controller = DesktopShellController();

    expect(controller.shortcuts, isNotEmpty);
    expect(controller.shortcuts.first.section, CoreScreenSection.circles);
    expect(
      controller.shortcuts.first.shortcut,
      const SingleActivator(LogicalKeyboardKey.digit1, control: true),
    );
  });

  test('activateSection updates active section', () {
    final controller = DesktopShellController();

    controller.activateSection(CoreScreenSection.search);

    expect(controller.activeSection, CoreScreenSection.search);
  });

  test('buildShortcutMap actions activate matching sections', () {
    final controller = DesktopShellController();
    final map = controller.buildShortcutMap();

    final searchShortcut = const SingleActivator(
      LogicalKeyboardKey.digit3,
      control: true,
    );

    map[searchShortcut]?.call();

    expect(controller.activeSection, CoreScreenSection.search);
  });

  test('hover state exposes tooltip-like status text', () {
    final controller = DesktopShellController();

    controller.onSectionHover(CoreScreenSection.transfers);

    expect(controller.hoverState.hoveredSection, CoreScreenSection.transfers);
    expect(controller.hoverState.statusMessage, contains('Ctrl+4'));

    controller.onSectionHover(null);
    expect(controller.hoverState.statusMessage, isNull);
  });
}
