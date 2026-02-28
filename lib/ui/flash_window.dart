enum FlashType {
  all,
}

abstract class FlashWindowDriver {
  const FlashWindowDriver();

  bool flash({int? count, FlashType type = FlashType.all});

  bool start({FlashType type = FlashType.all});

  bool stop();

  bool applicationIsActivated();
}

class NoopFlashWindowDriver extends FlashWindowDriver {
  const NoopFlashWindowDriver({this.isActivated = true});

  final bool isActivated;

  @override
  bool applicationIsActivated() => isActivated;

  @override
  bool flash({int? count, FlashType type = FlashType.all}) => false;

  @override
  bool start({FlashType type = FlashType.all}) => false;

  @override
  bool stop() => false;
}

class FlashWindow {
  static FlashWindowDriver driver = const NoopFlashWindowDriver();

  static bool flash({int? count, FlashType type = FlashType.all}) {
    return driver.flash(count: count, type: type);
  }

  static bool start({FlashType type = FlashType.all}) {
    return driver.start(type: type);
  }

  static bool stop() {
    return driver.stop();
  }

  static bool applicationIsActivated() {
    return driver.applicationIsActivated();
  }

  const FlashWindow._();
}
