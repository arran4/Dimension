import 'package:dimension/ui/user_panel.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('controller supports selectChat/displayMessage/addLine/close', () {
    final controller = UserPanelController(username: 'alice');

    controller.selectChat();
    expect(controller.chatSelected, isTrue);

    controller.displayMessage('hello\nthere', author: 'bob');
    expect(controller.lines, hasLength(2));
    expect(controller.lines.first, 'bob: hello');

    controller.addLine('manual line');
    expect(controller.lines.last, 'manual line');

    controller.close();
    expect(controller.isClosed, isTrue);
  });

  testWidgets('widget renders empty and populated chat views', (tester) async {
    final controller = UserPanelController(username: 'alice');

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: UserPanel(controller: controller, isMono: true),
        ),
      ),
    );

    expect(find.text('No private messages yet.'), findsOneWidget);

    controller.displayMessage('hello', author: 'bob');
    await tester.pump();

    expect(find.text('alice'), findsOneWidget);
    expect(find.text('bob: hello'), findsOneWidget);
  });
}
