import 'dart:io';

import 'commands/command.dart';
import 'commands/private_chat_command.dart';
import 'commands/request_chunks.dart';
import 'commands/request_folder_contents.dart';
import 'commands/reverse_connection_type.dart';
import 'fs_listing.dart';
import 'outgoing_connection.dart';
import 'transfer.dart';

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

typedef PeerPrivateChatHandler = void Function(PrivateChatCommand command);
typedef PeerCommandHandler = void Function(Command command);

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

  OutgoingConnection? controlConnection;
  OutgoingConnection? dataConnection;

  PeerPrivateChatHandler? onPrivateChatReceived;
  PeerCommandHandler? onCommandReceived;

  int uploadRate = 0;
  int downloadRate = 0;
  bool punchActive = false;

  final List<Command> queuedCommands = <Command>[];
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

  Future<void> downloadElement(FSListing listing, {int startingByte = 0}) {
    final path = listing.name.startsWith('/') ? listing.name : '/${listing.name}';
    if (listing.isFolder) {
      final request = RequestFolderContents()..path = path;
      return sendCommand(request);
    }
    return downloadFilePath(path, startingByte: startingByte);
  }

  Future<void> downloadFilePath(String path, {int startingByte = 0}) {
    final request = RequestChunks()
      ..path = path
      ..startingByte = startingByte
      ..allChunks = true;
    return sendCommand(request);
  }

  void commandReceived(Command command) {
    if (command is PrivateChatCommand) {
      chatReceived(command);
    }
    onCommandReceived?.call(command);
  }

  void chatReceived(PrivateChatCommand command) {
    onPrivateChatReceived?.call(command);
  }

  void updateTransfers([Iterable<Transfer>? transferSnapshot]) {
    final transfers = transferSnapshot ?? Transfer.transfers;
    var up = 0;
    var down = 0;

    for (final transfer in transfers) {
      if (transfer.userId != id) {
        continue;
      }
      if (transfer.download) {
        down += transfer.rate;
      } else {
        up += transfer.rate;
      }
    }

    uploadRate = up;
    downloadRate = down;
  }

  Future<void> sendCommand(Command command) async {
    final connection = controlConnection;
    if (connection == null || !connection.connected) {
      queuedCommands.add(command);
      return;
    }
    await connection.send(command);
  }

  Future<void> reverseConnect({bool makeControl = true, bool makeData = true}) {
    final reverse = ReverseConnectionType()
      ..id = id
      ..makeControl = makeControl
      ..makeData = makeData;
    return sendCommand(reverse);
  }

  Future<void> createConnection({
    OutgoingConnection? control,
    OutgoingConnection? data,
  }) async {
    if (control != null) {
      controlConnection = control;
      control.commandReceived = commandReceived;
    }
    if (data != null) {
      dataConnection = data;
      data.commandReceived = commandReceived;
    }

    await _flushQueuedCommands();
  }

  void endPunch() {
    punchActive = false;
  }

  void releasePunch() {
    punchActive = false;
  }

  Future<void> _flushQueuedCommands() async {
    if (queuedCommands.isEmpty) {
      return;
    }
    final pending = List<Command>.from(queuedCommands);
    queuedCommands.clear();
    for (final command in pending) {
      await sendCommand(command);
    }
  }

  static PeerEndpoint? _toEndpoint(dynamic endpoint) {
    if (endpoint is PeerEndpoint) {
      return endpoint;
    }

    if (endpoint is InternetAddressEndpoint) {
      return PeerEndpoint(address: endpoint.address.address, port: endpoint.port);
    }

    try {
      final dynamicEndpoint = endpoint as dynamic;
      final dynamic addressValue = dynamicEndpoint.address;
      final dynamic portValue = dynamicEndpoint.port;
      if (addressValue != null && portValue is int) {
        return PeerEndpoint(address: addressValue.toString(), port: portValue);
      }
    } catch (_) {
      return null;
    }

    return null;
  }
}
