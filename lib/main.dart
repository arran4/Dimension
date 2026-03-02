import 'package:flutter/material.dart';
import 'ui/app_shell.dart';
import 'ui/main_form.dart';
import 'ui/flash_window.dart';

class MockMainFormSettings implements MainFormSettings {
  @override
  bool getBool(String key, bool defaultValue) => defaultValue;
}

class MockFlashWindowDriver implements FlashWindowDriver {
  @override
  bool applicationIsActivated() => true;

  @override
  bool flash({int? count, FlashType type = FlashType.all}) => true;

  @override
  bool start({FlashType type = FlashType.all}) => true;

  @override
  bool stop() => true;
}

class MockMainFormSoundPlayer implements MainFormSoundPlayer {
  @override
  Future<void> playBell() async {}
}

void main() {
  runApp(const DimensionApp());
}

class DimensionApp extends StatefulWidget {
  const DimensionApp({super.key});

  @override
  State<DimensionApp> createState() => _DimensionAppState();
}

class _DimensionAppState extends State<DimensionApp> {
  late final AppShellController _controller;
  late final MainFormController _mainFormController;

  @override
  void initState() {
    super.initState();
    _controller = AppShellController();
    _mainFormController = MainFormController(
      settings: MockMainFormSettings(),
      flashDriver: MockFlashWindowDriver(),
      soundPlayer: MockMainFormSoundPlayer(),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    _mainFormController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Dimension',
      theme: DimensionTheme.light(),
      darkTheme: DimensionTheme.dark(),
      home: AppShell(
        controller: _controller,
        contentBuilder: (context, breakpoint, route) {
          return MainForm(controller: _mainFormController);
        },
      ),
    );
  }
}
