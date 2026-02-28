import 'package:dimension/model/commands/file_listing.dart';
import 'package:dimension/model/commands/fs_listing.dart';
import 'package:dimension/model/commands/private_chat_command.dart';
import 'package:dimension/ui/circle_panel.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('controller supports select/unselect/close and chatReceived', () {
    final controller = CirclePanelController(circleName: 'LAN');

    controller.select();
    expect(controller.isSelected, isTrue);

    controller.unselect();
    expect(controller.isSelected, isFalse);

    final chat = PrivateChatCommand()..content = 'line1\nline2';
    controller.chatReceived(chat, username: 'alice');
    expect(controller.chatLines.length, 2);
    expect(controller.chatLines.first, contains('alice: line1'));

    controller.close();
    expect(controller.isClosed, isTrue);
  });

  test('controller updates listings and navigates up', () {
    final controller = CirclePanelController(circleName: 'LAN');

    final listing = FileListing()
      ..path = '/docs/sub'
      ..folders = [
        FSListing()
          ..name = 'nested'
          ..isFolder = true,
      ]
      ..files = [
        FSListing()
          ..name = 'readme.txt'
          ..fullPath = '/docs/sub/readme.txt'
          ..size = 12,
      ];

    controller.applyFileListing(listing);
    expect(controller.connected, isTrue);
    expect(controller.currentPath, '/docs/sub');
    expect(controller.folders, hasLength(1));
    expect(controller.files, hasLength(1));

    controller.navigateUp();
    expect(controller.currentPath, '/docs');
  });

  testWidgets('widget renders listings and chat', (tester) async {
    final controller = CirclePanelController(circleName: 'Test Circle');
    controller.applyFileListing(
      FileListing()
        ..path = '/'
        ..folders = [
          FSListing()
            ..name = 'music'
            ..isFolder = true,
        ]
        ..files = [
          FSListing()
            ..name = 'song.mp3'
            ..size = 2048,
        ],
    );
    controller.chatReceived(PrivateChatCommand()..content = 'hi', username: 'bob');

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: CirclePanel(
            controller: controller,
            isMono: false,
          ),
        ),
      ),
    );

    expect(find.text('Test Circle'), findsOneWidget);
    expect(find.text('music'), findsOneWidget);
    expect(find.text('song.mp3'), findsOneWidget);
    expect(find.textContaining('bob: hi'), findsOneWidget);
  });
}
