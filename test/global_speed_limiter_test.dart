import 'dart:async';

import 'package:dimension/model/global_speed_limiter.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('returns full amount when limits are disabled', () async {
    final limiter = GlobalSpeedLimiter();

    final upload = await limiter.limitUpload(1024);
    final download = await limiter.limitDownload(2048);

    expect(upload, 1024);
    expect(download, 2048);
    limiter.dispose();
  });

  test('uses injected settings getter for upload and download limits', () async {
    final values = <String, int>{
      'Global Upload Rate Limit': 100,
      'Global Download Rate Limit': 80,
    };

    final limiter = GlobalSpeedLimiter(
      getInt: (key, defaultValue) => values[key] ?? defaultValue,
      tickInterval: const Duration(milliseconds: 5),
    );

    await Future<void>.delayed(const Duration(milliseconds: 10));

    final upload = await limiter.limitUpload(20);
    final download = await limiter.limitDownload(20);

    expect(upload, 10);
    expect(download, 8);
    limiter.dispose();
  });
}

