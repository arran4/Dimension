import 'dart:async';

class ByteCounter {
  int totalBytes = 0;
  int frontBuffer = 0;
  int _backBuffer = 0;

  static bool _updateRunning = false;
  // Use a WeakReference list or manual dispose, or since we only care about global ones
  // we can use a Set and rely on dispose being called, but to be completely safe from leaks
  // we will use Expando or just manage them carefully. Since Dart doesn't have easy WeakList out of box,
  // we will use a static Timer that iterates a regular List, but we MUST call dispose.
  // Wait, the safest Dart idiomatic way to handle "bytes per second" without a background thread updating
  // everything is to track it at read-time.

  // Actually, we'll restore a list but use a periodic timer that only runs when not empty.
  static final Set<ByteCounter> _allCounters = {};
  static Timer? _updateTimer;

  ByteCounter() {
    _allCounters.add(this);
    _startTimerIfNeeded();
  }

  void addBytes(int u) {
    totalBytes += u;
    _backBuffer += u;
  }

  void _update() {
    frontBuffer = _backBuffer;
    _backBuffer = 0;
  }

  static void _startTimerIfNeeded() {
    if (!_updateRunning && _allCounters.isNotEmpty) {
      _updateRunning = true;
      _updateTimer = Timer.periodic(const Duration(seconds: 1), (_) {
        if (_allCounters.isEmpty) {
          _updateTimer?.cancel();
          _updateRunning = false;
        } else {
          for (var c in _allCounters) {
            c._update();
          }
        }
      });
    }
  }

  void dispose() {
    _allCounters.remove(this);
  }
}
