import 'package:dimension/model/system_log.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  tearDown(SystemLog.resetForTests);

  test('addEntry updates last line and appends timestamped log', () async {
    final writes = <String>[];
    SystemLog.configure(
      now: () => DateTime(2026, 2, 28, 10, 30, 45),
      writer: writes.add,
    );

    await SystemLog.addEntry('hello world');

    expect(SystemLog.lastLine, 'hello world');
    expect(SystemLog.theLog, contains('2026-02-28 10:30:45 - hello world'));
    expect(writes, <String>['hello world']);
  });

  test('addEntry is ignored when app is disposed', () async {
    SystemLog.configure(isDisposed: () => true);

    await SystemLog.addEntry('ignored');

    expect(SystemLog.lastLine, isEmpty);
    expect(SystemLog.theLog, isEmpty);
  });
}
