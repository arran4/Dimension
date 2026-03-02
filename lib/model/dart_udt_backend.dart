import 'dart:async';
import 'dart:typed_data';

import 'package:dart_udt/dart_udt.dart';

import 'udt_backend.dart';
import 'udt_connection.dart';

/// A Dart backend that uses the `dart_udt` package.
/// Currently stubbed out as the package does not yet have full session/socket parity.
class DartUdtBackend implements UdtBackend {
  const DartUdtBackend();

  @override
  Future<UdtTransport> connect(UdtEndpoint endpoint) async {
    const profileBuilder = UdtCompatibilityProfileBuilder();
    final profile = profileBuilder.build(
      platform: 'linux', // Using dummy platform for now
      ipMode: UdtIpMode.ipv4Only,
      ipv6: false,
      mobileInput: const UdtMobilePolicyInput(
        appState: UdtMobileAppState.foreground,
        networkType: UdtMobileNetworkType.wifi,
        allowBackgroundNetwork: true,
        batterySaverEnabled: false,
      ),
    );

    // TODO: Use dart_udt's profile to execute real socket binding once it has a full session/socket parity.
    throw UnimplementedError(
        'dart_udt package lacks full session parity to connect to $endpoint with $profile.');
  }
}

class RealUdtTransport implements UdtTransport {
  RealUdtTransport(this.endpoint);

  final UdtEndpoint endpoint;

  @override
  Stream<Uint8List> get incomingPackets =>
      throw UnimplementedError('dart_udt lacks session parity.');

  @override
  bool get isConnected => false;

  @override
  bool get isConnecting => false;

  @override
  Future<void> close() async {
    throw UnimplementedError('dart_udt lacks session parity.');
  }

  @override
  Future<void> send(Uint8List packet) async {
    throw UnimplementedError('dart_udt lacks session parity.');
  }
}
