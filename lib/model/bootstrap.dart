import 'dart:io';

/// Minimal pure-Dart port of `DimensionLib/Model/Bootstrap.cs`.
///
/// The original class also handled NAT traversal (STUN/UPnP socket setup).
/// That flow still needs to be ported. This implementation focuses on the
/// completed tasks that are currently testable without real networking:
/// logging plumbing and bootstrap endpoint retrieval/parsing.
typedef BootstrapLogger = void Function(String message);
typedef InternetAddressParser = InternetAddress? Function(String input);

abstract class BootstrapHttpClient {
  Future<String> get(Uri uri);
}

abstract class BootstrapNatAdapter {
  Future<BootstrapNatProbeResult> probe();
}

class BootstrapNatProbeResult {
  const BootstrapNatProbeResult({
    required this.upnpActive,
    this.externalIp,
    this.publicControlPort,
  });

  final bool upnpActive;
  final InternetAddress? externalIp;
  final int? publicControlPort;
}

abstract class BootstrapStunClient {
  Future<BootstrapStunResult?> probe();
}

class BootstrapStunResult {
  const BootstrapStunResult({
    required this.publicAddress,
    required this.publicControlPort,
  });

  final InternetAddress publicAddress;
  final int publicControlPort;
}

class Bootstrap {
  Bootstrap({
    BootstrapHttpClient? httpClient,
    BootstrapLogger? logger,
    InternetAddressParser? internetAddressParser,
    BootstrapNatAdapter? natAdapter,
    BootstrapStunClient? stunClient,
  })  : _httpClient = httpClient ?? IoBootstrapHttpClient(),
        _logger = logger,
        _internetAddressParser =
            internetAddressParser ?? InternetAddress.tryParse,
        _natAdapter = natAdapter,
        _stunClient = stunClient;

  final BootstrapHttpClient _httpClient;
  final BootstrapLogger? _logger;
  final InternetAddressParser _internetAddressParser;
  final BootstrapNatAdapter? _natAdapter;
  final BootstrapStunClient? _stunClient;

  bool upnpActive = true;
  bool lanMode = false;
  bool behindDoubleNAT = false;

  InternetAddress? externalIpFromUpnp;
  InternetAddress? publicAddress;
  int? publicControlPort;

  bool _disposed = false;

  bool get disposed => _disposed;

  /// C# compatibility shim.
  void Dispose() => dispose();

  void dispose() {
    _disposed = true;
  }

  Future<void> launch() async {
    final natProbe = await _natAdapter?.probe();
    if (natProbe != null) {
      upnpActive = natProbe.upnpActive;
      externalIpFromUpnp = natProbe.externalIp;
      if (natProbe.publicControlPort != null) {
        publicControlPort = natProbe.publicControlPort;
      }
      if (natProbe.externalIp != null) {
        publicAddress = natProbe.externalIp;
      }
    }

    final stunProbe = await _stunClient?.probe();
    if (stunProbe != null) {
      final upnpIp = externalIpFromUpnp?.address;
      final stunIp = stunProbe.publicAddress.address;
      behindDoubleNAT = upnpIp != null && upnpIp != stunIp;
      publicAddress = stunProbe.publicAddress;
      publicControlPort = stunProbe.publicControlPort;
    } else {
      behindDoubleNAT = false;
    }

    if (publicAddress == null && externalIpFromUpnp != null) {
      publicAddress = externalIpFromUpnp;
    }
  }

  /// C# compatibility shim.
  void WriteLine(String message) => writeLine(message);

  void writeLine(String message) {
    _logger?.call(message);
  }

  /// C# compatibility shim.
  void Write(String message) => write(message);

  void write(String message) {
    _logger?.call(message);
  }

  /// Joins a bootstrap endpoint and returns deduplicated, valid peers.
  ///
  /// Invalid rows are ignored to mirror the original C# behavior.
  Future<List<InternetAddressEndpoint>> join(String address) async {
    final baseUri = Uri.parse(address);
    final response = await _httpClient.get(
      baseUri.replace(
        queryParameters: <String, String>{
          ...baseUri.queryParameters,
          if (publicControlPort != null) 'port': publicControlPort.toString(),
        },
      ),
    );

    final uniqueRows = <String>{};
    final endpoints = <InternetAddressEndpoint>[];

    for (final rawLine in response.split('\n')) {
      final line = rawLine.trim();
      if (line.isEmpty || !uniqueRows.add(line)) {
        continue;
      }

      final parts = line.split(RegExp(r'\s+'));
      if (parts.length < 2) {
        continue;
      }

      final ip = _internetAddressParser(parts[0]);
      final port = int.tryParse(parts[1]);
      if (ip == null || port == null || port < 0 || port > 65535) {
        continue;
      }

      endpoints.add(InternetAddressEndpoint(ip, port));
    }

    return endpoints;
  }
}

class IoBootstrapHttpClient implements BootstrapHttpClient {
  IoBootstrapHttpClient({HttpClient? httpClient})
      : _httpClient = httpClient ?? HttpClient();

  final HttpClient _httpClient;

  @override
  Future<String> get(Uri uri) async {
    final request = await _httpClient.getUrl(uri);
    final response = await request.close();
    return response.transform(const SystemEncoding().decoder).join();
  }
}

class InternetAddressEndpoint {
  const InternetAddressEndpoint(this.address, this.port);

  final InternetAddress address;
  final int port;

  @override
  bool operator ==(Object other) {
    return other is InternetAddressEndpoint &&
        other.address.address == address.address &&
        other.port == port;
  }

  @override
  int get hashCode => Object.hash(address.address, port);

  @override
  String toString() => '${address.address}:$port';
}
