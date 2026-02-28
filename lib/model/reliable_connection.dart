import 'dart:async';
import 'dart:io';
import 'dart:typed_data';

import 'incoming_connection.dart';
import 'outgoing_connection.dart';
import 'commands/command.dart';
import 'commands/data_command.dart';
import 'commands/hello_command.dart';
import 'commands/reverse_connection_type.dart';
import 'byte_counter.dart';

// Note: Requires App, App.theCore, App.serializer, App.globalUpCounter, etc.
// We mock these dependencies or use placeholders for now until they are ported.
// We'll also use dummy globals here to allow it to compile based on existing models if any.

class DummyGlobal {
  static final ByteCounter globalUpCounter = ByteCounter();
  static final ByteCounter globalDownCounter = ByteCounter();
  static final dynamic serializer = null; // Replace with App.serializer
  static final dynamic theCore = null; // Replace with App.theCore
  static final dynamic speedLimiter = null; // Replace with App.speedLimiter
}

class ReliableIncomingConnection extends IncomingConnection {
  final Socket _client;
  StreamSubscription<Uint8List>? _subscription;

  int rate = 0;
  final ByteCounter rateCounter = ByteCounter();
  double _internalRate = 0.0;

  ReliableIncomingConnection(this._client) {
    _startReceiveLoop();
  }

  void _startReceiveLoop() {
    // In C#, it reads a 4-byte length prefix, then the data.
    // In Dart, we can buffer the incoming stream.
    List<int> _buffer = [];
    int? _expectedLength;
    Command? _pendingDataCommand;

    _subscription = _client.listen((data) {
      _buffer.addAll(data);

      while (true) {
        if (_pendingDataCommand != null) {
          int expectedDataLength = (_pendingDataCommand as DataCommand).dataLength;
          if (_buffer.length >= expectedDataLength) {
            final rawData = Uint8List.fromList(_buffer.sublist(0, expectedDataLength));
            _buffer.removeRange(0, expectedDataLength);
            DummyGlobal.globalDownCounter.addBytes(expectedDataLength);

            (_pendingDataCommand as DataCommand).data = rawData;

            if (commandReceived != null) {
              commandReceived!(_pendingDataCommand!, this);
            }
            _pendingDataCommand = null;
          } else {
            break; // Need more data for DataCommand payload
          }
        } else {
          if (_expectedLength == null) {
            if (_buffer.length >= 4) {
              final lenBytes = Uint8List.fromList(_buffer.sublist(0, 4));
              _expectedLength = ByteData.view(lenBytes.buffer).getInt32(0, Endian.little);
              _buffer.removeRange(0, 4);
              DummyGlobal.globalDownCounter.addBytes(4);
            } else {
              break; // Need more data for length
            }
          }

          if (_expectedLength != null && _buffer.length >= _expectedLength!) {
            final packetData = Uint8List.fromList(_buffer.sublist(0, _expectedLength!));
            _buffer.removeRange(0, _expectedLength!);
            DummyGlobal.globalDownCounter.addBytes(_expectedLength!);

            _processCommandData(packetData, (cmd) {
               if (cmd is DataCommand) {
                 _pendingDataCommand = cmd;
               } else {
                 if (commandReceived != null) {
                   commandReceived!(cmd, this);
                 }
               }
            });
            _expectedLength = null; // Reset for next message
          } else {
            break; // Need more data for packet
          }
        }
      }
    },
    onDone: () {
      _cleanup();
    },
    onError: (e) {
      _cleanup();
    });
  }

  void _cleanup() {
    _subscription?.cancel();
    rateCounter.dispose();
  }

  void _processCommandData(Uint8List dataByte, void Function(Command) onCommandParsed) {
    if (DummyGlobal.serializer == null) return;
    try {
      Command c = DummyGlobal.serializer.deserialize(dataByte);

      if (c is ReverseConnectionType) {
        if (DummyGlobal.theCore != null && DummyGlobal.theCore.peerManager != null) {
          for (var p in DummyGlobal.theCore.peerManager.allPeers) {
            if (p.id == c.id) {
              if (c.makeControl) {
                p.controlConnection = ReliableOutgoingConnection(_client);
                p.controlConnection!.commandReceived = p.commandReceived;
              }
              if (c.makeData) {
                p.dataConnection = ReliableOutgoingConnection(_client);
                p.dataConnection!.commandReceived = p.commandReceived;
              }
              DummyGlobal.theCore.removeIncomingConnection(this);
              return;
            }
          }
        }
      }

      if (c is HelloCommand) {
        hello = c;
      }

      onCommandParsed(c);
    } catch (e) {
      // ignore
    }
  }

  @override
  Future<void> send(Command c) async {
    if (c is DataCommand) {
      c.dataLength = c.data.length;
    }
    if (DummyGlobal.serializer == null) return;

    Uint8List b = DummyGlobal.serializer.serialize(c);
    int len = b.length;

    try {
      var byteData = ByteData(4);
      byteData.setInt32(0, len, Endian.little);
      _client.add(byteData.buffer.asUint8List());
      DummyGlobal.globalUpCounter.addBytes(4);

      _client.add(b);
      DummyGlobal.globalUpCounter.addBytes(b.length);

      if (c is DataCommand) {
        int pos = 0;
        while (pos < c.data.length) {
          int amt = c.data.length - pos;
          if (DummyGlobal.speedLimiter != null) {
            amt = await DummyGlobal.speedLimiter.limitUpload(amt, disabled: rateLimiterDisabled);
          }
          if (amt <= 0) {
            await Future.delayed(const Duration(milliseconds: 10));
            continue;
          }

          rateCounter.addBytes(amt);
          _internalRate = (_internalRate * 0.9) + (rateCounter.frontBuffer * 0.1);
          rate = _internalRate.toInt();

          _client.add(c.data.sublist(pos, pos + amt));
          pos += amt;
          DummyGlobal.globalUpCounter.addBytes(amt);
        }
      }
    } catch (e) {
      // ignore
    }
  }

  @override
  bool get connected => true; // Simplified, in Dart Socket doesn't have a direct 'connected' boolean without checking state.
}


class ReliableOutgoingConnection extends OutgoingConnection {
  static int successfulConnections = 0;
  Socket? _client;
  StreamSubscription<Uint8List>? _subscription;

  final ByteCounter downCounter = ByteCounter();
  double _currentRate = 0.0;

  // Since Socket.connect is async, we use a static factory or init method.
  ReliableOutgoingConnection(this._client) {
    _init();
  }

  static Future<ReliableOutgoingConnection> connect(String host, int port) async {
    Socket socket = await Socket.connect(host, port);
    successfulConnections++;
    return ReliableOutgoingConnection(socket);
  }

  void _init() {
    if (DummyGlobal.theCore != null) {
      send(DummyGlobal.theCore.generateHello());
    }
    _startReceiveLoop();
  }

  void _startReceiveLoop() {
    if (_client == null) return;

    List<int> _buffer = [];
    int? _expectedLength;
    Command? _pendingDataCommand;

    _subscription = _client!.listen((data) {
      _buffer.addAll(data);

      while (true) {
        if (_pendingDataCommand != null) {
          int expectedDataLength = (_pendingDataCommand as DataCommand).dataLength;
          if (_buffer.length >= expectedDataLength) {
            final rawData = Uint8List.fromList(_buffer.sublist(0, expectedDataLength));
            _buffer.removeRange(0, expectedDataLength);
            DummyGlobal.globalDownCounter.addBytes(expectedDataLength);

            (_pendingDataCommand as DataCommand).data = rawData;

            int amt = expectedDataLength;
            downCounter.addBytes(amt);
            _currentRate = (_currentRate * 0.9) + (downCounter.frontBuffer * 0.1);
            rate = _currentRate.toInt();

            if (commandReceived != null) {
              commandReceived!(_pendingDataCommand!);
            }
            _pendingDataCommand = null;
          } else {
            break; // Need more data for DataCommand payload
          }
        } else {
          if (_expectedLength == null) {
            if (_buffer.length >= 4) {
              final lenBytes = Uint8List.fromList(_buffer.sublist(0, 4));
              _expectedLength = ByteData.view(lenBytes.buffer).getInt32(0, Endian.little);
              _buffer.removeRange(0, 4);
              DummyGlobal.globalDownCounter.addBytes(4);
            } else {
              break;
            }
          }

          if (_expectedLength != null && _buffer.length >= _expectedLength!) {
            final packetData = Uint8List.fromList(_buffer.sublist(0, _expectedLength!));
            _buffer.removeRange(0, _expectedLength!);
            DummyGlobal.globalDownCounter.addBytes(_expectedLength!);

            _processCommandData(packetData, (cmd) {
               if (cmd is DataCommand) {
                 _pendingDataCommand = cmd;
               } else {
                 if (commandReceived != null) {
                   commandReceived!(cmd);
                 }
               }
            });
            _expectedLength = null;
          } else {
            break;
          }
        }
      }
    },
    onDone: () {
      _cleanup();
    },
    onError: (e) {
      _cleanup();
    });
  }

  void _cleanup() {
    _subscription?.cancel();
    downCounter.dispose();
  }

  void _processCommandData(Uint8List dataByte, void Function(Command) onCommandParsed) {
    if (DummyGlobal.serializer == null) return;
    try {
      Command c = DummyGlobal.serializer.deserialize(dataByte);
      onCommandParsed(c);
    } catch (e) {
      // ignore
    }
  }

  @override
  Future<void> send(Command c) async {
    if (c is DataCommand) {
      c.dataLength = c.data.length;
    }
    if (DummyGlobal.serializer == null || _client == null) return;

    Uint8List b = DummyGlobal.serializer.serialize(c);
    int len = b.length;

    try {
      var byteData = ByteData(4);
      byteData.setInt32(0, len, Endian.little);
      _client!.add(byteData.buffer.asUint8List());
      DummyGlobal.globalUpCounter.addBytes(4);

      _client!.add(b);
      DummyGlobal.globalUpCounter.addBytes(b.length);

      if (c is DataCommand) {
        int pos = 0;
        while (pos < c.data.length) {
          int amt = c.data.length - pos;
          if (DummyGlobal.speedLimiter != null) {
            amt = await DummyGlobal.speedLimiter.limitUpload(amt, disabled: rateLimiterDisabled);
          }
          if (amt <= 0) {
            await Future.delayed(const Duration(milliseconds: 10));
            continue;
          }

          _client!.add(c.data.sublist(pos, pos + amt));
          pos += amt;
          DummyGlobal.globalUpCounter.addBytes(amt);
        }
      }
    } catch (e) {
      // ignore
    }
  }

  @override
  bool get connected => _client != null;
}
