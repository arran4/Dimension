import 'dart:async';

import 'file_list_database.dart';
import 'fs_listing.dart';

typedef FileListUpdateCompleteHandler = void Function();

abstract class FileListShareScanner {
  FutureOr<ShareSnapshot?> scanRootShare(RootShare share, {required bool urgent});
}

abstract class FileListIncrementalWatcher {
  Stream<ShareSnapshot> watch(
    Iterable<RootShare> rootShares, {
    required bool urgent,
  });

  Future<void> dispose();
}

class ShareSnapshot {
  ShareSnapshot({
    required this.root,
    required this.folders,
    required this.files,
  });

  final RootShare root;
  final List<Folder> folders;
  final List<File> files;
}

class FileList {
  FileList({
    required FileListDatabase database,
    FileListShareScanner? scanner,
    FileListIncrementalWatcher? incrementalWatcher,
  }) : _database = database,
       _scanner = scanner,
       _incrementalWatcher = incrementalWatcher;

  final FileListDatabase _database;
  final FileListShareScanner? _scanner;
  final FileListIncrementalWatcher? _incrementalWatcher;

  bool isUpdating = false;
  bool _disposed = false;
  StreamSubscription<ShareSnapshot>? _watchSubscription;

  final List<FileListUpdateCompleteHandler> _updateListeners =
      <FileListUpdateCompleteHandler>[];

  final Map<int, RootShare> _rootSharesById = <int, RootShare>{};
  final Map<int, Folder> _foldersById = <int, Folder>{};
  final Map<int, File> _filesById = <int, File>{};
  final Map<int, Set<int>> _rootFolderIds = <int, Set<int>>{};
  final Map<int, Set<int>> _rootFileIds = <int, Set<int>>{};

  Future<void> update(bool urgent) async {
    isUpdating = true;
    _rebuildIndexesFromDatabase();

    final shares = _database.getRootShares();

    if (_scanner != null) {
      for (final share in shares) {
        final snapshot = await _scanner?.scanRootShare(share, urgent: urgent);
        if (snapshot != null) {
          _applySnapshot(snapshot);
        }
      }
    }

    _ensureIncrementalWatch(shares, urgent: urgent);

    isUpdating = false;
    _notifyUpdateComplete();
  }

  void clear() {
    _rootSharesById.clear();
    _foldersById.clear();
    _filesById.clear();
    _rootFolderIds.clear();
    _rootFileIds.clear();
    _database.setULong(_database.fileList, 'Current FSListing ID', 0);
  }

  void dispose() {
    _disposed = true;
    final subscription = _watchSubscription;
    if (subscription != null) {
      unawaited(subscription.cancel());
      _watchSubscription = null;
    }

    final watcher = _incrementalWatcher;
    if (watcher != null) {
      unawaited(watcher.dispose());
    }
  }

  // Temporary compatibility shim for line-by-line migration parity.
  void Dispose() => dispose();

  RootShare? getRootShare(int id) {
    return _rootSharesById[id];
  }

  Folder? getFolder(int id) {
    return _foldersById[id];
  }

  File? getFile(int id) {
    return _filesById[id];
  }

  String getFullPath(FSListing item) {
    final segments = <String>[item.name];
    var parentId = item.parentId;

    while (parentId != 0) {
      final parentFolder = _foldersById[parentId] ?? _rootSharesById[parentId];
      if (parentFolder == null) {
        break;
      }
      segments.insert(0, parentFolder.name);
      parentId = parentFolder.parentId;
    }

    return '/${segments.join('/')}';
  }

  FSListing? getFSListing(String path, bool folder) {
    final normalizedPath = _normalize(path);
    if (normalizedPath == '/') {
      return null;
    }

    final segments = normalizedPath
        .split('/')
        .where((segment) => segment.isNotEmpty)
        .toList(growable: false);
    if (segments.isEmpty) {
      return null;
    }

    RootShare? currentRoot;
    for (final share in _rootSharesById.values) {
      if (share.name == segments.first) {
        currentRoot = share;
        break;
      }
    }
    if (currentRoot == null) {
      return null;
    }

    if (segments.length == 1) {
      return folder ? currentRoot : _filesById[currentRoot.id];
    }

    FSListing current = currentRoot;
    for (var i = 1; i < segments.length; i++) {
      final segment = segments[i];
      final isLast = i == segments.length - 1;
      final inFolder = current is Folder;
      if (!inFolder) {
        return null;
      }

      final currentFolder = current as Folder;
      Folder? nextFolder;
      for (final folderId in currentFolder.folderIds) {
        final candidate = _foldersById[folderId];
        if (candidate != null && candidate.name == segment) {
          nextFolder = candidate;
          break;
        }
      }
      if (nextFolder != null) {
        current = nextFolder;
        continue;
      }

      if (isLast) {
        for (final fileId in currentFolder.fileIds) {
          final candidate = _filesById[fileId];
          if (candidate != null && candidate.name == segment) {
            return candidate;
          }
        }
      }

      return null;
    }

    if (folder && current is! Folder) {
      return null;
    }
    if (!folder && current is Folder) {
      return null;
    }
    return current;
  }

  FSListing? getFSListingFromParent(Folder parent, String path) {
    final parentPath = getFullPath(parent);
    final normalized = _normalize(path);
    final rooted = normalized.startsWith(parentPath)
        ? normalized
        : _normalize('$parentPath/$normalized');
    return getFSListing(rooted, true) ?? getFSListing(rooted, false);
  }

  Future<void> doSave() async {
    // Current pure-Dart representation keeps data in injected stores directly.
  }

  Future<void> startUpdate(bool urgent) async {
    if (_disposed) {
      return;
    }
    await update(urgent);
  }

  void addUpdateCompleteListener(FileListUpdateCompleteHandler handler) {
    _updateListeners.add(handler);
  }

  void removeUpdateCompleteListener(FileListUpdateCompleteHandler handler) {
    _updateListeners.remove(handler);
  }

  void _ensureIncrementalWatch(List<RootShare> shares, {required bool urgent}) {
    if (_incrementalWatcher == null || _watchSubscription != null || _disposed) {
      return;
    }

    _watchSubscription = _incrementalWatcher
        ?.watch(shares, urgent: urgent)
        .listen((snapshot) {
          if (_disposed) {
            return;
          }
          _applySnapshot(snapshot);
          _notifyUpdateComplete();
        });
  }

  void _notifyUpdateComplete() {
    for (final handler in _updateListeners) {
      handler();
    }
  }

  void _rebuildIndexesFromDatabase() {
    _rootSharesById.clear();
    _foldersById.clear();
    _filesById.clear();
    _rootFolderIds.clear();
    _rootFileIds.clear();

    final rootShares = _database.getRootShares();
    for (final share in rootShares) {
      _rootSharesById[share.id] = share;
      _foldersById[share.id] = share;
      _rootFolderIds[share.id] = <int>{};
      _rootFileIds[share.id] = <int>{};
    }
  }

  void _applySnapshot(ShareSnapshot snapshot) {
    final rootId = snapshot.root.id;
    final previousRoot = _rootSharesById[rootId];

    final staleFolders = _rootFolderIds[rootId] ?? <int>{};
    final staleFiles = _rootFileIds[rootId] ?? <int>{};
    for (final id in staleFolders) {
      _foldersById.remove(id);
    }
    for (final id in staleFiles) {
      _filesById.remove(id);
    }

    _rootSharesById[rootId] = snapshot.root;
    _foldersById[rootId] = snapshot.root;

    final nextFolderIds = <int>{};
    for (final folder in snapshot.folders) {
      _foldersById[folder.id] = folder;
      nextFolderIds.add(folder.id);
    }

    final nextFileIds = <int>{};
    var totalBytes = 0;
    for (final file in snapshot.files) {
      _filesById[file.id] = file;
      nextFileIds.add(file.id);
      if (file.size > 0) {
        totalBytes += file.size;
      }
    }

    _rootFolderIds[rootId] = nextFolderIds;
    _rootFileIds[rootId] = nextFileIds;

    snapshot.root.totalBytes = totalBytes;
    final previousQuick = previousRoot?.quickHashedBytes ?? 0;
    final previousFull = previousRoot?.fullHashedBytes ?? 0;
    snapshot.root.quickHashedBytes =
        snapshot.root.quickHashedBytes > 0
        ? snapshot.root.quickHashedBytes.clamp(0, totalBytes)
        : previousQuick.clamp(0, totalBytes);
    snapshot.root.fullHashedBytes =
        snapshot.root.fullHashedBytes > 0
        ? snapshot.root.fullHashedBytes.clamp(0, totalBytes)
        : previousFull.clamp(0, totalBytes);

    _updateCurrentListingId(snapshot);
  }

  void _updateCurrentListingId(ShareSnapshot snapshot) {
    var maxId = snapshot.root.id;
    for (final folder in snapshot.folders) {
      if (folder.id > maxId) {
        maxId = folder.id;
      }
    }
    for (final file in snapshot.files) {
      if (file.id > maxId) {
        maxId = file.id;
      }
    }

    final current = _database.getULong(_database.fileList, 'Current FSListing ID', 0);
    if (maxId > current) {
      _database.setULong(_database.fileList, 'Current FSListing ID', maxId);
    }
  }

  String _normalize(String path) {
    final withForwardSlashes = path.replaceAll('\\', '/').trim();
    if (withForwardSlashes.isEmpty) {
      return '/';
    }
    final noDouble = withForwardSlashes.replaceAll(RegExp(r'/+'), '/');
    return noDouble.startsWith('/') ? noDouble : '/$noDouble';
  }
}
