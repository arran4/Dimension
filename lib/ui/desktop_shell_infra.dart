import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'core_screens.dart';

class DesktopShortcutAction {
  const DesktopShortcutAction({
    required this.label,
    required this.shortcut,
    required this.section,
  });

  final String label;
  final SingleActivator shortcut;
  final CoreScreenSection section;
}

class DesktopHoverState {
  const DesktopHoverState({
    required this.hoveredSection,
    required this.statusMessage,
  });

  final CoreScreenSection? hoveredSection;
  final String? statusMessage;
}

class DesktopShellController extends ChangeNotifier {
  DesktopShellController()
    : _shortcuts = const <DesktopShortcutAction>[
        DesktopShortcutAction(
          label: 'Circles',
          shortcut: SingleActivator(LogicalKeyboardKey.digit1, control: true),
          section: CoreScreenSection.circles,
        ),
        DesktopShortcutAction(
          label: 'Peers',
          shortcut: SingleActivator(LogicalKeyboardKey.digit2, control: true),
          section: CoreScreenSection.peers,
        ),
        DesktopShortcutAction(
          label: 'Search',
          shortcut: SingleActivator(LogicalKeyboardKey.digit3, control: true),
          section: CoreScreenSection.search,
        ),
        DesktopShortcutAction(
          label: 'Transfers',
          shortcut: SingleActivator(LogicalKeyboardKey.digit4, control: true),
          section: CoreScreenSection.transfers,
        ),
      ];

  final List<DesktopShortcutAction> _shortcuts;
  CoreScreenSection _activeSection = CoreScreenSection.circles;
  DesktopHoverState _hoverState =
      const DesktopHoverState(hoveredSection: null, statusMessage: null);

  List<DesktopShortcutAction> get shortcuts =>
      List<DesktopShortcutAction>.unmodifiable(_shortcuts);

  CoreScreenSection get activeSection => _activeSection;

  DesktopHoverState get hoverState => _hoverState;

  void activateSection(CoreScreenSection section) {
    if (_activeSection == section) {
      return;
    }
    _activeSection = section;
    notifyListeners();
  }

  void onSectionHover(CoreScreenSection? section) {
    _hoverState = DesktopHoverState(
      hoveredSection: section,
      statusMessage: section == null
          ? null
          : 'Hovering ${section.name} (${_tooltip(section)})',
    );
    notifyListeners();
  }

  String _tooltip(CoreScreenSection section) {
    final entry = _shortcuts.where((s) => s.section == section).firstOrNull;
    if (entry == null) {
      return 'no shortcut';
    }
    return 'Ctrl+${_digitFor(section)}';
  }

  String _digitFor(CoreScreenSection section) {
    return switch (section) {
      CoreScreenSection.circles => '1',
      CoreScreenSection.peers => '2',
      CoreScreenSection.search => '3',
      CoreScreenSection.transfers => '4',
      CoreScreenSection.chat => '-',
      CoreScreenSection.settings => '-',
      CoreScreenSection.diagnostics => '-',
    };
  }

  Map<ShortcutActivator, VoidCallback> buildShortcutMap() {
    return <ShortcutActivator, VoidCallback>{
      for (final shortcut in _shortcuts)
        shortcut.shortcut: () => activateSection(shortcut.section),
    };
  }
}

extension<E> on Iterable<E> {
  E? get firstOrNull => isEmpty ? null : first;
}
