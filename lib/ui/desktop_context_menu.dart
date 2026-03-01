import 'package:flutter/material.dart';

class DesktopMenuAction {
  const DesktopMenuAction({required this.label, required this.onSelected});

  final String label;
  final VoidCallback onSelected;
}

class DesktopContextMenuController {
  const DesktopContextMenuController();

  List<DesktopMenuAction> buildActions({
    required String section,
    required VoidCallback onRefresh,
    required VoidCallback onOpenDetails,
  }) {
    return <DesktopMenuAction>[
      DesktopMenuAction(
        label: 'Refresh $section',
        onSelected: onRefresh,
      ),
      DesktopMenuAction(
        label: 'Open $section details',
        onSelected: onOpenDetails,
      ),
    ];
  }
}
