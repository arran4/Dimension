import 'package:dimension/model/bootstrap.dart';
import 'package:flutter/material.dart';

enum CircleType { bootstrap, kademlia, lan }

abstract class JoinCircleService {
  Future<List<InternetAddressEndpoint>> joinBootstrap(String address);

  Future<List<InternetAddressEndpoint>> lookupKademlia(String query);

  bool get isKademliaReady;
}

typedef AddInternetCircle = void Function(
  List<InternetAddressEndpoint> endpoints,
  String input,
  CircleType circleType,
);

class JoinCircleController {
  JoinCircleController({required JoinCircleService service, required AddInternetCircle addInternetCircle})
      : _service = service,
        _addInternetCircle = addInternetCircle;

  final JoinCircleService _service;
  final AddInternetCircle _addInternetCircle;

  bool get kademliaReady => _service.isKademliaReady;

  Future<void> joinCircle(String input, CircleType circleType) async {
    var normalizedInput = input.trim();
    if (normalizedInput.toLowerCase().startsWith('http://lan')) {
      normalizedInput = 'LAN';
    }

    List<InternetAddressEndpoint> endpoints;
    if (circleType == CircleType.bootstrap && normalizedInput != 'LAN') {
      endpoints = await _service.joinBootstrap(normalizedInput);
    } else if (circleType == CircleType.kademlia) {
      endpoints = await _service.lookupKademlia(normalizedInput.toLowerCase());
    } else {
      endpoints = const <InternetAddressEndpoint>[];
    }

    _addInternetCircle(endpoints, normalizedInput, circleType);
  }

  /// C# compatibility shim.
  Future<void> JoinCircle(String input, CircleType circleType) =>
      joinCircle(input, circleType);
}

class JoinCircleForm extends StatefulWidget {
  const JoinCircleForm({
    super.key,
    required this.controller,
    required this.circleType,
    this.initialValue,
    this.onError,
  });

  final JoinCircleController controller;
  final CircleType circleType;
  final String? initialValue;
  final ValueChanged<Object>? onError;

  @override
  State<JoinCircleForm> createState() => _JoinCircleFormState();
}

class _JoinCircleFormState extends State<JoinCircleForm> {
  late final TextEditingController _urlController;
  var _busy = false;

  bool get _kademliaBlocked =>
      widget.circleType == CircleType.kademlia && !widget.controller.kademliaReady;

  @override
  void initState() {
    super.initState();
    _urlController = TextEditingController(
      text: widget.initialValue ??
          (widget.circleType == CircleType.kademlia ? '#Test' : ''),
    );
  }

  @override
  void dispose() {
    _urlController.dispose();
    super.dispose();
  }

  Future<void> _join() async {
    if (_busy || _kademliaBlocked) {
      return;
    }

    setState(() {
      _busy = true;
    });

    try {
      await widget.controller.joinCircle(_urlController.text, widget.circleType);
      if (mounted) {
        Navigator.of(context).pop();
      }
    } catch (error) {
      widget.onError?.call(error);
    } finally {
      if (mounted) {
        setState(() {
          _busy = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Join circle'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          TextField(
            key: const Key('joinCircle.url'),
            controller: _urlController,
            onSubmitted: (_) => _join(),
            decoration: const InputDecoration(labelText: 'Bootstrap URL / circle id'),
          ),
          if (_kademliaBlocked)
            const Padding(
              padding: EdgeInsets.only(top: 8),
              child: Text('Kademlia is still initializing...'),
            ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: _busy ? null : () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(
          key: const Key('joinCircle.submit'),
          onPressed: _busy || _kademliaBlocked ? null : _join,
          child: Text(_busy ? 'Joining...' : 'Join'),
        ),
      ],
    );
  }
}
