class PeerEndpoint {
  const PeerEndpoint({required this.address, required this.port});

  final String address;
  final int port;

  @override
  bool operator ==(Object other) =>
      other is PeerEndpoint && other.address == address && other.port == port;

  @override
  int get hashCode => Object.hash(address, port);
}

class Peer {
  int id = 0;
  bool? afk;
  List<int> circles = [];
  bool isLocal = false;
  DateTime lastContact = DateTime.now();
  bool maybeDead = false;
  bool probablyDead = false;
  bool assumingDead = false;
  String? publicAddress;
  bool quit = false;
  DateTime timeQuit = DateTime.fromMillisecondsSinceEpoch(0);
  dynamic actualEndpoint;
  bool behindDoubleNAT = false;
  int buildNumber = 0;
  String description = '';
  int externalControlPort = 0;
  int externalDataPort = 0;
  List<String> internalAddress = [];
  int localControlPort = 0;
  int localDataPort = 0;
  int localUDTPort = 0;
  Map<int, int> peerCount = {};
  int share = 0;
  bool useUDT = false;
  String username = '';

  final List<PeerEndpoint> _endpointHistory = <PeerEndpoint>[];

  bool endpointIsInHistory(PeerEndpoint endpoint) {
    return _endpointHistory.contains(endpoint);
  }

  void addEndpointToHistory(dynamic endpoint) {
    final normalized = _toEndpoint(endpoint);
    if (normalized == null || endpointIsInHistory(normalized)) {
      return;
    }
    _endpointHistory.add(normalized);
    if (_endpointHistory.length > 16) {
      _endpointHistory.removeAt(0);
    }
  }

  static PeerEndpoint? _toEndpoint(dynamic endpoint) {
    if (endpoint is PeerEndpoint) {
      return endpoint;
    }
    return null;
  }
}
