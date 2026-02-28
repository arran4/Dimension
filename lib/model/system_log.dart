import 'dart:async';

/// Pure-Dart port of `SystemLog.cs` with injectable write/disposal hooks.
class SystemLog {
  SystemLog._();

  static String lastLine = '';
  static String theLog = '';

  static DateTime Function() _now = DateTime.now;
  static bool Function() _isDisposed = () => false;
  static FutureOr<void> Function(String message)? _writer;

  static void configure({
    DateTime Function()? now,
    bool Function()? isDisposed,
    FutureOr<void> Function(String message)? writer,
  }) {
    if (now != null) {
      _now = now;
    }
    if (isDisposed != null) {
      _isDisposed = isDisposed;
    }
    _writer = writer;
  }

  static void resetForTests() {
    lastLine = '';
    theLog = '';
    _now = DateTime.now;
    _isDisposed = () => false;
    _writer = null;
  }

  static Future<void> addEntry(String message) async {
    if (_isDisposed()) {
      return;
    }

    lastLine = message;
    final now = _now();
    theLog += '${_shortDate(now)} ${_shortTime(now)} - $message\n';

    final writer = _writer;
    if (writer != null) {
      await writer(message);
    }
  }

  static String _shortDate(DateTime value) {
    final month = value.month.toString().padLeft(2, '0');
    final day = value.day.toString().padLeft(2, '0');
    return '${value.year}-$month-$day';
  }

  static String _shortTime(DateTime value) {
    final hour = value.hour.toString().padLeft(2, '0');
    final minute = value.minute.toString().padLeft(2, '0');
    final second = value.second.toString().padLeft(2, '0');
    return '$hour:$minute:$second';
  }
}
