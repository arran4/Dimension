import 'package:flutter/material.dart';

class DownloadQueuePanel extends StatelessWidget {
  const DownloadQueuePanel({super.key, this.emptyMessage = 'No queued downloads.'});

  final String emptyMessage;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Text(
        emptyMessage,
        style: Theme.of(context).textTheme.bodyMedium,
      ),
    );
  }
}
