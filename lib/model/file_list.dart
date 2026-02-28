import 'dart:async';

import 'file_list_database.dart';
import 'fs_listing.dart';

typedef FileListUpdateCompleteHandler = void Function();

abstract class FileListShareScanner {
  FutureOr<ShareSnapshot?> scanRootShare(RootShare share, {required bool urgent});
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
  }) : _database = database,
       _scanner = scanner;

  final FileListDatabase _database;
  final FileListShareScanner? _scanner;

  bool isUpdating = false;
  bool _disposed = false;

  final List<FileListUpdateCompleteHandler> _updateListeners =
      <FileListUpdateCompleteHandler>[];

  final Map<int, RootShare> _rootSharesById = <int, RootShare>{};
  final Map<int, Folder> _foldersById = <int, Folder>{};
  final Map<int, File> _filesById = <int, File>{};

  Future<void> update(bool urgent) async {
    isUpdating = true;
    _rebuildIndexesFromDatabase();

    if (_scanner != null) {
      final shares = _database.getRootShares();
      for (final share in shares) {
        final snapshot = await _scanner.scanRootShare(share, urgent: urgent);
        if (snapshot != null) {
          _applySnapshot(snapshot);
        }
      }
    }

    isUpdating = false;
    _notifyUpdateComplete();
  }

  void clear() {
    _rootSharesById.clear();
    _foldersById.clear();
    _filesById.clear();
    _database.setULong(_database.fileList, 'Current FSListing ID', 0);
  }

  void dispose() {
    _disposed = true;
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

  void _notifyUpdateComplete() {
    for (final handler in _updateListeners) {
      handler();
    }
  }

  void _rebuildIndexesFromDatabase() {
    _rootSharesById.clear();
    _foldersById.clear();
    _filesById.clear();

    final rootShares = _database.getRootShares();
    for (final share in rootShares) {
      _rootSharesById[share.id] = share;
      _foldersById[share.id] = share;
    }
  }

  void _applySnapshot(ShareSnapshot snapshot) {
    _rootSharesById[snapshot.root.id] = snapshot.root;
    _foldersById[snapshot.root.id] = snapshot.root;

    for (final folder in snapshot.folders) {
      _foldersById[folder.id] = folder;
    }

    for (final file in snapshot.files) {
      _filesById[file.id] = file;
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
