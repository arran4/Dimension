import 'dart:io';

import 'package:dimension/model/file_list.dart';
import 'package:dimension/model/file_list_database.dart';
import 'package:dimension/model/file_list_runtime.dart';
import 'package:dimension/model/file_system_incremental_watcher.dart';
import 'package:dimension/model/fs_listing.dart';
import 'package:flutter_test/flutter_test.dart';

class _Scanner implements FileListShareScanner {
  @override
  Future<ShareSnapshot?> scanRootShare(
    RootShare share, {
    required bool urgent,
  }) async {
    return null;
  }
}

class _Source implements FileSystemEventSource {
  @override
  Stream<FileSystemEvent> watch(String path, {required bool recursive}) {
    return const Stream<FileSystemEvent>.empty();
  }
}

void main() {
  test('runtime file-list factory wires filesystem incremental watcher', () {
    final fileList = createRuntimeFileList(
      database: FileListDatabase(),
      scanner: _Scanner(),
      fileSystemEventSource: _Source(),
    );

    expect(fileList, isA<FileList>());
  });
}
