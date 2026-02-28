import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

enum JoinCircleType { bootstrap, lan }

class JoinCircleRequest {
  const JoinCircleRequest({required this.url, required this.type});

  final String url;
  final JoinCircleType type;
}

JoinCircleRequest? parseDimensionJoinLink(String rawLink) {
  final normalized = rawLink.trim();
  final lower = normalized.toLowerCase();

  const bootstrapPrefix = 'dimensionbootstrap://';
  const lanPrefix = 'dimensionlan://';

  if (lower.startsWith(bootstrapPrefix)) {
    final host = normalized.substring(bootstrapPrefix.length);
    return JoinCircleRequest(
      url: 'http://$host',
      type: JoinCircleType.bootstrap,
    );
  }

  if (lower.startsWith(lanPrefix)) {
    final host = normalized.substring(lanPrefix.length);
    return JoinCircleRequest(
      url: 'http://$host',
      type: JoinCircleType.lan,
    );
  }

  return null;
}

class HTMLPanel extends StatelessWidget {
  const HTMLPanel({super.key, this.onJoinCircle});

  final ValueChanged<JoinCircleRequest>? onJoinCircle;

  static bool get isMono => !kIsWeb && (defaultTargetPlatform == TargetPlatform.linux);

  static const bootstrapPrimaryLink =
      'DimensionBootstrap://www.9thcircle.net/Projects/Dimension/bootstrap.php';
  static const bootstrapSecondaryLink =
      'DimensionBootstrap://www.respawn.com.au/dimension.php';
  static const lanLink = 'DimensionLAN://LAN';

  void _handleTap(String rawLink) {
    final request = parseDimensionJoinLink(rawLink);
    if (request != null) {
      onJoinCircle?.call(request);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Welcome to Dimension!', style: Theme.of(context).textTheme.headlineSmall),
            const SizedBox(height: 8),
            const Text(
              "It's still young, so there aren't many networks to join. Check these ones out:",
            ),
            const SizedBox(height: 16),
            Text('List of Bootstraps', style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 8),
            _JoinLinkRow(
              label: '9th Circle Test Bootstrap',
              rawLink: bootstrapPrimaryLink,
              onTap: _handleTap,
            ),
            _JoinLinkRow(
              label: 'Respawn LAN Bootstrap',
              rawLink: bootstrapSecondaryLink,
              onTap: _handleTap,
            ),
            _JoinLinkRow(
              label: 'Your local network',
              rawLink: lanLink,
              onTap: _handleTap,
            ),
          ],
        ),
      ),
    );
  }
}

class _JoinLinkRow extends StatelessWidget {
  const _JoinLinkRow({
    required this.label,
    required this.rawLink,
    required this.onTap,
  });

  final String label;
  final String rawLink;
  final ValueChanged<String> onTap;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: InkWell(
        onTap: () => onTap(rawLink),
        child: Text(
          label,
          style: TextStyle(
            color: Theme.of(context).colorScheme.primary,
            decoration: TextDecoration.underline,
          ),
        ),
      ),
    );
  }
}
