import 'incoming_connection.dart';
import 'outgoing_connection.dart';
import 'commands/command.dart';
import 'commands/data_command.dart';
import 'byte_counter.dart';

// Dummy for App global counters
class LoopbackDummyGlobal {
  static final ByteCounter globalUpCounter = ByteCounter();
  static final ByteCounter globalDownCounter = ByteCounter();
  static final dynamic speedLimiter = null; // Replace with App.speedLimiter
  static final dynamic theCore = null; // Replace with App.theCore
}

class LoopbackIncomingConnection extends IncomingConnection {
  final ByteCounter upCounter = ByteCounter();
  final ByteCounter downCounter = ByteCounter();

  final LoopbackOutgoingConnection _outgoing;

  LoopbackIncomingConnection(this._outgoing);

  @override
  Future<void> send(Command c) async {
    if (c is DataCommand) {
      int len = c.data.length;
      LoopbackDummyGlobal.globalUpCounter.addBytes(len);
      upCounter.addBytes(len);
      if (LoopbackDummyGlobal.speedLimiter != null) {
        await LoopbackDummyGlobal.speedLimiter.limitUpload(len, disabled: rateLimiterDisabled);
      }
    }
    _outgoing.received(c);
  }

  void received(Command c) {
    if (c is DataCommand) {
      int len = c.data.length;
      LoopbackDummyGlobal.globalDownCounter.addBytes(len);
      downCounter.addBytes(len);
      // App.speedLimiter.limitDownload(len, rateLimiterDisabled);
    }
    if (commandReceived != null) {
      commandReceived!(c, this);
    }
  }

  @override
  bool get connected => true;
}


class LoopbackOutgoingConnection extends OutgoingConnection {
  final ByteCounter upCounter = ByteCounter();
  final ByteCounter downCounter = ByteCounter();

  late LoopbackIncomingConnection _incoming;

  LoopbackOutgoingConnection() {
    _incoming = LoopbackIncomingConnection(this);
    if (LoopbackDummyGlobal.theCore != null) {
      LoopbackDummyGlobal.theCore.addIncomingConnection(_incoming);
    }
  }

  @override
  Future<void> send(Command c) async {
    if (c is DataCommand) {
      int len = c.data.length;
      LoopbackDummyGlobal.globalUpCounter.addBytes(len);
      upCounter.addBytes(len);
      if (LoopbackDummyGlobal.speedLimiter != null) {
        await LoopbackDummyGlobal.speedLimiter.limitUpload(len, disabled: rateLimiterDisabled);
      }
    }
    _incoming.received(c);
  }

  void received(Command c) {
    if (c is DataCommand) {
      int len = c.data.length;
      LoopbackDummyGlobal.globalDownCounter.addBytes(len);
      downCounter.addBytes(len);
      // App.speedLimiter.limitDownload(len, rateLimiterDisabled);
    }
    if (commandReceived != null) {
      commandReceived!(c);
    }
  }

  @override
  bool get connected => true;
}
