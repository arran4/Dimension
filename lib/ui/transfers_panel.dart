import 'dart:io' show Platform;

import 'package:flutter/material.dart';

import '../model/incoming_connection.dart';
import '../model/outgoing_connection.dart';
import '../model/transfer.dart';
import 'byte_formatter.dart';

abstract class TransferLimitProvider {
  int getUploadLimitBytesPerSecond();

  int getDownloadLimitBytesPerSecond();
}

class InMemoryTransferLimitProvider implements TransferLimitProvider {
  InMemoryTransferLimitProvider({
    this.uploadLimitBytesPerSecond = 0,
    this.downloadLimitBytesPerSecond = 0,
  });

  int uploadLimitBytesPerSecond;
  int downloadLimitBytesPerSecond;

  @override
  int getDownloadLimitBytesPerSecond() => downloadLimitBytesPerSecond;

  @override
  int getUploadLimitBytesPerSecond() => uploadLimitBytesPerSecond;
}

class TransfersPanelRow {
  const TransfersPanelRow({
    required this.name,
    required this.direction,
    required this.user,
    required this.speed,
    required this.limit,
    required this.eta,
    required this.done,
    required this.percent,
    required this.size,
    required this.protocol,
  });

  final String name;
  final String direction;
  final String user;
  final String speed;
  final String limit;
  final String eta;
  final String done;
  final String percent;
  final String size;
  final String protocol;
}

class TransfersPanelLogic {
  List<TransfersPanelRow> buildRows({
    required List<Transfer> transfers,
    required TransferLimitProvider limitProvider,
    DateTime? now,
  }) {
    final timestamp = now ?? DateTime.now();
    final uploadLimit = limitProvider.getUploadLimitBytesPerSecond();
    final downloadLimit = limitProvider.getDownloadLimitBytesPerSecond();

    return transfers
        .map(
          (transfer) => TransfersPanelRow(
            name: transfer.filename,
            direction: transfer.download ? 'Downloading' : 'Uploading',
            user: transfer.username,
            speed: '${ByteFormatter.formatBytes(transfer.rate)}/s',
            limit: _limitTextForTransfer(
              transfer,
              uploadLimitBytesPerSecond: uploadLimit,
              downloadLimitBytesPerSecond: downloadLimit,
            ),
            eta: _etaTextForTransfer(transfer, timestamp),
            done: ByteFormatter.formatBytes(transfer.completed),
            percent: _percentForTransfer(transfer),
            size: ByteFormatter.formatBytes(transfer.size),
            protocol: transfer.protocol,
          ),
        )
        .toList(growable: false);
  }

  String _percentForTransfer(Transfer transfer) {
    if (transfer.size <= 0 || transfer.completed <= 0) {
      return '0%';
    }
    final ratio = (100.0 * transfer.completed) / transfer.size;
    return '${ratio.floor()}%';
  }

  String _etaTextForTransfer(Transfer transfer, DateTime now) {
    if (transfer.completed <= transfer.startingByte ||
        transfer.size <= transfer.startingByte) {
      return '00:00:00';
    }

    final timeElapsed = now.difference(transfer.timeCreated);
    final proportionateFraction =
        (transfer.completed - transfer.startingByte) /
        (transfer.size - transfer.startingByte);

    if (proportionateFraction <= 0) {
      return '00:00:00';
    }

    final estimatedTotalSeconds =
        timeElapsed.inSeconds * (1.0 / proportionateFraction);
    final remainingSeconds =
        (estimatedTotalSeconds - timeElapsed.inSeconds).round().clamp(0, 1 << 31)
            as int;

    final hours = (remainingSeconds ~/ 3600).toString().padLeft(2, '0');
    final minutes =
        ((remainingSeconds % 3600) ~/ 60).toString().padLeft(2, '0');
    final seconds = (remainingSeconds % 60).toString().padLeft(2, '0');
    return '$hours:$minutes:$seconds';
  }

  String _limitTextForTransfer(
    Transfer transfer, {
    required int uploadLimitBytesPerSecond,
    required int downloadLimitBytesPerSecond,
  }) {
    if (transfer.con is IncomingConnection &&
        (transfer.con as IncomingConnection).rateLimiterDisabled) {
      return 'Bypassed';
    }
    if (transfer.con is OutgoingConnection &&
        (transfer.con as OutgoingConnection).rateLimiterDisabled) {
      return 'Bypassed';
    }

    final limit = transfer.download
        ? downloadLimitBytesPerSecond
        : uploadLimitBytesPerSecond;
    if (limit <= 0) {
      return 'None';
    }
    return '${ByteFormatter.formatBytes(limit)}/s';
  }

  const TransfersPanelLogic();
}

class TransfersPanel extends StatelessWidget {
  TransfersPanel({
    super.key,
    required this.transfers,
    required this.limitProvider,
    this.logic = const TransfersPanelLogic(),
    bool? isMono,
  }) : _isMonoOverride = isMono;

  final List<Transfer> transfers;
  final TransferLimitProvider limitProvider;
  final TransfersPanelLogic logic;
  final bool? _isMonoOverride;

  bool get isMono => _isMonoOverride ?? _platformIsMono();

  static bool _platformIsMono() {
    return Platform.environment.containsKey('MONO_ENV_OPTIONS') ||
        Platform.environment.containsKey('MONO_PATH');
  }

  @override
  Widget build(BuildContext context) {
    final rows = logic.buildRows(
      transfers: transfers,
      limitProvider: limitProvider,
    );

    if (rows.isEmpty) {
      return const Center(child: Text('No active transfers.'));
    }

    return ListView.separated(
      itemCount: rows.length,
      separatorBuilder: (_, _) => const Divider(height: 1),
      itemBuilder: (context, index) {
        final row = rows[index];
        return ListTile(
          dense: !isMono,
          title: Text(row.name),
          subtitle: Text(
            '${row.direction} • ${row.user} • ${row.speed} • ETA ${row.eta}',
          ),
          trailing: Text('${row.percent} (${row.done}/${row.size})'),
        );
      },
    );
  }
}
