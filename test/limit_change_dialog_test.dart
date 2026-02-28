import 'package:dimension/ui/limit_change_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('converts bytes to limit selection and back', () {
    final selection = LimitChangeLogic.fromBytesPerSecond(4096);

    expect(selection.noLimit, isFalse);
    expect(selection.value, 4);
    expect(selection.unitIndex, 1);

    final serialized = LimitChangeLogic.toBytesPerSecond(
      noLimit: selection.noLimit,
      value: selection.value,
      unitIndex: selection.unitIndex,
    );
    expect(serialized, 4096);
  });

  testWidgets('saves selected rate limit to injected settings', (tester) async {
    final settings = InMemorySpeedLimitSettings();

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: Builder(
            builder: (context) => TextButton(
              onPressed: () {
                LimitChangeDialog.show(
                  context,
                  whichLimit: WhichLimit.up,
                  settings: settings,
                );
              },
              child: const Text('Open'),
            ),
          ),
        ),
      ),
    );

    await tester.tap(find.text('Open'));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Use limit'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextField), '2');
    await tester.tap(find.text('B/s'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('KB/s').last);
    await tester.pumpAndSettle();

    await tester.tap(find.text('OK'));
    await tester.pumpAndSettle();

    expect(settings.getLimitBytesPerSecond(WhichLimit.up), 2048);
  });
}
