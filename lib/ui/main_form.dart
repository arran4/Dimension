import 'dart:io';

import 'package:dimension/model/commands/private_chat_command.dart';
import 'package:flutter/material.dart';

import 'circle_panel.dart';
import 'flash_window.dart';
import 'join_circle_form.dart';
import 'selectable_tab.dart';
import 'user_panel.dart';

class MainFormPeer {
  const MainFormPeer({required this.id, required this.username});

  final int id;
  final String username;
}

abstract class MainFormSettings {
  bool getBool(String key, bool defaultValue);
}

abstract class MainFormSoundPlayer {
  Future<void> playBell();
}

class MainFormTab {
  MainFormTab({
    required this.text,
    required this.tag,
    required this.controller,
    required this.builder,
  });

  final String text;
  final String tag;
  final Object controller;
  final WidgetBuilder builder;
}

class MainFormController extends ChangeNotifier {
  MainFormController({
    required MainFormSettings settings,
    required FlashWindowDriver flashDriver,
    required MainFormSoundPlayer soundPlayer,
    DateTime Function()? clock,
  }) : _settings = settings,
       _flashDriver = flashDriver,
       _soundPlayer = soundPlayer,
       _clock = clock ?? DateTime.now;

  final MainFormSettings _settings;
  final FlashWindowDriver _flashDriver;
  final MainFormSoundPlayer _soundPlayer;
  final DateTime Function() _clock;

  final List<MainFormTab> _tabs = <MainFormTab>[];
  int _selectedIndex = -1;
  bool _invertedColors = false;

  List<MainFormTab> get tabs => List<MainFormTab>.unmodifiable(_tabs);

  int get selectedIndex => _selectedIndex;

  bool get invertedColors => _invertedColors;

  bool get isMono =>
      Platform.environment.containsKey('MONO_ENV_OPTIONS') ||
      Platform.environment.containsKey('MONO_PATH');

  void selectIndex(int index) {
    if (index < 0 || index >= _tabs.length || index == _selectedIndex) {
      return;
    }

    _setSelectedState(false);
    _selectedIndex = index;
    _setSelectedState(true);
    notifyListeners();
  }

  MainFormTab addOrSelectPanel(
    String text,
    Object panelController,
    WidgetBuilder panelBuilder,
    String tag,
  ) {
    final existingIndex = _tabs.indexWhere((tab) => tab.tag == tag);
    if (existingIndex >= 0) {
      selectIndex(existingIndex);
      return _tabs[existingIndex];
    }

    _setSelectedState(false);

    final tab = MainFormTab(
      text: text,
      tag: tag,
      controller: panelController,
      builder: panelBuilder,
    );
    _tabs.add(tab);
    _selectedIndex = _tabs.length - 1;
    _setSelectedState(true);
    notifyListeners();
    return tab;
  }

  UserPanelController selectUser(MainFormPeer peer) {
    final existingIndex = _tabs.indexWhere(
      (tab) => tab.tag == '(!) Files for ${peer.id}',
    );
    if (existingIndex >= 0) {
      final controller = _tabs[existingIndex].controller as UserPanelController;
      selectIndex(existingIndex);
      return controller;
    }

    final controller = UserPanelController(username: peer.username);
    addOrSelectPanel(
      peer.username,
      controller,
      (_) => UserPanel(controller: controller, isMono: isMono),
      '(!) Files for ${peer.id}',
    );
    return controller;
  }

  void privateChatReceived(PrivateChatCommand command, MainFormPeer peer) {
    final panel = selectUser(peer);
    panel.selectChat();

    final now = _clock();
    final hh = now.hour.toString().padLeft(2, '0');
    final mm = now.minute.toString().padLeft(2, '0');
    for (final line in command.content.split('\n')) {
      if (line.trim().isEmpty) {
        continue;
      }
      panel.addLine('$hh:$mm ${peer.username}: $line');
    }

    flash();
  }

  Future<void> flash() async {
    if (_flashDriver.applicationIsActivated()) {
      return;
    }

    if (_settings.getBool('Flash on Name Drop', true)) {
      _flashDriver.flash();
    }

    if (_settings.getBool('Play sounds', true)) {
      await _soundPlayer.playBell();
    }
  }

  CirclePanelController addInternetCircle(
    List<InternetAddressEndpoint> endpoints,
    String url,
    CircleType circleType,
  ) {
    final normalizedTag = 'circle:${url.toLowerCase()}';
    final existingIndex = _tabs.indexWhere((tab) => tab.tag == normalizedTag);
    if (existingIndex >= 0) {
      selectIndex(existingIndex);
      return _tabs[existingIndex].controller as CirclePanelController;
    }

    final displayName = switch (circleType) {
      CircleType.lan => 'LAN',
      CircleType.kademlia => 'Kademlia: $url',
      CircleType.bootstrap => 'Internet: $url',
    };

    final controller = CirclePanelController(
      circleName: displayName,
      displayName: url,
    )..connected = endpoints.isNotEmpty;

    addOrSelectPanel(
      displayName,
      controller,
      (_) => CirclePanel(controller: controller, isMono: isMono),
      normalizedTag,
    );
    return controller;
  }

  void setColors() {
    _invertedColors = _settings.getBool('Invert Colors', false);
    notifyListeners();
  }

  void _setSelectedState(bool selected) {
    if (_selectedIndex < 0 || _selectedIndex >= _tabs.length) {
      return;
    }

    final controller = _tabs[_selectedIndex].controller;
    if (controller is SelectableTab) {
      if (selected) {
        controller.select();
      } else {
        controller.unselect();
      }
    }
  }
}

class MainForm extends StatelessWidget {
  const MainForm({super.key, required this.controller});

  final MainFormController controller;

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: controller,
      builder: (context, _) {
        final selectedTab = controller.selectedIndex >= 0
            ? controller.tabs[controller.selectedIndex]
            : null;

        final colorScheme = controller.invertedColors
            ? const ColorScheme.dark()
            : const ColorScheme.light();

        return Theme(
          data: ThemeData.from(colorScheme: colorScheme),
          child: Scaffold(
            appBar: AppBar(title: const Text('Dimension')),
            body: Row(
              children: [
                NavigationRail(
                  selectedIndex: controller.selectedIndex < 0
                      ? null
                      : controller.selectedIndex,
                  onDestinationSelected: controller.selectIndex,
                  destinations: [
                    for (final tab in controller.tabs)
                      NavigationRailDestination(
                        icon: const Icon(Icons.tab),
                        label: Text(tab.text),
                      ),
                  ],
                ),
                const VerticalDivider(width: 1),
                Expanded(
                  child: selectedTab == null
                      ? const Center(child: Text('No panel selected.'))
                      : selectedTab.builder(context),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}
