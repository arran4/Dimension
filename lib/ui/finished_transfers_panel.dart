import 'package:flutter/material.dart';

class FinishedTransferRow {
  const FinishedTransferRow({required this.name, required this.status});

  final String name;
  final String status;
}

class FinishedTransfersPanel extends StatelessWidget {
  const FinishedTransfersPanel({super.key, required this.items});

  final List<FinishedTransferRow> items;

  @override
  Widget build(BuildContext context) {
    if (items.isEmpty) {
      return const Center(
        child: Text('No completed transfers yet.'),
      );
    }

    return ListView.separated(
      itemCount: items.length,
      separatorBuilder: (_, _) => const Divider(height: 1),
      itemBuilder: (context, index) {
        final item = items[index];
        return ListTile(
          title: Text(item.name),
          subtitle: Text(item.status),
          leading: const Icon(Icons.check_circle_outline),
        );
      },
    );
  }
}
