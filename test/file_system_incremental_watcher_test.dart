import 'dart:async';
import 'dart:io';

import 'package:dimension/model/file_list.dart';
import 'package:dimension/model/file_system_incremental_watcher.dart';
import 'package:dimension/model/fs_listing.dart';
import 'package:flutter_test/flutter_test.dart';

class _Scanner implements FileListShareScanner {
  String? lastRoot;

  @override
  Future<ShareSnapshot?> scanRootShare(
    RootShare share, {
    required bool urgent,
  }) async {
    lastRoot = share.fullPath;
    return ShareSnapshot(root: share, folders: const <Folder>[], files: const <File>[]);
  }
}

class _EventSource implements FileSystemEventSource {
  final StreamController<FileSystemEvent> controller =
      StreamController<FileSystemEvent>.broadcast();

  String? watchedPath;

  @override
  Stream<FileSystemEvent> watch(String path, {required bool recursive}) {
    watchedPath = path;
    return controller.stream;
  }
}

void main() {
  test('filesystem incremental watcher emits snapshot on fs event', () async {
    final scanner = _Scanner();
    final source = _EventSource();
    final watcher = FileSystemIncrementalWatcher(
      scanner: scanner,
      eventSource: source,
      debounce: Duration.zero,
    );

    final root = RootShare()
      ..id = 1
      ..name = 'Public'
      ..fullPath = '/tmp/public';

    final stream = watcher.watch([root], urgent: false);
    final results = <ShareSnapshot>[];
    final sub = stream.listen(results.add);

    source.controller.add(FileSystemCreateEvent('/tmp/public/a.txt', false));
    await Future<void>.delayed(Duration.zero);
    await Future<void>.delayed(Duration.zero);

    expect(source.watchedPath, '/tmp/public');
    expect(scanner.lastRoot, '/tmp/public');
    expect(results, hasLength(1));

    await sub.cancel();
    await watcher.dispose();
  });
}
