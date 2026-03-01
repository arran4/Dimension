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
}
