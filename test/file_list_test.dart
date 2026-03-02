import 'dart:async';

import 'package:dimension/model/file_list.dart';
import 'package:dimension/model/file_list_database.dart';
import 'package:dimension/model/fs_listing.dart';
import 'package:flutter_test/flutter_test.dart';

Map<String, dynamic> _rootShareToJson(RootShare value) {
  return <String, dynamic>{
    'id': value.id,
    'parentId': value.parentId,
    'name': value.name,
    'size': value.size,
    'lastModified': value.lastModified,
    'index': value.index,
    'fullPath': value.fullPath,
    'totalBytes': value.totalBytes,
    'quickHashedBytes': value.quickHashedBytes,
    'fullHashedBytes': value.fullHashedBytes,
    'folderIds': value.folderIds,
    'fileIds': value.fileIds,
  };
}

class _Scanner implements FileListShareScanner {
  @override
  Future<ShareSnapshot?> scanRootShare(
    RootShare share, {
    required bool urgent,
  }) async {
    final docs = Folder()
      ..id = 100
      ..parentId = share.id
      ..name = 'docs'
      ..fileIds = [200];
    final readme = File()
      ..id = 200
      ..parentId = docs.id
      ..name = 'readme.txt';

    share.folderIds = [docs.id];
    return ShareSnapshot(root: share, folders: [docs], files: [readme]);
  }
}

class _Watcher implements FileListIncrementalWatcher {
  final StreamController<ShareSnapshot> controller =
      StreamController<ShareSnapshot>.broadcast();

  var watchCalls = 0;

  @override
  Stream<ShareSnapshot> watch(
    Iterable<RootShare> rootShares, {
    required bool urgent,
  }) {
    watchCalls++;
    return controller.stream;
  }

  @override
  Future<void> dispose() async {
    await controller.close();
  }
}

void main() {
  test('update + scanner populate listing indexes', () async {
    final db = FileListDatabase();
    final root = RootShare()
      ..id = 1
      ..name = 'Public'
      ..index = 0
      ..fullPath = '/public';
    db.setInt(db.settingsStore, 'Root Share Count', 1);
    db.setObject<RootShare>(
      db.settingsStore,
      'Root Share 0',
      root,
      _rootShareToJson,
    );

    final fileList = FileList(database: db, scanner: _Scanner());

    await fileList.update(false);

    final folder = fileList.getFSListing('/Public/docs', true);
    final file = fileList.getFSListing('/Public/docs/readme.txt', false);

    expect(folder, isA<Folder>());
    expect(file, isA<File>());
    expect(fileList.getFullPath(file!), '/Public/docs/readme.txt');
  });

  test('clear resets indexes and fslisting id counter', () {
    final db = FileListDatabase();
    db.setULong(db.fileList, 'Current FSListing ID', 9);

    final fileList = FileList(database: db);
    fileList.clear();

    expect(db.getULong(db.fileList, 'Current FSListing ID', -1), 0);
    expect(fileList.getRootShare(1), isNull);
  });

  test('incremental watcher applies updates after initial scan', () async {
    final db = FileListDatabase();
    final root = RootShare()
      ..id = 1
      ..name = 'Public'
      ..index = 0
      ..fullPath = '/public';
    db.setInt(db.settingsStore, 'Root Share Count', 1);
    db.setObject<RootShare>(
      db.settingsStore,
      'Root Share 0',
      root,
      _rootShareToJson,
    );

    final watcher = _Watcher();
    final fileList = FileList(
      database: db,
      scanner: _Scanner(),
      incrementalWatcher: watcher,
    );

    await fileList.update(false);

    final updates = <int>[];
    fileList.addUpdateCompleteListener(() {
      updates.add(updates.length + 1);
    });

    final extraFolder = Folder()
      ..id = 101
      ..parentId = root.id
      ..name = 'music'
      ..fileIds = [201];
    final track = File()
      ..id = 201
      ..parentId = extraFolder.id
      ..name = 'track.mp3';

    root.folderIds = [...root.folderIds, extraFolder.id];
    watcher.controller.add(
      ShareSnapshot(root: root, folders: [extraFolder], files: [track]),
    );
    await Future<void>.delayed(Duration.zero);

    expect(watcher.watchCalls, 1);
    expect(
        fileList.getFSListing('/Public/music/track.mp3', false), isA<File>());
    expect(updates, isNotEmpty);

    fileList.dispose();
  });

  test(
      'snapshot reconcile removes stale entries and keeps hash progress bounded',
      () async {
    final db = FileListDatabase();
    final root = RootShare()
      ..id = 1
      ..name = 'Public'
      ..index = 0
      ..fullPath = '/public';
    db.setInt(db.settingsStore, 'Root Share Count', 1);
    db.setObject<RootShare>(
      db.settingsStore,
      'Root Share 0',
      root,
      _rootShareToJson,
    );

    final watcher = _Watcher();
    final fileList = FileList(
      database: db,
      scanner: _Scanner(),
      incrementalWatcher: watcher,
    );

    await fileList.update(false);
    expect(
        fileList.getFSListing('/Public/docs/readme.txt', false), isA<File>());

    final newerRoot = RootShare()
      ..id = root.id
      ..name = root.name
      ..index = root.index
      ..fullPath = root.fullPath;

    final folder = Folder()
      ..id = 101
      ..parentId = newerRoot.id
      ..name = 'media'
      ..fileIds = [205];
    final media = File()
      ..id = 205
      ..parentId = folder.id
      ..name = 'movie.mkv'
      ..size = 100;

    newerRoot.folderIds = [folder.id];
    newerRoot.quickHashedBytes = 5000;
    newerRoot.fullHashedBytes = 5000;

    watcher.controller.add(
      ShareSnapshot(root: newerRoot, folders: [folder], files: [media]),
    );
    await Future<void>.delayed(Duration.zero);

    expect(fileList.getFSListing('/Public/docs/readme.txt', false), isNull);
    expect(
        fileList.getFSListing('/Public/media/movie.mkv', false), isA<File>());

    final updatedRoot = fileList.getRootShare(1)!;
    expect(updatedRoot.totalBytes, 100);
    expect(updatedRoot.quickHashedBytes, 100);
    expect(updatedRoot.fullHashedBytes, 100);
    expect(db.getULong(db.fileList, 'Current FSListing ID', 0), 205);

    fileList.dispose();
  });
}
