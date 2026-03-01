import 'dart:typed_data';

import 'model/commands/private_chat_command.dart';

abstract class AppSettings {
  bool getBool(String key, bool defaultValue);

  String getString(String key, String defaultValue);

  void setString(String key, String value);

  Future<void> save();
}

abstract class AppUpdateService {
  Future<bool> needsUpdate(int buildNumber);

  Future<void> downloadUpdates(int buildNumber);
}

abstract class AppPrompt {
  Future<bool> confirmUpdateAvailable();
}

abstract class AppLifecycle {
  Future<void> exit();
}

abstract class AppLogger {
  void addEntry(String message);
}

abstract class AppBootstrap {
  bool get behindDoubleNAT;

  Future<void> launch();

  Future<void> dispose();
}

abstract class AppCore {
  Future<void> dispose();
}

abstract class AppKademlia {
  Future<void> initialize();

  Future<void> dispose();
}

abstract class AppFileList {
  Future<void> startUpdate(bool urgent);

  Future<void> dispose();
}

abstract class AppFileListDatabase {
  Future<void> close();
}

abstract class AppSpeedLimiter {
  Future<void> dispose();
}

abstract class AppSerializer {
  String getType(Uint8List bytes);
}

class UdpTarget {
  const UdpTarget({required this.host, required this.port});

  final String host;
  final int port;
}

abstract class AppUdpTransport {
  Future<void> send(Uint8List bytes, UdpTarget target);
}

class App {
  App({
    required this.settings,
    required this.updateService,
    required this.prompt,
    required this.lifecycle,
    required this.logger,
    required this.bootstrap,
    required this.core,
    required this.kademlia,
    required this.fileList,
    required this.fileListDatabase,
    required this.speedLimiter,
    required this.serializer,
    required this.udpTransport,
    this.machineName = 'Machine',
    Map<String, String>? environment,
  }) : _environment = environment ?? const <String, String>{};

  static const int buildNumber = 110;

  final AppSettings settings;
  final AppUpdateService updateService;
  final AppPrompt prompt;
  final AppLifecycle lifecycle;
  final AppLogger logger;
  final AppBootstrap bootstrap;
  final AppCore core;
  final AppKademlia kademlia;
  final AppFileList fileList;
  final AppFileListDatabase fileListDatabase;
  final AppSpeedLimiter speedLimiter;
  final AppSerializer serializer;
  final AppUdpTransport udpTransport;
  final String machineName;
  final Map<String, String> _environment;

  final Map<String, int> outgoingTraffic = <String, int>{};
  bool doneLoading = false;

  bool _checking = false;
  bool _updateBegin = false;
  bool _updateDeclined = false;

  final List<void Function(PrivateChatCommand command, Object? peer)>
  _privateChatHandlers = <void Function(PrivateChatCommand command, Object? peer)>[];
  final List<void Function()> _flashHandlers = <void Function()>[];

  bool get isMono =>
      _environment.containsKey('MONO_ENV_OPTIONS') ||
      _environment.containsKey('MONO_PATH');

  bool get comicSansOnly => bootstrap.behindDoubleNAT;

  Future<void> downloadUpdates() {
    return updateService.downloadUpdates(buildNumber);
  }

  Future<bool> checkForUpdates() async {
    if (_checking) {
      return false;
    }
    _checking = true;
    try {
      if (settings.getBool('Update Without Prompting', false)) {
        final needsUpdate = await updateService.needsUpdate(buildNumber);
        if (needsUpdate) {
          if (_updateBegin) {
            return false;
          }
          _updateBegin = true;
          await downloadUpdates();
          await lifecycle.exit();
          return true;
        }
      }

      if (_updateDeclined) {
        return false;
      }

      final needsUpdate = await updateService.needsUpdate(buildNumber);
      if (!needsUpdate) {
        return false;
      }

      final accepted = await prompt.confirmUpdateAvailable();
      if (!accepted) {
        _updateDeclined = true;
        return false;
      }

      if (_updateBegin) {
        return false;
      }

      _updateBegin = true;
      await downloadUpdates();
      await lifecycle.exit();
      return true;
    } finally {
      _checking = false;
    }
  }

  Future<void> doCleanup() async {
    await kademlia.dispose();
    await fileList.dispose();
    await fileListDatabase.close();
    await bootstrap.dispose();
    await speedLimiter.dispose();
    await core.dispose();
  }

  Future<void> udpSend(Uint8List bytes, UdpTarget target) async {
    final type = serializer.getType(bytes);
    outgoingTraffic[type] = (outgoingTraffic[type] ?? 0) + bytes.length;
    await udpTransport.send(bytes, target);
  }

  Future<void> udpSendWithLength(
    Uint8List bytes,
    int length,
    UdpTarget target,
  ) async {
    final boundedLength = length.clamp(0, bytes.length);
    final payload = Uint8List.sublistView(bytes, 0, boundedLength);
    await udpSend(payload, target);
  }

  Future<void> doLoad() async {
    logger.addEntry('---------------');
    logger.addEntry('');
    logger.addEntry('Startup');

    final username = settings.getString('Username', machineName);
    settings.setString('Username', username);

    logger.addEntry('Setting up NAT...');
    await bootstrap.launch();

    logger.addEntry('Starting Kademlia launching...');
    if (!isMono) {
      await kademlia.initialize();
    }

    logger.addEntry('Starting a file list update...');
    await fileList.startUpdate(false);

    logger.addEntry('Saving settings...');
    await settings.save();

    logger.addEntry('Done loading!');
    doneLoading = true;
  }

  void addPrivateChatReceivedListener(
    void Function(PrivateChatCommand command, Object? peer) listener,
  ) {
    _privateChatHandlers.add(listener);
  }

  void removePrivateChatReceivedListener(
    void Function(PrivateChatCommand command, Object? peer) listener,
  ) {
    _privateChatHandlers.remove(listener);
  }

  void doPrivateChatReceived(PrivateChatCommand command, Object? peer) {
    for (final handler in _privateChatHandlers) {
      handler(command, peer);
    }
  }

  void addFlashListener(void Function() listener) {
    _flashHandlers.add(listener);
  }

  void removeFlashListener(void Function() listener) {
    _flashHandlers.remove(listener);
  }

  void doFlash() {
    for (final handler in _flashHandlers) {
      handler();
    }
  }
}
