import 'file_list.dart';
import 'file_list_database.dart';
import 'file_system_incremental_watcher.dart';

FileList createRuntimeFileList({
  required FileListDatabase database,
  required FileListShareScanner scanner,
  FileSystemEventSource? fileSystemEventSource,
}) {
  return FileList(
    database: database,
    scanner: scanner,
    incrementalWatcher: FileSystemIncrementalWatcher(
      scanner: scanner,
      eventSource: fileSystemEventSource,
    ),
  );
}
