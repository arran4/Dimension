import 'package:dimension/model/commands/fs_listing.dart';
import 'package:dimension/model/commands/search_result_command.dart';
import 'package:dimension/ui/search_panel.dart';
import 'package:flutter_test/flutter_test.dart';

class _Backend implements SearchPanelBackend {
  String? lastKeyword;
  final downloaded = <String>[];

  @override
  Future<void> beginSearch(String keyword) async {
    lastKeyword = keyword;
  }

  @override
  Future<void> downloadElement({required int peerId, required FSListing listing}) async {
    downloaded.add('$peerId:${listing.fullPath}');
  }

  @override
  String usernameForPeer(int peerId) => peerId == 42 ? 'alice' : 'unknown';
}

void main() {
  test('controller applies keyword filtering and maps rows', () async {
    final backend = _Backend();
    final controller = SearchPanelController(backend: backend);

    await controller.doSearch('hello');
    expect(backend.lastKeyword, 'hello');

    final ignored = SearchResultCommand()
      ..keyword = 'other'
      ..myId = 42
      ..files = [
        FSListing()
          ..name = 'ignore.txt'
          ..fullPath = '/ignore.txt'
          ..size = 12,
      ];
    controller.searchCallback(ignored);
    expect(controller.rows, isEmpty);

    final accepted = SearchResultCommand()
      ..keyword = 'hello'
      ..myId = 42
      ..folders = [
        FSListing()
          ..name = 'docs'
          ..fullPath = '/docs'
          ..isFolder = true,
      ]
      ..files = [
        FSListing()
          ..name = 'readme.md'
          ..fullPath = '/docs/readme.md'
          ..size = 2048,
      ];

    controller.searchCallback(accepted);

    expect(controller.rows.length, 2);
    expect(controller.rows.first.username, 'alice');
    expect(controller.rows.last.entry.fsListing.name, 'readme.md');
  });

  test('controller forwards selected downloads via backend', () async {
    final backend = _Backend();
    final controller = SearchPanelController(backend: backend);

    final item = SearchThingy(
      userId: 9,
      fsListing: FSListing()
        ..name = 'a.bin'
        ..fullPath = '/a.bin',
    );

    await controller.downloadSelections([item]);

    expect(backend.downloaded, ['9:/a.bin']);
  });
}
