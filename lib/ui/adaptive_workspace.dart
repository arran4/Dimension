import 'package:flutter/material.dart';

import 'core_screens.dart';
import 'platform_plan_infra.dart';

class AdaptiveWorkspace extends StatelessWidget {
  const AdaptiveWorkspace({
    super.key,
    required this.planController,
    required this.screensController,
  });

  final PlatformPlanController planController;
  final CoreScreensController screensController;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        planController.recompute(width: constraints.maxWidth);
        return AnimatedBuilder(
          animation: planController,
          builder: (context, _) {
            final snapshot = planController.snapshot;
            final tabs = CoreScreenSection.values;
            return switch (snapshot.navigationPattern) {
              NavigationPattern.bottomTabs => _BottomTabsWorkspace(
                sections: tabs,
                screensController: screensController,
              ),
              NavigationPattern.rail => _RailWorkspace(
                sections: tabs,
                screensController: screensController,
              ),
              NavigationPattern.splitView => _SplitViewWorkspace(
                sections: tabs,
                screensController: screensController,
              ),
            };
          },
        );
      },
    );
  }
}

class _BottomTabsWorkspace extends StatelessWidget {
  const _BottomTabsWorkspace({
    required this.sections,
    required this.screensController,
  });

  final List<CoreScreenSection> sections;
  final CoreScreensController screensController;

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: sections.length,
      child: Scaffold(
        appBar: AppBar(title: const Text('Dimension Mobile')),
        body: TabBarView(
          children: [
            for (final section in sections)
              _SectionPreview(
                section: section,
                state: screensController.stateFor(section),
              ),
          ],
        ),
        bottomNavigationBar: Material(
          color: Theme.of(context).colorScheme.surface,
          child: TabBar(
            isScrollable: true,
            tabs: [for (final section in sections) Tab(text: _label(section))],
          ),
        ),
      ),
    );
  }
}

class _RailWorkspace extends StatefulWidget {
  const _RailWorkspace({required this.sections, required this.screensController});

  final List<CoreScreenSection> sections;
  final CoreScreensController screensController;

  @override
  State<_RailWorkspace> createState() => _RailWorkspaceState();
}

class _RailWorkspaceState extends State<_RailWorkspace> {
  int _selected = 0;

  @override
  Widget build(BuildContext context) {
    final section = widget.sections[_selected];
    return Scaffold(
      appBar: AppBar(title: const Text('Dimension Desktop')),
      body: Row(
        children: [
          NavigationRail(
            selectedIndex: _selected,
            onDestinationSelected: (index) => setState(() => _selected = index),
            destinations: [
              for (final entry in widget.sections)
                NavigationRailDestination(
                  icon: const Icon(Icons.circle_outlined),
                  selectedIcon: const Icon(Icons.circle),
                  label: Text(_label(entry)),
                ),
            ],
          ),
          const VerticalDivider(width: 1),
          Expanded(
            child: _SectionPreview(
              section: section,
              state: widget.screensController.stateFor(section),
            ),
          ),
        ],
      ),
    );
  }
}

class _SplitViewWorkspace extends StatefulWidget {
  const _SplitViewWorkspace({
    required this.sections,
    required this.screensController,
  });

  final List<CoreScreenSection> sections;
  final CoreScreensController screensController;

  @override
  State<_SplitViewWorkspace> createState() => _SplitViewWorkspaceState();
}

class _SplitViewWorkspaceState extends State<_SplitViewWorkspace> {
  int _selected = 0;

  @override
  Widget build(BuildContext context) {
    final section = widget.sections[_selected];
    return Scaffold(
      appBar: AppBar(title: const Text('Dimension Web/Desktop Split')),
      body: Row(
        children: [
          SizedBox(
            width: 260,
            child: ListView(
              children: [
                for (var i = 0; i < widget.sections.length; i++)
                  ListTile(
                    title: Text(_label(widget.sections[i])),
                    selected: i == _selected,
                    onTap: () => setState(() => _selected = i),
                  ),
              ],
            ),
          ),
          const VerticalDivider(width: 1),
          Expanded(
            child: _SectionPreview(
              section: section,
              state: widget.screensController.stateFor(section),
            ),
          ),
        ],
      ),
    );
  }
}

class _SectionPreview extends StatelessWidget {
  const _SectionPreview({required this.section, required this.state});

  final CoreScreenSection section;
  final CoreScreensState state;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Container(
          padding: const EdgeInsets.all(12),
          color: Theme.of(context).colorScheme.surfaceContainerHighest,
          child: Text('Section: ${_label(section)}'),
        ),
        Expanded(
          child: switch (state.status) {
            CoreScreenStatus.loading => const Center(
              child: CircularProgressIndicator(),
            ),
            CoreScreenStatus.error => Center(
              child: Text(state.errorMessage ?? 'Unable to load section'),
            ),
            CoreScreenStatus.ready => state.items.isEmpty
                ? const Center(child: Text('No items'))
                : ListView.builder(
                    itemCount: state.items.length,
                    itemBuilder: (_, index) => ListTile(
                      title: Text(state.items[index]),
                    ),
                  ),
          },
        ),
      ],
    );
  }
}

String _label(CoreScreenSection section) {
  return switch (section) {
    CoreScreenSection.circles => 'Circles',
    CoreScreenSection.peers => 'Peers',
    CoreScreenSection.chat => 'Chat',
    CoreScreenSection.search => 'Search',
    CoreScreenSection.transfers => 'Transfers',
    CoreScreenSection.settings => 'Settings',
    CoreScreenSection.diagnostics => 'Diagnostics',
  };
}
