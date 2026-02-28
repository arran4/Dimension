import 'dart:typed_data';

abstract class UpdaterMutex {
  Future<void> waitUntilReleased();
}

abstract class UpdaterDownloader {
  Future<Uint8List> downloadLatestPackage();
}

abstract class UpdaterInstaller {
  Future<void> install(Uint8List packageBytes);
}

abstract class UpdaterLauncher {
  Future<void> launchDimension();
}

class UpdaterStatus {
  const UpdaterStatus(this.message);

  final String message;

  static const waitingForClose = UpdaterStatus('Waiting for Dimension to close...');
  static const downloading = UpdaterStatus('Downloading new version of Dimension...');
  static const installing = UpdaterStatus('Installing new version of Dimension...');
  static const launching = UpdaterStatus('Launching new version of Dimension...');
  static const done = UpdaterStatus('Done.');
}

class UpdatingFormController {
  UpdatingFormController({
    required this.mutex,
    required this.downloader,
    required this.installer,
    required this.launcher,
    this.onStatusChanged,
  });

  final UpdaterMutex mutex;
  final UpdaterDownloader downloader;
  final UpdaterInstaller installer;
  final UpdaterLauncher launcher;
  final void Function(UpdaterStatus status)? onStatusChanged;

  UpdaterStatus _status = UpdaterStatus.waitingForClose;

  UpdaterStatus get status => _status;

  Future<void> run() async {
    _setStatus(UpdaterStatus.waitingForClose);
    await mutex.waitUntilReleased();

    _setStatus(UpdaterStatus.downloading);
    final packageBytes = await downloader.downloadLatestPackage();

    _setStatus(UpdaterStatus.installing);
    await installer.install(packageBytes);

    _setStatus(UpdaterStatus.launching);
    await launcher.launchDimension();

    _setStatus(UpdaterStatus.done);
  }

  void _setStatus(UpdaterStatus status) {
    _status = status;
    onStatusChanged?.call(status);
  }
}
