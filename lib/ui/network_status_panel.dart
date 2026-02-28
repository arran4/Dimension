import 'package:dimension/ui/byte_formatter.dart';
import 'package:flutter/material.dart';

class NetworkPortsInfo {
  const NetworkPortsInfo({
    required this.internalIpAddresses,
    required this.externalIpAddress,
    required this.internalTcpPort,
    required this.externalTcpPort,
    required this.internalUdpPort,
    required this.externalUdpPort,
    required this.internalKademliaPort,
    required this.externalKademliaPort,
  });

  final List<String> internalIpAddresses;
  final String externalIpAddress;
  final int internalTcpPort;
  final int externalTcpPort;
  final int internalUdpPort;
  final int externalUdpPort;
  final int internalKademliaPort;
  final int externalKademliaPort;
}

class NetworkEventsInfo {
  const NetworkEventsInfo({
    required this.udpCommandsNotFromUs,
    required this.incomingTcpConnections,
    required this.incomingUdtConnections,
    required this.successfulOutgoingTcpConnections,
    required this.successfulOutgoingUdtConnections,
  });

  final int udpCommandsNotFromUs;
  final int incomingTcpConnections;
  final int incomingUdtConnections;
  final int successfulOutgoingTcpConnections;
  final int successfulOutgoingUdtConnections;
}

class NetworkStatusSnapshot {
  const NetworkStatusSnapshot({
    required this.ports,
    required this.events,
    required this.incomingTrafficByCommand,
    required this.outgoingTrafficByCommand,
    required this.systemLog,
  });

  final NetworkPortsInfo ports;
  final NetworkEventsInfo events;
  final Map<String, int> incomingTrafficByCommand;
  final Map<String, int> outgoingTrafficByCommand;
  final String systemLog;
}

abstract class NetworkStatusProvider {
  NetworkStatusSnapshot get snapshot;
}

class NetworkStatusFormatter {
  static String formatPorts(NetworkPortsInfo ports) {
    final buffer = StringBuffer()
      ..writeln('Internal IP Addresses: ${ports.internalIpAddresses.join(' ')}')
      ..writeln('External IP Address: ${ports.externalIpAddress}')
      ..writeln('Internal TCP Port: ${ports.internalTcpPort}')
      ..writeln('External TCP Port: ${ports.externalTcpPort}')
      ..writeln('Internal UDP Port: ${ports.internalUdpPort}')
      ..writeln('External UDP Port: ${ports.externalUdpPort}')
      ..writeln('Internal Kademlia (UDP) Port: ${ports.internalKademliaPort}')
      ..write('External Kademlia (UDP) Port: ${ports.externalKademliaPort}');

    return buffer.toString();
  }

  static String formatEvents(NetworkEventsInfo events) {
    final buffer = StringBuffer()
      ..writeln(
        'Successful UDP command receives from other machines: '
        '${events.udpCommandsNotFromUs}',
      )
      ..writeln(
        'Successful incoming TCP connections: ${events.incomingTcpConnections}',
      )
      ..writeln(
        'Successful incoming UDT connections: ${events.incomingUdtConnections}',
      )
      ..writeln(
        'Successful outgoing TCP connections: '
        '${events.successfulOutgoingTcpConnections}',
      )
      ..write(
        'Successful outgoing UDT connections: '
        '${events.successfulOutgoingUdtConnections}',
      );

    return buffer.toString();
  }

  static String formatTraffic({
    required Map<String, int> incoming,
    required Map<String, int> outgoing,
  }) {
    final buffer = StringBuffer()
      ..writeln('*** Protocol Traffic Analysis ***')
      ..writeln('(Excluding bulk data such as file transfers)')
      ..writeln()
      ..writeln('*** Incoming ***');

    for (final entry in incoming.entries) {
      buffer.writeln('${entry.key}: ${ByteFormatter.formatBytes(entry.value)}');
    }

    buffer
      ..writeln()
      ..writeln('*** Outgoing ***');

    for (final entry in outgoing.entries) {
      buffer.writeln('${entry.key}: ${ByteFormatter.formatBytes(entry.value)}');
    }

    return buffer.toString().trimRight();
  }

  const NetworkStatusFormatter._();
}

class NetworkStatusPanel extends StatelessWidget {
  const NetworkStatusPanel({super.key, required this.provider});

  final NetworkStatusProvider provider;

  @override
  Widget build(BuildContext context) {
    final snapshot = provider.snapshot;

    return LayoutBuilder(
      builder: (context, constraints) {
        final compact = constraints.maxWidth < 900;
        final sections = [
          _InfoSection(
            title: 'Ports',
            text: NetworkStatusFormatter.formatPorts(snapshot.ports),
          ),
          _InfoSection(
            title: 'Events',
            text: NetworkStatusFormatter.formatEvents(snapshot.events),
          ),
          _InfoSection(
            title: 'Traffic',
            text: NetworkStatusFormatter.formatTraffic(
              incoming: snapshot.incomingTrafficByCommand,
              outgoing: snapshot.outgoingTrafficByCommand,
            ),
          ),
          _InfoSection(
            title: 'System Log',
            text: snapshot.systemLog,
          ),
        ];

        if (compact) {
          return ListView(
            padding: const EdgeInsets.all(12),
            children: sections,
          );
        }

        return GridView.count(
          crossAxisCount: 2,
          childAspectRatio: 1.9,
          padding: const EdgeInsets.all(12),
          children: sections,
        );
      },
    );
  }
}

class _InfoSection extends StatelessWidget {
  const _InfoSection({required this.title, required this.text});

  final String title;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 8),
            Expanded(
              child: SingleChildScrollView(
                child: SelectableText(text),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
