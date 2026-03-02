import 'package:flutter/material.dart';

class DimensionSectionAction extends StatelessWidget {
  const DimensionSectionAction({
    super.key,
    required this.label,
    required this.semanticsLabel,
    required this.onPressed,
  });

  final String label;
  final String semanticsLabel;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: Semantics(
        button: true,
        label: semanticsLabel,
        child: Tooltip(
          message: semanticsLabel,
          child: TextButton(
            onPressed: onPressed,
            style: ButtonStyle(
              side: WidgetStateProperty.resolveWith<BorderSide?>((states) {
                if (states.contains(WidgetState.focused)) {
                  return const BorderSide(width: 2);
                }
                return null;
              }),
            ),
            child: Text(label),
          ),
        ),
      ),
    );
  }
}

class DimensionSectionHeader extends StatelessWidget {
  const DimensionSectionHeader({
    super.key,
    required this.title,
    this.statusMessage,
    this.actions = const <Widget>[],
    this.busy = false,
    this.containerKey,
  });

  final String title;
  final String? statusMessage;
  final List<Widget> actions;
  final bool busy;
  final Key? containerKey;

  @override
  Widget build(BuildContext context) {
    if (actions.isEmpty && statusMessage == null && !busy) {
      return const SizedBox.shrink();
    }

    final highContrast = MediaQuery.highContrastOf(context);
    final statusStyle = Theme.of(context).textTheme.bodyMedium?.copyWith(
      fontWeight: highContrast ? FontWeight.w700 : FontWeight.w400,
      color: highContrast
          ? Theme.of(context).colorScheme.onSurface
          : Theme.of(context).textTheme.bodyMedium?.color,
    );

    return Column(
      children: [
        Container(
          key: containerKey,
          width: double.infinity,
          color: highContrast
              ? Theme.of(context).colorScheme.surfaceContainerHighest
              : null,
          padding: const EdgeInsets.fromLTRB(12, 8, 12, 0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (actions.isNotEmpty)
                Wrap(spacing: 8, runSpacing: 4, children: actions),
              if (statusMessage != null)
                Padding(
                  padding: const EdgeInsets.only(top: 4),
                  child: Text(
                    statusMessage!,
                    key: Key('section-status.$title'),
                    textAlign: TextAlign.end,
                    style: statusStyle,
                  ),
                ),
            ],
          ),
        ),
        if (busy) const LinearProgressIndicator(),
      ],
    );
  }
}
