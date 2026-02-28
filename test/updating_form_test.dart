import 'dart:typed_data';

import 'package:dimension/updating_form.dart';
import 'package:flutter_test/flutter_test.dart';

class _Mutex implements UpdaterMutex {
  bool waited = false;

  @override
  Future<void> waitUntilReleased() async {
    waited = true;
  }
}

class _Downloader implements UpdaterDownloader {
  bool called = false;

  @override
  Future<Uint8List> downloadLatestPackage() async {
    called = true;
    return Uint8List.fromList([1, 2, 3]);
  }
}

class _Installer implements UpdaterInstaller {
  Uint8List? receivedBytes;

  @override
  Future<void> install(Uint8List packageBytes) async {
    receivedBytes = packageBytes;
  }
}

class _Launcher implements UpdaterLauncher {
  bool launched = false;

  @override
  Future<void> launchDimension() async {
    launched = true;
  }
}

void main() {
  test('runs update flow in expected order and emits statuses', () async {
    final mutex = _Mutex();
    final downloader = _Downloader();
    final installer = _Installer();
    final launcher = _Launcher();
    final messages = <String>[];

    final controller = UpdatingFormController(
      mutex: mutex,
      downloader: downloader,
      installer: installer,
      launcher: launcher,
      onStatusChanged: (status) => messages.add(status.message),
    );

    await controller.run();

    expect(mutex.waited, isTrue);
    expect(downloader.called, isTrue);
    expect(installer.receivedBytes, isNotNull);
    expect(launcher.launched, isTrue);
    expect(controller.status, UpdaterStatus.done);
    expect(messages, [
      UpdaterStatus.waitingForClose.message,
      UpdaterStatus.downloading.message,
      UpdaterStatus.installing.message,
      UpdaterStatus.launching.message,
      UpdaterStatus.done.message,
    ]);
  });
}
