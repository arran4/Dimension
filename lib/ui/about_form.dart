import 'package:flutter/material.dart';

class AboutForm extends StatelessWidget {
  const AboutForm({
    super.key,
    this.applicationName = 'Dimension',
    this.versionLabel = '',
    this.description =
        'A Flutter port of the Dimension peer-to-peer client.',
  });

  final String applicationName;
  final String versionLabel;
  final String description;

  static Future<void> show(
    BuildContext context, {
    String applicationName = 'Dimension',
    String versionLabel = '',
    String description =
        'A Flutter port of the Dimension peer-to-peer client.',
  }) {
    return showDialog<void>(
      context: context,
      builder: (_) => AboutForm(
        applicationName: applicationName,
        versionLabel: versionLabel,
        description: description,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text('About $applicationName'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            applicationName,
            style: Theme.of(context).textTheme.titleMedium,
          ),
          if (versionLabel.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(versionLabel),
          ],
          const SizedBox(height: 12),
          Text(description),
        ],
      ),
      actions: [
        FilledButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Close'),
        ),
      ],
    );
  }
}
