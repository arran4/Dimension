import 'dart:io' show Platform;

import 'package:flutter/material.dart';

import '../model/closable_tab.dart';

class UserPanelController extends ChangeNotifier implements ClosableTab {
  UserPanelController({required this.username});

  final String username;

  bool _closed = false;
  bool _chatSelected = true;
  final List<String> lines = <String>[];

  bool get isClosed => _closed;
  bool get chatSelected => _chatSelected;

  void selectChat() {
    _chatSelected = true;
    notifyListeners();
  }

  void displayMessage(String content, {required String author}) {
    for (final line in content.split('\n')) {
      if (line.trim().isEmpty) {
        continue;
      }
      addLine('$author: $line');
    }
  }

  void addLine(String line) {
    lines.add(line);
    notifyListeners();
  }

  @override
  void close() {
    _closed = true;
    notifyListeners();
  }
}

class UserPanel extends StatelessWidget {
  const UserPanel({
    super.key,
    required this.controller,
    this.onClose,
    bool? isMono,
  }) : _isMonoOverride = isMono;

  final UserPanelController controller;
  final VoidCallback? onClose;
  final bool? _isMonoOverride;

  bool get isMono => _isMonoOverride ?? _platformIsMono();

  static bool _platformIsMono() {
    return Platform.environment.containsKey('MONO_ENV_OPTIONS') ||
        Platform.environment.containsKey('MONO_PATH');
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: controller,
      builder: (context, _) {
        return Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            ListTile(
              dense: !isMono,
              title: Text(controller.username),
              subtitle: Text(controller.chatSelected ? 'Chat selected' : ''),
              trailing: IconButton(
                onPressed: () {
                  controller.close();
                  onClose?.call();
                },
                icon: const Icon(Icons.close),
              ),
            ),
            if (controller.lines.isEmpty)
              const Expanded(
                child: Center(child: Text('No private messages yet.')),
              )
            else
              Expanded(
                child: ListView.builder(
                  itemCount: controller.lines.length,
                  itemBuilder: (context, index) => ListTile(
                    dense: true,
                    title: Text(controller.lines[index]),
                  ),
                ),
              ),
          ],
        );
      },
    );
  }
}
