import 'package:flutter/material.dart';

import 'core_screens.dart';
import 'desktop_context_menu.dart';
import 'desktop_shell_infra.dart';
import 'desktop_window_state.dart';
import 'platform_plan_infra.dart';

class AdaptiveWorkspace extends StatelessWidget {
  AdaptiveWorkspace({
    super.key,
    required this.planController,
    required this.screensController,
    DesktopShellController? desktopShellController,
    DesktopContextMenuController? desktopContextMenuController,
    this.desktopWindowStateController,
  }) : desktopShellController = desktopShellController ?? DesktopShellController(),
       desktopContextMenuController =
           desktopContextMenuController ?? const DesktopContextMenuController();

  final PlatformPlanController planController;
  final CoreScreensController screensController;
  final DesktopShellController desktopShellController;
  final DesktopContextMenuController desktopContextMenuController;
  final DesktopWindowStateController? desktopWindowStateController;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        planController.recompute(
          width: constraints.maxWidth,
          height: constraints.maxHeight,
        );
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
                desktopShellController: desktopShellController,
                desktopContextMenuController: desktopContextMenuController,
                desktopWindowStateController: desktopWindowStateController,
              ),
              NavigationPattern.splitView => _SplitViewWorkspace(
                sections: tabs,
                screensController: screensController,
                desktopShellController: desktopShellController,
                desktopContextMenuController: desktopContextMenuController,
                desktopWindowStateController: desktopWindowStateController,
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
      child: Builder(
        builder: (context) {
          final tabController = DefaultTabController.of(context);
          return Scaffold(
            appBar: AppBar(
              title: Text(
                MediaQuery.of(context).orientation == Orientation.landscape
                    ? 'Dimension Mobile Landscape'
                    : 'Dimension Mobile',
              ),
            ),
            body: SafeArea(
              child: AnimatedPadding(
                duration: const Duration(milliseconds: 120),
                padding: EdgeInsets.only(
                  bottom: MediaQuery.of(context).viewInsets.bottom,
                ),
                child: TabBarView(
                  children: [
                    for (final section in sections)
                      _SectionPreview(
                        section: section,
                        state: screensController.stateFor(section),
                        refreshable: true,
                        onRefresh: () => _refreshSection(section, screensController),
                      ),
                  ],
                ),
              ),
            ),
            bottomNavigationBar: Material(
              color: Theme.of(context).colorScheme.surface,
              child: TabBar(
                isScrollable: true,
                tabs: [
                  for (final section in sections)
                    Tab(height: 56, text: _label(section)),
                ],
              ),
            ),
            floatingActionButton: FloatingActionButton.extended(
              key: const Key('mobileRefreshFab'),
              onPressed: () {
                final index = tabController.index;
                final section = sections[index];
                _refreshSection(section, screensController);
              },
              label: const Text('Refresh'),
              icon: const Icon(Icons.refresh),
            ),
          );
        },
      ),
    );
  }

  Future<void> _refreshSection(
    CoreScreenSection section,
    CoreScreensController controller,
  ) {
    return switch (section) {
      CoreScreenSection.circles => controller.joinCircle('LAN'),
      CoreScreenSection.peers => controller.refreshPeers(),
      CoreScreenSection.search => controller.runSearch('mobile'),
      CoreScreenSection.transfers => controller.queueDownload('mobile.bin'),
      CoreScreenSection.chat ||
      CoreScreenSection.settings ||
      CoreScreenSection.diagnostics => Future<void>.value(),
    };
  }
}

class _RailWorkspace extends StatefulWidget {
  const _RailWorkspace({
    required this.sections,
    required this.screensController,
    required this.desktopShellController,
    required this.desktopContextMenuController,
    required this.desktopWindowStateController,
  });

  final List<CoreScreenSection> sections;
  final CoreScreensController screensController;
  final DesktopShellController desktopShellController;
  final DesktopContextMenuController desktopContextMenuController;
  final DesktopWindowStateController? desktopWindowStateController;

  @override
  State<_RailWorkspace> createState() => _RailWorkspaceState();
}

class _RailWorkspaceState extends State<_RailWorkspace> {
  int _selected = 0;

  @override
  Widget build(BuildContext context) {
    final section = widget.sections[_selected];
    return _DesktopWorkspaceScaffold(
      title: 'Dimension Desktop',
      section: section,
      shellController: widget.desktopShellController,
      contextMenuController: widget.desktopContextMenuController,
      windowStateController: widget.desktopWindowStateController,
      onRefreshSection: () {
        if (section == CoreScreenSection.peers) {
          widget.screensController.refreshPeers();
        }
      },
      navigation: NavigationRail(
        selectedIndex: _selected,
        onDestinationSelected: (index) {
          setState(() => _selected = index);
          widget.desktopShellController.activateSection(widget.sections[index]);
        },
        destinations: [
          for (final entry in widget.sections)
            NavigationRailDestination(
              icon: MouseRegion(
                onEnter: (_) => widget.desktopShellController.onSectionHover(entry),
                onExit: (_) => widget.desktopShellController.onSectionHover(null),
                child: const Icon(Icons.circle_outlined),
              ),
              selectedIcon: const Icon(Icons.circle),
              label: Text(_label(entry)),
            ),
        ],
      ),
      child: _SectionPreview(
        section: section,
        state: widget.screensController.stateFor(section),
        desktopDense: true,
      ),
    );
  }
}

class _SplitViewWorkspace extends StatefulWidget {
  const _SplitViewWorkspace({
    required this.sections,
    required this.screensController,
    required this.desktopShellController,
    required this.desktopContextMenuController,
    required this.desktopWindowStateController,
  });

  final List<CoreScreenSection> sections;
  final CoreScreensController screensController;
  final DesktopShellController desktopShellController;
  final DesktopContextMenuController desktopContextMenuController;
  final DesktopWindowStateController? desktopWindowStateController;

  @override
  State<_SplitViewWorkspace> createState() => _SplitViewWorkspaceState();
}

class _SplitViewWorkspaceState extends State<_SplitViewWorkspace> {
  int _selected = 0;

  @override
  Widget build(BuildContext context) {
    final section = widget.sections[_selected];
    return _DesktopWorkspaceScaffold(
      title: 'Dimension Web/Desktop Split',
      section: section,
      shellController: widget.desktopShellController,
      contextMenuController: widget.desktopContextMenuController,
      windowStateController: widget.desktopWindowStateController,
      onRefreshSection: () {
        if (section == CoreScreenSection.peers) {
          widget.screensController.refreshPeers();
        }
      },
      navigation: SizedBox(
        width: 260,
        child: ListView(
          children: [
            for (var i = 0; i < widget.sections.length; i++)
              MouseRegion(
                onEnter: (_) =>
                    widget.desktopShellController.onSectionHover(widget.sections[i]),
                onExit: (_) => widget.desktopShellController.onSectionHover(null),
                child: ListTile(
                  title: Text(_label(widget.sections[i])),
                  selected: i == _selected,
                  onTap: () {
                    setState(() => _selected = i);
                    widget.desktopShellController.activateSection(widget.sections[i]);
                  },
                ),
              ),
          ],
        ),
      ),
      child: _SectionPreview(
        section: section,
        state: widget.screensController.stateFor(section),
        desktopDense: true,
      ),
    );
  }
}

class _DesktopWorkspaceScaffold extends StatelessWidget {
  const _DesktopWorkspaceScaffold({
    required this.title,
    required this.section,
    required this.navigation,
    required this.child,
    required this.shellController,
    required this.contextMenuController,
    required this.windowStateController,
    required this.onRefreshSection,
  });

  final String title;
  final CoreScreenSection section;
  final Widget navigation;
  final Widget child;
  final DesktopShellController shellController;
  final DesktopContextMenuController contextMenuController;
  final DesktopWindowStateController? windowStateController;
  final VoidCallback onRefreshSection;

  @override
  Widget build(BuildContext context) {
    final shortcuts = shellController.buildShortcutMap();
    return AnimatedBuilder(
      animation: shellController,
      builder: (context, _) {
        final actions = contextMenuController.buildActions(
          section: _label(section),
          onRefresh: onRefreshSection,
          onOpenDetails: () {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text('Open ${_label(section)} details')),
            );
          },
        );

        return CallbackShortcuts(
          bindings: shortcuts,
          child: Focus(
            autofocus: true,
            child: Scaffold(
                appBar: AppBar(
                  title: Text(title),
                  actions: [
                    FutureBuilder<DesktopWindowGeometry>(
                      future: windowStateController?.restoreOrDefault(),
                      builder: (context, snapshot) {
                        final geometry = snapshot.data;
                        return Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 12),
                          child: Center(
                            child: Text(
                              geometry == null
                                  ? 'Window: default'
                                  : 'Window: ${geometry.width.toInt()}×${geometry.height.toInt()}',
                              key: const Key('desktopWindowGeometryLabel'),
                              style: Theme.of(context).textTheme.bodySmall,
                            ),
                          ),
                        );
                      },
                    ),
                    PopupMenuButton<DesktopMenuAction>(
                      key: const Key('desktopContextMenuButton'),
                      icon: const Icon(Icons.more_vert),
                      onSelected: (action) => action.onSelected(),
                      itemBuilder: (context) {
                        return [
                          for (final action in actions)
                            PopupMenuItem<DesktopMenuAction>(
                              value: action,
                              child: Text(action.label),
                            ),
                        ];
                      },
                    ),
                  ],
                ),
                body: Row(
                  children: [
                    navigation,
                    const VerticalDivider(width: 1),
                    Expanded(child: child),
                  ],
                ),
                bottomNavigationBar: _DesktopStatusBar(
                  message: shellController.hoverState.statusMessage,
                ),
              ),
            ),
          ),
        );
      },
    );
  }
}

class _DesktopStatusBar extends StatelessWidget {
  const _DesktopStatusBar({required this.message});

  final String? message;

  @override
  Widget build(BuildContext context) {
    return Container(
      key: const Key('desktopStatusBar'),
      height: 28,
      alignment: Alignment.centerLeft,
      color: Theme.of(context).colorScheme.surfaceContainerHighest,
      padding: const EdgeInsets.symmetric(horizontal: 12),
      child: Text(message ?? 'Ready', style: Theme.of(context).textTheme.bodySmall),
    );
  }
}

class _SectionPreview extends StatelessWidget {
  const _SectionPreview({
    required this.section,
    required this.state,
    this.desktopDense = false,
    this.refreshable = false,
    this.onRefresh,
  });

  final CoreScreenSection section;
  final CoreScreensState state;
  final bool desktopDense;
  final bool refreshable;
  final Future<void> Function()? onRefresh;

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
        Expanded(child: _buildContent()),
      ],
    );
  }

  Widget _buildContent() {
    final content = switch (state.status) {
      CoreScreenStatus.loading => const Center(child: CircularProgressIndicator()),
      CoreScreenStatus.error => Center(
        child: Text(state.errorMessage ?? 'Unable to load section'),
      ),
      CoreScreenStatus.ready => state.items.isEmpty
          ? const Center(child: Text('No items'))
          : desktopDense && _supportsDesktopTable(section)
          ? _ResizableDesktopTable(section: section, rows: state.items)
          : ListView.builder(
              itemCount: state.items.length,
              itemBuilder: (_, index) => ListTile(
                title: Text(state.items[index]),
              ),
            ),
    };

    if (!refreshable || onRefresh == null) {
      return content;
    }

    final scrollable = content is ListView
        ? content
        : ListView(
            physics: const AlwaysScrollableScrollPhysics(),
            children: [
              SizedBox(
                height: 280,
                child: Center(child: content),
              ),
            ],
          );

    return RefreshIndicator(onRefresh: onRefresh!, child: scrollable);
  }

  static bool _supportsDesktopTable(CoreScreenSection section) {
    return section == CoreScreenSection.peers ||
        section == CoreScreenSection.search ||
        section == CoreScreenSection.transfers;
  }
}

class _ResizableDesktopTable extends StatefulWidget {
  const _ResizableDesktopTable({required this.section, required this.rows});

  final CoreScreenSection section;
  final List<String> rows;

  @override
  State<_ResizableDesktopTable> createState() => _ResizableDesktopTableState();
}

class _ResizableDesktopTableState extends State<_ResizableDesktopTable> {
  double _leftPaneFraction = 0.55;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final leftWidth = constraints.maxWidth * _leftPaneFraction;
        return Row(
          key: Key('desktopResizableTable-${widget.section.name}'),
          children: [
            SizedBox(
              width: leftWidth,
              child: _DesktopColumnList(
                title: _label(widget.section),
                rows: widget.rows,
              ),
            ),
            GestureDetector(
              key: const Key('desktopColumnResizer'),
              behavior: HitTestBehavior.translucent,
              onHorizontalDragUpdate: (details) {
                final next =
                    _leftPaneFraction + (details.delta.dx / constraints.maxWidth);
                setState(() {
                  _leftPaneFraction = next.clamp(0.3, 0.8);
                });
              },
              child: const MouseRegion(
                cursor: SystemMouseCursors.resizeColumn,
                child: VerticalDivider(width: 10, thickness: 2),
              ),
            ),
            Expanded(
              child: _DesktopColumnList(
                title: 'Details',
                rows: [
                  for (final row in widget.rows)
                    '${widget.section.name.toUpperCase()}: $row',
                ],
              ),
            ),
          ],
        );
      },
    );
  }
}

class _DesktopColumnList extends StatelessWidget {
  const _DesktopColumnList({required this.title, required this.rows});

  final String title;
  final List<String> rows;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Container(
          color: Theme.of(context).colorScheme.surfaceContainerLow,
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
          child: Text(title),
        ),
        Expanded(
          child: ListView.builder(
            itemCount: rows.length,
            itemBuilder: (context, index) {
              return ListTile(
                dense: true,
                visualDensity: VisualDensity.compact,
                title: Text(rows[index]),
              );
            },
          ),
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
