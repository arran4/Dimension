import 'package:dimension/model/commands/keyword_search_command.dart';
import 'package:dimension/model/core.dart';

import 'core_screens.dart';

abstract class CoreScreensDownloadDispatcher {
  Future<void> queueDownload(String itemName);
}

class CoreScreensLiveBackend implements CoreScreensBackend {
  CoreScreensLiveBackend({
    required this.core,
    required this.peersProvider,
    this.downloadDispatcher,
  });

  final Core core;
  final Iterable<CorePeer> Function() peersProvider;
  final CoreScreensDownloadDispatcher? downloadDispatcher;

  @override
  Future<void> joinCircle(String name) async {
    core.joinCircle(name);
  }

  @override
  Future<void> queueDownload(String itemName) async {
    await downloadDispatcher?.queueDownload(itemName);
  }

  @override
  Future<List<String>> refreshPeers() async {
    return peersProvider()
        .where((peer) => !peer.quit)
        .map((peer) => 'Peer ${peer.id}')
        .toList(growable: false);
  }

  @override
  Future<List<String>> runSearch(String query) async {
    final command = KeywordSearchCommand()..keyword = query;
    core.beginSearch(command);

    final result = core.searchResultForKeyword(query);
    if (result == null) {
      return const <String>[];
    }

    final rows = <String>[
      for (final folder in result.folders) 'Folder: ${folder.name}',
      for (final file in result.files) 'File: ${file.name}',
    ];
    return List<String>.unmodifiable(rows);
  }
}
