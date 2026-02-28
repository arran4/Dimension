import 'package:flutter/material.dart';

class HashProgressSnapshot {
  const HashProgressSnapshot({
    required this.currentFileLabel,
    required this.processedFiles,
    required this.totalFiles,
  });

  final String currentFileLabel;
  final int processedFiles;
  final int totalFiles;

  double? get progress {
    if (totalFiles <= 0) {
      return null;
    }
    final ratio = processedFiles / totalFiles;
    return ratio.clamp(0, 1).toDouble();
  }
}

class HashProgressForm extends StatelessWidget {
  const HashProgressForm({
    super.key,
    required this.snapshot,
    this.onClose,
  });

  final HashProgressSnapshot snapshot;
  final VoidCallback? onClose;

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Hashing progress'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(snapshot.currentFileLabel),
          const SizedBox(height: 12),
          LinearProgressIndicator(value: snapshot.progress),
          const SizedBox(height: 8),
          Text('${snapshot.processedFiles} / ${snapshot.totalFiles} files'),
        ],
      ),
      actions: [
        FilledButton(
          onPressed: onClose ?? () => Navigator.of(context).pop(),
          child: const Text('Close'),
        ),
      ],
    );
  }
}
