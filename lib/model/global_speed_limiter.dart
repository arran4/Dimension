import 'dart:async';
import 'dart:math';

typedef SpeedLimitProvider = int Function(String key, int defaultValue);

class GlobalSpeedLimiter {
  bool _disposed = false;
  Timer? _updateTimer;

  int _totalUpload = 0;
  int _totalDownload = 0;

  int _currentDownloadLimit = 0;
  int _currentUploadLimit = 0;

  final SpeedLimitProvider _getInt;
  final Duration _tickInterval;

  GlobalSpeedLimiter({
    SpeedLimitProvider? getInt,
    Duration tickInterval = const Duration(milliseconds: 100),
  }) : _getInt = getInt ?? _defaultGetInt,
       _tickInterval = tickInterval {
    _startUpdateLoop();
  }

  void dispose() {
    _disposed = true;
    _updateTimer?.cancel();
  }

  void _startUpdateLoop() {
    _updateTimer = Timer.periodic(_tickInterval, (_) {
      if (_disposed) {
        _updateTimer?.cancel();
        return;
      }

      _totalDownload = 0;
      _totalUpload = 0;

      _currentDownloadLimit = _getInt('Global Download Rate Limit', 0);
      _currentUploadLimit = _getInt('Global Upload Rate Limit', 0);
    });
  }

  Future<int> limitUpload(int amount, {bool disabled = false}) async {
    if (disabled) return amount;

    if (_currentUploadLimit > 0) {
      while (_totalUpload * 10 > _currentUploadLimit && _currentUploadLimit > 0) {
        await Future.delayed(const Duration(milliseconds: 1));
      }
      while (min(amount, _currentUploadLimit - _totalUpload) <= 0 && _currentUploadLimit > 0) {
        await Future.delayed(const Duration(milliseconds: 1));
      }

      if (_currentUploadLimit == 0) return amount;

      _totalUpload += amount;
      return min(amount, ((_currentUploadLimit - (_totalUpload - amount)) / 10).floor());
    } else {
      return amount;
    }
  }

  Future<int> limitDownload(int amount, {bool disabled = false}) async {
    if (disabled) return amount;

    if (_currentDownloadLimit > 0) {
      while (_totalDownload * 10 > _currentDownloadLimit && _currentDownloadLimit > 0) {
        await Future.delayed(const Duration(milliseconds: 1));
      }
      while (min(amount, _currentDownloadLimit - _totalDownload) <= 0 && _currentDownloadLimit > 0) {
        await Future.delayed(const Duration(milliseconds: 1));
      }

      if (_currentDownloadLimit == 0) return amount;

      _totalDownload += amount;
      return min(amount, ((_currentDownloadLimit - (_totalDownload - amount)) / 10).floor());
    } else {
      return amount;
    }
  }
}

int _defaultGetInt(String _, int defaultValue) => defaultValue;
