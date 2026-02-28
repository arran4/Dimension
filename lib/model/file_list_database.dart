import 'dart:convert';

import 'fs_listing.dart';

/// Minimal key-value store abstraction so production code can use a persistent
/// backend later while tests can inject in-memory fakes/mocks.
abstract class StringKeyValueStore {
  void set(String key, String value);
  String? get(String key);
  void remove(String key);
  void dispose();
}

/// Pure-Dart in-memory implementation used by default and by tests.
class InMemoryStringKeyValueStore implements StringKeyValueStore {
  final Map<String, String> _values = <String, String>{};

  @override
  void set(String key, String value) {
    _values[key] = value;
  }

  @override
  String? get(String key) => _values[key];

  @override
  void remove(String key) {
    _values.remove(key);
  }

  @override
  void dispose() {
    _values.clear();
  }
}

/// Stores all file-list related key-value stores.
class FileListDatabase {
  FileListDatabase({
    StringKeyValueStore? searchList,
    StringKeyValueStore? fileList,
    StringKeyValueStore? remoteFileLists,
    StringKeyValueStore? quickHashes,
    StringKeyValueStore? fullHashes,
    StringKeyValueStore? downloadQueue,
    StringKeyValueStore? settingsStore,
  })  : searchList = searchList ?? InMemoryStringKeyValueStore(),
        fileList = fileList ?? InMemoryStringKeyValueStore(),
        remoteFileLists = remoteFileLists ?? InMemoryStringKeyValueStore(),
        quickHashes = quickHashes ?? InMemoryStringKeyValueStore(),
        fullHashes = fullHashes ?? InMemoryStringKeyValueStore(),
        downloadQueue = downloadQueue ?? InMemoryStringKeyValueStore(),
        settingsStore = settingsStore ?? InMemoryStringKeyValueStore();

  final StringKeyValueStore searchList;
  final StringKeyValueStore fileList;
  final StringKeyValueStore remoteFileLists;
  final StringKeyValueStore quickHashes;
  final StringKeyValueStore fullHashes;
  final StringKeyValueStore downloadQueue;

  /// Temporary bridge while Settings is still being ported.
  final StringKeyValueStore settingsStore;

  void close() {
    for (final store in <StringKeyValueStore>[
      searchList,
      fileList,
      remoteFileLists,
      quickHashes,
      fullHashes,
      downloadQueue,
      settingsStore,
    ]) {
      store.dispose();
    }
  }

  void setString(StringKeyValueStore db, String name, String value) {
    db.set('s$name', value);
  }

  String getString(StringKeyValueStore db, String name, String defaultValue) {
    final value = db.get('s$name');
    if (value == null || value.isEmpty) {
      return defaultValue;
    }
    return value;
  }

  int allocateId() {
    final current = getULong(fileList, 'Current FSListing ID', 0);
    final next = current + 1;
    setULong(fileList, 'Current FSListing ID', next);
    return next;
  }

  List<RootShare> getRootShares() {
    final shareCount = getInt(settingsStore, 'Root Share Count', 0);
    final shares = <RootShare>[];
    for (var i = 0; i < shareCount; i++) {
      final share = getObject<RootShare>(
        settingsStore,
        'Root Share $i',
        RootShare.fromJson,
      );
      if (share != null) {
        shares.add(share);
      }
    }
    return shares;
  }

  void deleteObject(StringKeyValueStore db, String name) {
    db.remove('o$name');
  }

  void setObject<T>(
    StringKeyValueStore db,
    String name,
    T value,
    Map<String, dynamic> Function(T value) toJson,
  ) {
    db.set('o$name', jsonEncode(toJson(value)));
  }

  T? getObject<T>(
    StringKeyValueStore db,
    String name,
    T Function(Map<String, dynamic> json) fromJson,
  ) {
    final value = db.get('o$name');
    if (value == null || value.isEmpty) {
      return null;
    }

    final decoded = jsonDecode(value);
    if (decoded is! Map<String, dynamic>) {
      return null;
    }

    return fromJson(decoded);
  }

  void setInt(StringKeyValueStore db, String name, int value) {
    db.set('i$name', '$value');
  }

  void setULong(StringKeyValueStore db, String name, int value) {
    db.set('i$name', '$value');
  }

  int getInt(StringKeyValueStore db, String name, int defaultValue) {
    final rawValue = db.get('i$name');
    if (rawValue == null || rawValue.isEmpty) {
      return defaultValue;
    }
    return int.tryParse(rawValue) ?? defaultValue;
  }

  int getULong(StringKeyValueStore db, String name, int defaultValue) {
    final rawValue = db.get('i$name');
    if (rawValue == null || rawValue.isEmpty) {
      return defaultValue;
    }
    return int.tryParse(rawValue) ?? defaultValue;
  }
}

extension on RootShare {
  static RootShare fromJson(Map<String, dynamic> json) {
    final output = RootShare();
    output.id = json['id'] as int? ?? 0;
    output.parentId = json['parentId'] as int? ?? 0;
    output.name = json['name'] as String? ?? '';
    output.size = json['size'] as int? ?? 0;
    output.lastModified = json['lastModified'] as int? ?? 0;
    output.index = json['index'] as int? ?? 0;
    output.fullPath = json['fullPath'] as String? ?? '';
    output.totalBytes = json['totalBytes'] as int? ?? 0;
    output.quickHashedBytes = json['quickHashedBytes'] as int? ?? 0;
    output.fullHashedBytes = json['fullHashedBytes'] as int? ?? 0;
    output.folderIds = (json['folderIds'] as List<dynamic>? ?? <dynamic>[])
        .map((value) => value as int)
        .toList();
    output.fileIds = (json['fileIds'] as List<dynamic>? ?? <dynamic>[])
        .map((value) => value as int)
        .toList();
    return output;
  }
}
