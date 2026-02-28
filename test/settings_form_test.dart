import 'package:dimension/ui/settings_form.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

class _Controller implements SettingsFormController {
  @override
  String description = 'desc';

  @override
  bool playSounds = false;

  @override
  String username = 'old-name';

  String? savedUsername;
  String? savedDescription;
  bool? savedPlaySounds;

  @override
  Future<void> save({
    required String username,
    required String description,
    required bool playSounds,
  }) async {
    savedUsername = username;
    savedDescription = description;
    savedPlaySounds = playSounds;
  }
}

void main() {
  testWidgets('saves edited values via injected controller', (tester) async {
    final controller = _Controller();

    await tester.pumpWidget(
      MaterialApp(home: Scaffold(body: SettingsForm(controller: controller))),
    );

    await tester.enterText(find.byKey(const Key('settings.username')), 'new-name');
    await tester.enterText(find.byKey(const Key('settings.description')), 'new-desc');
    await tester.tap(find.byKey(const Key('settings.playSounds')));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Save'));
    await tester.pumpAndSettle();

    expect(controller.savedUsername, 'new-name');
    expect(controller.savedDescription, 'new-desc');
    expect(controller.savedPlaySounds, isTrue);
  });
}
