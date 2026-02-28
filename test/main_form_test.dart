import 'dart:io';

import 'package:dimension/model/commands/private_chat_command.dart';
import 'package:dimension/ui/flash_window.dart';
import 'package:dimension/ui/join_circle_form.dart';
import 'package:dimension/ui/main_form.dart';
import 'package:dimension/ui/user_panel.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

class _Settings implements MainFormSettings {
  _Settings(this.values);

  final Map<String, bool> values;

  @override
  bool getBool(String key, bool defaultValue) => values[key] ?? defaultValue;
}

class _SoundPlayer implements MainFormSoundPlayer {
  var playCount = 0;

  @override
  Future<void> playBell() async {
    playCount++;
  }
}

class _FlashDriver extends FlashWindowDriver {
  _FlashDriver({required this.activated});

  bool activated;
  var flashCount = 0;

  @override
  bool applicationIsActivated() => activated;

  @override
  bool flash({int? count, FlashType type = FlashType.all}) {
    flashCount++;
    return true;
  }

  @override
  bool start({FlashType type = FlashType.all}) => true;

  @override
  bool stop() => true;
}

void main() {
  test('addOrSelectPanel deduplicates tags and tracks selection', () {
    final controller = MainFormController(
      settings: _Settings({}),
      flashDriver: _FlashDriver(activated: true),
      soundPlayer: _SoundPlayer(),
    );

    controller.addOrSelectPanel('Welcome', Object(), (_) => const SizedBox(), 'welcome');
    controller.addOrSelectPanel('Again', Object(), (_) => const SizedBox(), 'welcome');

    expect(controller.tabs, hasLength(1));
    expect(controller.selectedIndex, 0);
  });

  test('privateChatReceived creates user panel and appends timestamped lines', () {
    final controller = MainFormController(
      settings: _Settings({'Flash on Name Drop': false, 'Play sounds': false}),
      flashDriver: _FlashDriver(activated: false),
      soundPlayer: _SoundPlayer(),
      clock: () => DateTime(2026, 1, 2, 3, 4),
    );

    final command = PrivateChatCommand()..content = 'hello\nthere';

    controller.privateChatReceived(
      command,
      const MainFormPeer(id: 42, username: 'alice'),
    );

    expect(controller.tabs.single.text, 'alice');
    final userController = controller.tabs.single.controller as UserPanelController;
    expect(userController.lines, ['03:04 alice: hello', '03:04 alice: there']);
  });

  test('flash honors activation and user settings', () async {
    final flashDriver = _FlashDriver(activated: false);
    final soundPlayer = _SoundPlayer();
    final controller = MainFormController(
      settings: _Settings({'Flash on Name Drop': true, 'Play sounds': true}),
      flashDriver: flashDriver,
      soundPlayer: soundPlayer,
    );

    await controller.flash();

    expect(flashDriver.flashCount, 1);
    expect(soundPlayer.playCount, 1);

    flashDriver.activated = true;
    await controller.flash();
    expect(flashDriver.flashCount, 1);
    expect(soundPlayer.playCount, 1);
  });

  test('addInternetCircle deduplicates URL irrespective of case', () {
    final controller = MainFormController(
      settings: _Settings({}),
      flashDriver: _FlashDriver(activated: true),
      soundPlayer: _SoundPlayer(),
    );

    controller.addInternetCircle(
      [InternetAddressEndpoint(InternetAddress.loopbackIPv4, 4040)],
      'Example.com/Bootstrap.php',
      CircleType.bootstrap,
    );
    controller.addInternetCircle(
      const [],
      'example.com/bootstrap.php',
      CircleType.bootstrap,
    );

    expect(controller.tabs, hasLength(1));
  });

  testWidgets('MainForm renders rail destinations and selected panel', (tester) async {
    final controller = MainFormController(
      settings: _Settings({'Invert Colors': true}),
      flashDriver: _FlashDriver(activated: true),
      soundPlayer: _SoundPlayer(),
    )..setColors();

    controller.addOrSelectPanel(
      'Welcome',
      Object(),
      (_) => const Center(child: Text('Welcome Body')),
      'welcome',
    );

    await tester.pumpWidget(MaterialApp(home: MainForm(controller: controller)));

    expect(find.text('Welcome'), findsOneWidget);
    expect(find.text('Welcome Body'), findsOneWidget);
  });
}
