import 'package:flutter/material.dart';

enum WhichLimit { up, down }

abstract class SpeedLimitSettings {
  int getLimitBytesPerSecond(WhichLimit whichLimit);

  Future<void> setLimitBytesPerSecond(WhichLimit whichLimit, int value);
}

class InMemorySpeedLimitSettings extends SpeedLimitSettings {
  final Map<WhichLimit, int> _values;

  InMemorySpeedLimitSettings({Map<WhichLimit, int>? initialValues})
    : _values = Map<WhichLimit, int>.from(initialValues ?? const {});

  @override
  int getLimitBytesPerSecond(WhichLimit whichLimit) => _values[whichLimit] ?? 0;

  @override
  Future<void> setLimitBytesPerSecond(WhichLimit whichLimit, int value) async {
    _values[whichLimit] = value;
  }
}

class LimitSelection {
  const LimitSelection({
    required this.noLimit,
    required this.value,
    required this.unitIndex,
  });

  final bool noLimit;
  final int value;
  final int unitIndex;
}

class LimitChangeLogic {
  static const List<String> units = <String>['B/s', 'KB/s', 'MB/s', 'GB/s'];

  static LimitSelection fromBytesPerSecond(int bytesPerSecond) {
    if (bytesPerSecond <= 0) {
      return const LimitSelection(noLimit: true, value: 0, unitIndex: 0);
    }

    var value = bytesPerSecond;
    var unitIndex = 0;

    while (value > 1024 && unitIndex < units.length - 1) {
      value ~/= 1024;
      unitIndex++;
    }

    return LimitSelection(noLimit: false, value: value, unitIndex: unitIndex);
  }

  static int toBytesPerSecond({
    required bool noLimit,
    required int value,
    required int unitIndex,
  }) {
    if (noLimit) {
      return 0;
    }

    var result = value;
    for (var i = 0; i < unitIndex; i++) {
      result *= 1024;
    }
    return result;
  }

  const LimitChangeLogic._();
}

class LimitChangeDialog extends StatefulWidget {
  const LimitChangeDialog({
    super.key,
    required this.whichLimit,
    required this.settings,
  });

  final WhichLimit whichLimit;
  final SpeedLimitSettings settings;

  static Future<void> show(
    BuildContext context, {
    required WhichLimit whichLimit,
    required SpeedLimitSettings settings,
  }) {
    return showDialog<void>(
      context: context,
      builder: (context) =>
          LimitChangeDialog(whichLimit: whichLimit, settings: settings),
    );
  }

  @override
  State<LimitChangeDialog> createState() => _LimitChangeDialogState();
}

class _LimitChangeDialogState extends State<LimitChangeDialog> {
  final TextEditingController _valueController = TextEditingController();

  bool _noLimit = true;
  int _unitIndex = 0;

  @override
  void initState() {
    super.initState();
    final initial = LimitChangeLogic.fromBytesPerSecond(
      widget.settings.getLimitBytesPerSecond(widget.whichLimit),
    );
    _noLimit = initial.noLimit;
    _unitIndex = initial.unitIndex;
    _valueController.text = initial.value.toString();
  }

  @override
  void dispose() {
    _valueController.dispose();
    super.dispose();
  }

  String get _title {
    return widget.whichLimit == WhichLimit.up
        ? 'Global Upload Rate Limit'
        : 'Global Download Rate Limit';
  }

  Future<void> _saveAndClose() async {
    final parsedValue = int.tryParse(_valueController.text) ?? 0;
    final value = LimitChangeLogic.toBytesPerSecond(
      noLimit: _noLimit,
      value: parsedValue,
      unitIndex: _unitIndex,
    );

    await widget.settings.setLimitBytesPerSecond(widget.whichLimit, value);
    if (!mounted) {
      return;
    }
    Navigator.of(context).pop();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(_title),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          RadioListTile<bool>(
            title: const Text('No limit'),
            value: true,
            groupValue: _noLimit,
            onChanged: (_) => setState(() => _noLimit = true),
            contentPadding: EdgeInsets.zero,
          ),
          RadioListTile<bool>(
            title: const Text('Use limit'),
            value: false,
            groupValue: _noLimit,
            onChanged: (_) => setState(() => _noLimit = false),
            contentPadding: EdgeInsets.zero,
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(
                child: TextField(
                  controller: _valueController,
                  enabled: !_noLimit,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Value',
                  ),
                  onChanged: (_) {
                    if (_noLimit) {
                      setState(() => _noLimit = false);
                    }
                  },
                ),
              ),
              const SizedBox(width: 12),
              DropdownButton<int>(
                value: _unitIndex,
                items: [
                  for (var i = 0; i < LimitChangeLogic.units.length; i++)
                    DropdownMenuItem<int>(
                      value: i,
                      child: Text(LimitChangeLogic.units[i]),
                    ),
                ],
                onChanged: _noLimit
                    ? null
                    : (index) {
                        if (index != null) {
                          setState(() => _unitIndex = index);
                        }
                      },
              ),
            ],
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(
          onPressed: _saveAndClose,
          child: const Text('OK'),
        ),
      ],
    );
  }
}
