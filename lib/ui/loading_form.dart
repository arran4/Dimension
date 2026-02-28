import 'dart:async';

import 'package:flutter/material.dart';

abstract class LoadingStatusSource {
  String get statusLine;

  bool get isDone;

  Future<void> startLoading();
}

class LoadingForm extends StatefulWidget {
  const LoadingForm({
    super.key,
    required this.statusSource,
    this.title = 'Dimension',
    this.pollInterval = const Duration(milliseconds: 200),
    this.onFinished,
  });

  final LoadingStatusSource statusSource;
  final String title;
  final Duration pollInterval;
  final VoidCallback? onFinished;

  @override
  State<LoadingForm> createState() => _LoadingFormState();
}

class _LoadingFormState extends State<LoadingForm> {
  Timer? _pollTimer;
  var _started = false;

  @override
  void initState() {
    super.initState();
    _pollTimer = Timer.periodic(widget.pollInterval, (_) => _tick());
  }

  Future<void> _tick() async {
    if (!_started) {
      _started = true;
      await widget.statusSource.startLoading();
      if (!mounted) {
        return;
      }
      setState(() {});
    }

    if (!mounted) {
      return;
    }

    setState(() {});

    if (widget.statusSource.isDone) {
      _pollTimer?.cancel();
      widget.onFinished?.call();
    }
  }

  @override
  void dispose() {
    _pollTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const SizedBox(
                height: 24,
                width: 24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
              const SizedBox(height: 12),
              Text('${widget.title} - ${widget.statusSource.statusLine}'),
            ],
          ),
        ),
      ),
    );
  }
}
