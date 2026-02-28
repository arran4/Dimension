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

void main() {
  group('FileListDatabase', () {
    test('allocateId increments from persisted value', () {
      final db = FileListDatabase();
      expect(db.allocateId(), 1);
      expect(db.allocateId(), 2);
    });

    test('get and set string/int values with defaults', () {
      final db = FileListDatabase();
      expect(db.getString(db.fileList, 'missing', 'fallback'), 'fallback');
      expect(db.getInt(db.fileList, 'missing-int', 17), 17);

      db.setString(db.fileList, 'greeting', 'hello');
      db.setInt(db.fileList, 'port', 1337);

      expect(db.getString(db.fileList, 'greeting', 'fallback'), 'hello');
      expect(db.getInt(db.fileList, 'port', 0), 1337);
    });

    test('stores and reads root shares from settings store', () {
      final db = FileListDatabase();
      final share = RootShare()
        ..name = 'Public'
        ..index = 0
        ..fullPath = '/shares/public'
        ..folderIds = <int>[1, 2]
        ..fileIds = <int>[3];

      db.setInt(db.settingsStore, 'Root Share Count', 1);
      db.setObject<RootShare>(
        db.settingsStore,
        'Root Share 0',
        share,
        _rootShareToJson,
      );

      final shares = db.getRootShares();
      expect(shares, hasLength(1));
      expect(shares.first.name, 'Public');
      expect(shares.first.folderIds, <int>[1, 2]);
      expect(shares.first.fileIds, <int>[3]);
    });

    test('deleteObject removes object data', () {
      final db = FileListDatabase();
      final share = RootShare()..name = 'Temp';
      db.setObject<RootShare>(db.fileList, 'sample', share, _rootShareToJson);

      db.deleteObject(db.fileList, 'sample');

      final deleted = db.getObject<RootShare>(
        db.fileList,
        'sample',
        (json) => RootShare()..name = json['name'] as String? ?? '',
      );
      expect(deleted, isNull);
    });
  });
}
