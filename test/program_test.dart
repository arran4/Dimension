import 'package:dimension/program.dart';
import 'package:flutter_test/flutter_test.dart';

class _Guard implements SingleInstanceGuard {
  _Guard(this.acquire);

  final bool acquire;
  bool released = false;

  @override
  void release() => released = true;

  @override
  bool tryAcquire() => acquire;
}

class _Lifecycle implements StartupLifecycle {
  int loadingCalls = 0;
  int mainCalls = 0;
  int cleanupCalls = 0;
  bool throwOnShowMain = false;

  @override
  Future<void> cleanup() async => cleanupCalls++;

  @override
  Future<void> showLoading() async => loadingCalls++;

  @override
  Future<void> showMain() async {
    mainCalls++;
    if (throwOnShowMain) {
      throw StateError('boom');
    }
  }
}

class _Logger implements ProgramLogger {
  final entries = <String>[];

  @override
  void addEntry(String message) => entries.add(message);
}


class _LatestBuildProvider implements LatestBuildProvider {
  _LatestBuildProvider(this.value);
  final int? value;

  @override
  Future<int?> latestBuildNumber() async => value;
}

class _UpdateGate implements UpdateGate {
  _UpdateGate(this.value);
  final bool value;

  @override
  Future<bool> shouldStartUpdater() async => value;
}

void main() {
  test('returns alreadyRunning when mutex cannot be acquired', () async {
    final guard = _Guard(false);
    final lifecycle = _Lifecycle();
    final logger = _Logger();

    final program = Program(
      instanceGuard: guard,
      startupLifecycle: lifecycle,
      logger: logger,
    );

    expect(await program.run(), ProgramRunResult.alreadyRunning);
    expect(lifecycle.loadingCalls, 0);
    expect(logger.entries, contains('Dimension is already running.'));
    expect(guard.released, isFalse);
  });

  test('handles startup exceptions and still releases guard', () async {
    final guard = _Guard(true);
    final lifecycle = _Lifecycle()..throwOnShowMain = true;
    final logger = _Logger();

    final program = Program(
      instanceGuard: guard,
      startupLifecycle: lifecycle,
      logger: logger,
    );

    expect(await program.run(), ProgramRunResult.crashed);
    expect(guard.released, isTrue);
    expect(logger.entries.first, 'Exception!');
  });

  test('needsUpdate compares build numbers from provider', () async {
    expect(
      await Program.needsUpdate(100, provider: _LatestBuildProvider(110)),
      isTrue,
    );
    expect(
      await Program.needsUpdate(100, provider: _LatestBuildProvider(100)),
      isFalse,
    );
    expect(
      await Program.needsUpdate(100, provider: _LatestBuildProvider(null)),
      isFalse,
    );
    expect(Program.downloadPath(), contains('9thcircle.net'));
  });

  test('short-circuits startup when update gate requests updater', () async {
    final guard = _Guard(true);
    final lifecycle = _Lifecycle();

    final program = Program(
      instanceGuard: guard,
      startupLifecycle: lifecycle,
      logger: _Logger(),
      updateGate: _UpdateGate(true),
    );

    expect(await program.run(), ProgramRunResult.updateStarted);
    expect(lifecycle.loadingCalls, 0);
    expect(guard.released, isFalse);
  });
}
