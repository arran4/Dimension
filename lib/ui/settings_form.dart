import 'package:flutter/material.dart';

abstract class SettingsFormController {
  String get username;

  String get description;

  bool get playSounds;

  Future<void> save({
    required String username,
    required String description,
    required bool playSounds,
  });
}

class SettingsForm extends StatefulWidget {
  const SettingsForm({super.key, required this.controller});

  final SettingsFormController controller;

  @override
  State<SettingsForm> createState() => _SettingsFormState();
}

class _SettingsFormState extends State<SettingsForm> {
  late final TextEditingController _usernameController;
  late final TextEditingController _descriptionController;
  late bool _playSounds;

  @override
  void initState() {
    super.initState();
    _usernameController = TextEditingController(text: widget.controller.username);
    _descriptionController = TextEditingController(
      text: widget.controller.description,
    );
    _playSounds = widget.controller.playSounds;
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Settings'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextField(
            key: const Key('settings.username'),
            controller: _usernameController,
            decoration: const InputDecoration(labelText: 'Username'),
          ),
          TextField(
            key: const Key('settings.description'),
            controller: _descriptionController,
            decoration: const InputDecoration(labelText: 'Description'),
          ),
          SwitchListTile(
            key: const Key('settings.playSounds'),
            title: const Text('Play sounds'),
            value: _playSounds,
            onChanged: (value) => setState(() => _playSounds = value),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(
          onPressed: () async {
            await widget.controller.save(
              username: _usernameController.text,
              description: _descriptionController.text,
              playSounds: _playSounds,
            );
            if (context.mounted) {
              Navigator.of(context).pop();
            }
          },
          child: const Text('Save'),
        ),
      ],
    );
  }
}
