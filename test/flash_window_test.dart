import 'package:dimension/ui/flash_window.dart';
import 'package:flutter_test/flutter_test.dart';

class _RecordingDriver extends FlashWindowDriver {
  int flashCalls = 0;
  int? flashCount;
  int startCalls = 0;
  int stopCalls = 0;

  @override
  bool applicationIsActivated() => false;

  @override
  bool flash({int? count, FlashType type = FlashType.all}) {
    flashCalls++;
    flashCount = count;
    return true;
  }

  @override
  bool start({FlashType type = FlashType.all}) {
    startCalls++;
    return true;
  }

  @override
  bool stop() {
    stopCalls++;
    return true;
  }
}

void main() {
  test('delegates flash/start/stop and activation queries to configured driver', () {
    final previousDriver = FlashWindow.driver;
    final driver = _RecordingDriver();
    FlashWindow.driver = driver;

    expect(FlashWindow.applicationIsActivated(), isFalse);
    expect(FlashWindow.flash(count: 3), isTrue);
    expect(FlashWindow.start(), isTrue);
    expect(FlashWindow.stop(), isTrue);

    expect(driver.flashCalls, 1);
    expect(driver.flashCount, 3);
    expect(driver.startCalls, 1);
    expect(driver.stopCalls, 1);

    FlashWindow.driver = previousDriver;
  });
}
