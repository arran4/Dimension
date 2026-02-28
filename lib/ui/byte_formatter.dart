class ByteFormatter {
  static const List<String> _suffixes = [
    'B',
    'KB',
    'MB',
    'GB',
    'TB',
    'PB',
    'EB',
    'ZB',
    'YB',
  ];

  static String formatBytes(int bytes) {
    double num = bytes.toDouble();
    int suffixNum = 0;

    while (num > 1024 && suffixNum < _suffixes.length - 1) {
      num /= 1024;
      suffixNum++;
    }

    final rounded = ((num * 100).truncateToDouble()) / 100;
    return '$rounded${_suffixes[suffixNum]}';
  }

  const ByteFormatter._();
}
