import 'dart:typed_data';

import 'package:dimension/app.dart';
import 'package:dimension/model/commands/private_chat_command.dart';
import 'package:flutter_test/flutter_test.dart';

class _Settings implements AppSettings {
  final Map<String, Object> values = <String, Object>{};

  @override
  bool getBool(String key, bool defaultValue) =>
      values[key] as bool? ?? defaultValue;

  @override
  String getString(String key, String defaultValue) =>
      values[key] as String? ?? defaultValue;

  @override
  Future<void> save() async {}

  @override
  void setString(String key, String value) {
    values[key] = value;
  }
}

class _UpdateService implements AppUpdateService {
  _UpdateService({required this.needsUpdateValue});

  bool needsUpdateValue;
  int downloadCalls = 0;

  @override
  Future<void> downloadUpdates(int buildNumber) async {
    downloadCalls++;
  }

  @override
  Future<bool> needsUpdate(int buildNumber) async => needsUpdateValue;
}

class _Prompt implements AppPrompt {
  _Prompt(this.result);

  bool result;

  @override
  Future<bool> confirmUpdateAvailable() async => result;
}

class _Lifecycle implements AppLifecycle {
  int exitCalls = 0;

  @override
  Future<void> exit() async {
    exitCalls++;
  }
}

class _Logger implements AppLogger {
  final List<String> entries = <String>[];

  @override
  void addEntry(String message) {
    entries.add(message);
  }
}

class _Bootstrap implements AppBootstrap {
  @override
  bool behindDoubleNAT = false;

  int launchCalls = 0;
  int disposeCalls = 0;

  @override
  Future<void> dispose() async {
    disposeCalls++;
  }

  @override
  Future<void> launch() async {
    launchCalls++;
  }
}

class _Core implements AppCore {
  int disposeCalls = 0;

  @override
  Future<void> dispose() async {
    disposeCalls++;
  }
}

class _Kademlia implements AppKademlia {
  int initCalls = 0;
  int disposeCalls = 0;

  @override
  Future<void> dispose() async {
    disposeCalls++;
  }

  @override
  Future<void> initialize() async {
    initCalls++;
  }
}

class _FileList implements AppFileList {
  int startUpdateCalls = 0;
  int disposeCalls = 0;

  @override
  Future<void> dispose() async {
    disposeCalls++;
  }

  @override
  Future<void> startUpdate(bool urgent) async {
    startUpdateCalls++;
  }
}

class _FileListDb implements AppFileListDatabase {
  int closeCalls = 0;

  @override
  Future<void> close() async {
    closeCalls++;
  }
}

class _SpeedLimiter implements AppSpeedLimiter {
  int disposeCalls = 0;

  @override
  Future<void> dispose() async {
    disposeCalls++;
  }
}

class _Serializer implements AppSerializer {
  @override
  String getType(Uint8List bytes) => 'Mock';
}

class _UdpTransport implements AppUdpTransport {
  int sends = 0;

  @override
  Future<void> send(Uint8List bytes, UdpTarget target) async {
    sends++;
  }
}

App _buildApp({
  required _Settings settings,
  required _UpdateService updateService,
  required _Prompt prompt,
  required _Lifecycle lifecycle,
  required _Logger logger,
  required _Bootstrap bootstrap,
  required _Core core,
  required _Kademlia kademlia,
  required _FileList fileList,
  required _FileListDb fileListDb,
  required _SpeedLimiter speedLimiter,
  required _UdpTransport udp,
  Map<String, String>? environment,
}) {
  return App(
    settings: settings,
    updateService: updateService,
    prompt: prompt,
    lifecycle: lifecycle,
    logger: logger,
    bootstrap: bootstrap,
    core: core,
    kademlia: kademlia,
    fileList: fileList,
    fileListDatabase: fileListDb,
    speedLimiter: speedLimiter,
    serializer: _Serializer(),
    udpTransport: udp,
    machineName: 'workstation',
    environment: environment,
  );
}

void main() {
  test('checkForUpdates auto mode starts updater and exits', () async {
    final settings = _Settings()..values['Update Without Prompting'] = true;
    final updateService = _UpdateService(needsUpdateValue: true);
    final lifecycle = _Lifecycle();

    final app = _buildApp(
      settings: settings,
      updateService: updateService,
      prompt: _Prompt(false),
      lifecycle: lifecycle,
      logger: _Logger(),
      bootstrap: _Bootstrap(),
      core: _Core(),
      kademlia: _Kademlia(),
      fileList: _FileList(),
      fileListDb: _FileListDb(),
      speedLimiter: _SpeedLimiter(),
      udp: _UdpTransport(),
    );

    expect(await app.checkForUpdates(), isTrue);
    expect(updateService.downloadCalls, 1);
    expect(lifecycle.exitCalls, 1);
  });

  test('checkForUpdates prompt decline is sticky', () async {
    final settings = _Settings();
    final prompt = _Prompt(false);

    final app = _buildApp(
      settings: settings,
      updateService: _UpdateService(needsUpdateValue: true),
      prompt: prompt,
      lifecycle: _Lifecycle(),
      logger: _Logger(),
      bootstrap: _Bootstrap(),
      core: _Core(),
      kademlia: _Kademlia(),
      fileList: _FileList(),
      fileListDb: _FileListDb(),
      speedLimiter: _SpeedLimiter(),
      udp: _UdpTransport(),
    );

    expect(await app.checkForUpdates(), isFalse);
    prompt.result = true;
    expect(await app.checkForUpdates(), isFalse);
  });

  test('doLoad initializes services and respects mono setting', () async {
    final kademlia = _Kademlia();
    final fileList = _FileList();
    final logger = _Logger();

    final app = _buildApp(
      settings: _Settings(),
      updateService: _UpdateService(needsUpdateValue: false),
      prompt: _Prompt(false),
      lifecycle: _Lifecycle(),
      logger: logger,
      bootstrap: _Bootstrap(),
      core: _Core(),
      kademlia: kademlia,
      fileList: fileList,
      fileListDb: _FileListDb(),
      speedLimiter: _SpeedLimiter(),
      udp: _UdpTransport(),
      environment: const {'MONO_PATH': '1'},
    );

    await app.doLoad();

    expect(app.isMono, isTrue);
    expect(app.doneLoading, isTrue);
    expect(kademlia.initCalls, 0);
    expect(fileList.startUpdateCalls, 1);
    expect(logger.entries, contains('Done loading!'));
  });

  test('udpSend tracks outgoing traffic and transport send', () async {
    final udp = _UdpTransport();
    final app = _buildApp(
      settings: _Settings(),
      updateService: _UpdateService(needsUpdateValue: false),
      prompt: _Prompt(false),
      lifecycle: _Lifecycle(),
      logger: _Logger(),
      bootstrap: _Bootstrap(),
      core: _Core(),
      kademlia: _Kademlia(),
      fileList: _FileList(),
      fileListDb: _FileListDb(),
      speedLimiter: _SpeedLimiter(),
      udp: udp,
    );

    await app.udpSend(Uint8List.fromList([1, 2, 3]), const UdpTarget(host: 'x', port: 1));
    await app.udpSendWithLength(
      Uint8List.fromList([1, 2, 3, 4]),
      2,
      const UdpTarget(host: 'x', port: 1),
    );

    expect(udp.sends, 2);
    expect(app.outgoingTraffic['Mock'], 5);
  });

  test('private chat and flash listeners are invoked', () {
    final app = _buildApp(
      settings: _Settings(),
      updateService: _UpdateService(needsUpdateValue: false),
      prompt: _Prompt(false),
      lifecycle: _Lifecycle(),
      logger: _Logger(),
      bootstrap: _Bootstrap()..behindDoubleNAT = true,
      core: _Core(),
      kademlia: _Kademlia(),
      fileList: _FileList(),
      fileListDb: _FileListDb(),
      speedLimiter: _SpeedLimiter(),
      udp: _UdpTransport(),
    );

    var flashCalls = 0;
    var privateCalls = 0;
    app.addFlashListener(() => flashCalls++);
    app.addPrivateChatReceivedListener((command, peer) => privateCalls++);

    app.doFlash();
    app.doPrivateChatReceived(PrivateChatCommand()..content = 'hi', Object());

    expect(flashCalls, 1);
    expect(privateCalls, 1);
    expect(app.comicSansOnly, isTrue);
  });

  test('doCleanup disposes all subsystems', () async {
    final bootstrap = _Bootstrap();
    final core = _Core();
    final kademlia = _Kademlia();
    final fileList = _FileList();
    final fileListDb = _FileListDb();
    final speedLimiter = _SpeedLimiter();

    final app = _buildApp(
      settings: _Settings(),
      updateService: _UpdateService(needsUpdateValue: false),
      prompt: _Prompt(false),
      lifecycle: _Lifecycle(),
      logger: _Logger(),
      bootstrap: bootstrap,
      core: core,
      kademlia: kademlia,
      fileList: fileList,
      fileListDb: fileListDb,
      speedLimiter: speedLimiter,
      udp: _UdpTransport(),
    );

    await app.doCleanup();

    expect(kademlia.disposeCalls, 1);
    expect(fileList.disposeCalls, 1);
    expect(fileListDb.closeCalls, 1);
    expect(bootstrap.disposeCalls, 1);
    expect(speedLimiter.disposeCalls, 1);
    expect(core.disposeCalls, 1);
  });
}
