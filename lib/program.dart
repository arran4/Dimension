import 'dart:async';

abstract class UpdateGate {
  Future<bool> shouldStartUpdater();
}

abstract class LatestBuildProvider {
  Future<int?> latestBuildNumber();
}

abstract class SingleInstanceGuard {
  bool tryAcquire();

  void release();
}

abstract class StartupLifecycle {
  Future<void> showLoading();

  Future<void> showMain();

  Future<void> cleanup();
}

abstract class ProgramLogger {
  void addEntry(String message);
}

class Program {
  Program({
    required this.instanceGuard,
    required this.startupLifecycle,
    required this.logger,
    this.updateGate,
  });

  final SingleInstanceGuard instanceGuard;
  final StartupLifecycle startupLifecycle;
  final ProgramLogger logger;
  final UpdateGate? updateGate;

  static Future<bool> needsUpdate(
    int currentBuildNumber, {
    required LatestBuildProvider provider,
  }) async {
    final latest = await provider.latestBuildNumber();
    if (latest == null) {
      return false;
    }
    return latest > currentBuildNumber;
  }

  static String downloadPath({
    String baseUrl = 'http://www.9thcircle.net/projects/Dimension/latest',
  }) => baseUrl;

  Future<ProgramRunResult> run() async {
    final gate = updateGate;
    if (gate != null && await gate.shouldStartUpdater()) {
      return ProgramRunResult.updateStarted;
    }

    if (!instanceGuard.tryAcquire()) {
      logger.addEntry('Dimension is already running.');
      return ProgramRunResult.alreadyRunning;
    }

    try {
      await startupLifecycle.showLoading();
      await startupLifecycle.showMain();
      await startupLifecycle.cleanup();
      return ProgramRunResult.started;
    } catch (error, stackTrace) {
      logger.addEntry('Exception!');
      logger.addEntry(error.toString());
      logger.addEntry(stackTrace.toString());
      return ProgramRunResult.crashed;
    } finally {
      instanceGuard.release();
    }
  }
}

enum ProgramRunResult { started, alreadyRunning, updateStarted, crashed }
