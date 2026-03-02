import 'dart:async';
import 'dart:io';

import 'file_list.dart';
import 'fs_listing.dart';

abstract class FileSystemEventSource {
  Stream<FileSystemEvent> watch(String path, {required bool recursive});
}

class IoFileSystemEventSource implements FileSystemEventSource {
  @override
  Stream<FileSystemEvent> watch(String path, {required bool recursive}) {
    return Directory(path).watch(recursive: recursive);
  }
}

class FileSystemIncrementalWatcher implements FileListIncrementalWatcher {
  FileSystemIncrementalWatcher({
    required FileListShareScanner scanner,
    FileSystemEventSource? eventSource,
    Duration debounce = const Duration(milliseconds: 300),
  })  : _scanner = scanner,
        _eventSource = eventSource ?? IoFileSystemEventSource(),
        _debounce = debounce;

  final FileListShareScanner _scanner;
  final FileSystemEventSource _eventSource;
  final Duration _debounce;

  final StreamController<ShareSnapshot> _controller =
      StreamController<ShareSnapshot>.broadcast();
  final List<StreamSubscription<FileSystemEvent>> _subscriptions =
      <StreamSubscription<FileSystemEvent>>[];
  final Map<int, Timer> _debounceTimers = <int, Timer>{};

  @override
  Stream<ShareSnapshot> watch(
    Iterable<RootShare> rootShares, {
    required bool urgent,
  }) {
    _cancelSubscriptions();

    for (final share in rootShares) {
      if (share.fullPath.trim().isEmpty) {
        continue;
      }

      final stream = _eventSource.watch(share.fullPath, recursive: true);
      final subscription = stream.listen((_) {
        _debounceTimers[share.id]?.cancel();
        _debounceTimers[share.id] = Timer(_debounce, () async {
          final snapshot = await _scanner.scanRootShare(share, urgent: urgent);
          if (snapshot != null && !_controller.isClosed) {
            _controller.add(snapshot);
          }
        });
      });
      _subscriptions.add(subscription);
    }

    return _controller.stream;
  }

  @override
  Future<void> dispose() async {
    _cancelSubscriptions();
    for (final timer in _debounceTimers.values) {
      timer.cancel();
    }
    _debounceTimers.clear();
    await _controller.close();
  }

  void _cancelSubscriptions() {
    for (final subscription in _subscriptions) {
      unawaited(subscription.cancel());
    }
    _subscriptions.clear();
  }
}
