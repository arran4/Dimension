import 'package:flutter/material.dart';

import 'component_primitives.dart';

enum CoreScreenSection {
  circles,
  peers,
  chat,
  search,
  transfers,
  settings,
  diagnostics,
}

enum CoreScreenStatus { loading, ready, error }

class CoreScreensState {
  const CoreScreensState({
    required this.status,
    this.errorMessage,
    this.items = const <String>[],
  });

  final CoreScreenStatus status;
  final String? errorMessage;
  final List<String> items;
}

abstract class CoreScreensBackend {
  Future<void> joinCircle(String name);

  Future<List<String>> refreshPeers();

  Future<List<String>> runSearch(String query);

  Future<void> queueDownload(String itemName);
}

class CoreScreensController extends ChangeNotifier {
  CoreScreensController({this.backend});

  final CoreScreensBackend? backend;

  final Map<CoreScreenSection, CoreScreensState> _sections =
      <CoreScreenSection, CoreScreensState>{
        for (final section in CoreScreenSection.values)
          section: const CoreScreensState(status: CoreScreenStatus.loading),
      };

  final Set<CoreScreenSection> _busySections = <CoreScreenSection>{};
  final Map<CoreScreenSection, String> _statusMessages =
      <CoreScreenSection, String>{};

  String joinCircleTarget = 'LAN';
  String searchQuery = '';
  String downloadTarget = '';

  CoreScreensState stateFor(CoreScreenSection section) {
    return _sections[section]!;
  }

  bool sectionBusy(CoreScreenSection section) => _busySections.contains(section);

  String? sectionMessage(CoreScreenSection section) => _statusMessages[section];

  void setJoinCircleTarget(String value) {
    joinCircleTarget = value;
    notifyListeners();
  }

  void setSearchQuery(String value) {
    searchQuery = value;
    notifyListeners();
  }

  void setDownloadTarget(String value) {
    downloadTarget = value;
    notifyListeners();
  }

  void setLoading(CoreScreenSection section) {
    _sections[section] = const CoreScreensState(status: CoreScreenStatus.loading);
    notifyListeners();
  }

  void setError(CoreScreenSection section, String message) {
    _sections[section] =
        CoreScreensState(status: CoreScreenStatus.error, errorMessage: message);
    _statusMessages[section] = message;
    notifyListeners();
  }

  void setItems(CoreScreenSection section, List<String> items) {
    _sections[section] = CoreScreensState(
      status: CoreScreenStatus.ready,
      items: List<String>.unmodifiable(items),
    );
    notifyListeners();
  }

  Future<void> joinCircle(String name) async {
    final trimmed = name.trim();
    if (trimmed.isEmpty) {
      return;
    }

    final section = CoreScreenSection.circles;
    final previous = stateFor(section);
    final optimisticItems = <String>{...previous.items, trimmed}.toList();

    setItems(section, optimisticItems);
    _statusMessages[section] = 'Joining $trimmed...';
    _busySections.add(section);
    notifyListeners();

    try {
      await backend?.joinCircle(trimmed);
      _statusMessages[section] = 'Joined $trimmed';
    } catch (_) {
      _sections[section] = previous;
      _statusMessages[section] = 'Failed to join $trimmed';
    } finally {
      _busySections.remove(section);
      notifyListeners();
    }
  }

  Future<void> refreshPeers() async {
    final section = CoreScreenSection.peers;
    _busySections.add(section);
    _statusMessages[section] = 'Refreshing peers...';
    notifyListeners();

    try {
      final peers = await backend?.refreshPeers() ?? const <String>[];
      setItems(section, peers);
      _statusMessages[section] = 'Peer list updated';
    } catch (_) {
      setError(section, 'Failed to refresh peers');
    } finally {
      _busySections.remove(section);
      notifyListeners();
    }
  }

  Future<void> runSearch(String query) async {
    final trimmed = query.trim();
    if (trimmed.isEmpty) {
      return;
    }

    final section = CoreScreenSection.search;
    _busySections.add(section);
    _statusMessages[section] = 'Searching for "$trimmed"...';
    notifyListeners();

    try {
      final results = await backend?.runSearch(trimmed) ?? const <String>[];
      setItems(section, results);
      _statusMessages[section] = 'Search complete';
    } catch (_) {
      setError(section, 'Search failed for "$trimmed"');
    } finally {
      _busySections.remove(section);
      notifyListeners();
    }
  }

  Future<void> queueDownload(String itemName) async {
    final trimmed = itemName.trim();
    if (trimmed.isEmpty) {
      return;
    }

    final section = CoreScreenSection.transfers;
    final previous = stateFor(section);
    final optimisticItems = <String>[...previous.items, 'Queued: $trimmed'];

    setItems(section, optimisticItems);
    _statusMessages[section] = 'Queueing $trimmed...';
    _busySections.add(section);
    notifyListeners();

    try {
      await backend?.queueDownload(trimmed);
      _statusMessages[section] = 'Queued $trimmed';
    } catch (_) {
      _sections[section] = previous;
      _statusMessages[section] = 'Failed to queue $trimmed';
    } finally {
      _busySections.remove(section);
      notifyListeners();
    }
  }
}

Map<ShortcutActivator, VoidCallback> _buildSectionShortcutBindings(
  TabController tabController,
) {
  return <ShortcutActivator, VoidCallback>{
    const SingleActivator(LogicalKeyboardKey.digit1, control: true): () {
      tabController.animateTo(0);
    },
    const SingleActivator(LogicalKeyboardKey.digit2, control: true): () {
      tabController.animateTo(1);
    },
    const SingleActivator(LogicalKeyboardKey.digit3, control: true): () {
      tabController.animateTo(2);
    },
    const SingleActivator(LogicalKeyboardKey.digit4, control: true): () {
      tabController.animateTo(3);
    },
    const SingleActivator(LogicalKeyboardKey.digit5, control: true): () {
      tabController.animateTo(4);
    },
    const SingleActivator(LogicalKeyboardKey.digit6, control: true): () {
      tabController.animateTo(5);
    },
    const SingleActivator(LogicalKeyboardKey.digit7, control: true): () {
      tabController.animateTo(6);
    },
  };
}

class CoreScreensView extends StatelessWidget {
  const CoreScreensView({super.key, required this.controller});

  final CoreScreensController controller;

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: controller,
      builder: (context, _) {
        return DefaultTabController(
          length: CoreScreenSection.values.length,
          child: Builder(
            builder: (context) {
              final tabController = DefaultTabController.of(context);
              final shortcuts = _buildSectionShortcutBindings(tabController);
              return CallbackShortcuts(
                bindings: shortcuts,
                child: FocusTraversalGroup(
                  child: Focus(
                    autofocus: true,
                    child: Scaffold(
                      appBar: AppBar(
                        title: const Text('Dimension'),
                        bottom: TabBar(
                          isScrollable: true,
                          tabs: const [
                            Tab(text: 'Circles'),
                            Tab(text: 'Peers'),
                            Tab(text: 'Chat'),
                            Tab(text: 'Search'),
                            Tab(text: 'Transfers'),
                            Tab(text: 'Settings'),
                            Tab(text: 'Diagnostics'),
                          ],
                        ),
                      ),
                      body: TabBarView(
                        children: [
                _SectionPane(
                  title: 'Circles',
                  state: controller.stateFor(CoreScreenSection.circles),
                  emptyMessage: 'No circles joined.',
                  statusMessage: controller.sectionMessage(
                    CoreScreenSection.circles,
                  ),
                  busy: controller.sectionBusy(CoreScreenSection.circles),
                  actions: [
                    SizedBox(
                      width: 220,
                      child: TextField(
                        key: const Key('joinCircleInput'),
                        decoration: const InputDecoration(labelText: 'Circle'),
                        onChanged: controller.setJoinCircleTarget,
                      ),
                    ),
                    _sectionActionButton(
                      label: 'Join',
                      semanticsLabel: 'Join circle',
                      onPressed: () => controller.joinCircle(controller.joinCircleTarget),
                    ),
                  ],
                ),
                _SectionPane(
                  title: 'Peers',
                  state: controller.stateFor(CoreScreenSection.peers),
                  emptyMessage: 'No peers connected.',
                  statusMessage: controller.sectionMessage(CoreScreenSection.peers),
                  busy: controller.sectionBusy(CoreScreenSection.peers),
                  actions: [
                    _sectionActionButton(
                      label: 'Refresh',
                      semanticsLabel: 'Refresh peers',
                      onPressed: controller.refreshPeers,
                    ),
                  ],
                ),
                _SectionPane(
                  title: 'Chat',
                  state: controller.stateFor(CoreScreenSection.chat),
                  emptyMessage: 'No chat messages yet.',
                  statusMessage: controller.sectionMessage(CoreScreenSection.chat),
                  busy: controller.sectionBusy(CoreScreenSection.chat),
                ),
                _SectionPane(
                  title: 'Search',
                  state: controller.stateFor(CoreScreenSection.search),
                  emptyMessage: 'No search results.',
                  statusMessage: controller.sectionMessage(CoreScreenSection.search),
                  busy: controller.sectionBusy(CoreScreenSection.search),
                  actions: [
                    SizedBox(
                      width: 260,
                      child: TextField(
                        key: const Key('searchQueryInput'),
                        decoration: const InputDecoration(labelText: 'Search'),
                        onChanged: controller.setSearchQuery,
                      ),
                    ),
                    _sectionActionButton(
                      label: 'Search',
                      semanticsLabel: 'Run search',
                      onPressed: () => controller.runSearch(controller.searchQuery),
                    ),
                  ],
                ),
                _SectionPane(
                  title: 'Transfers',
                  state: controller.stateFor(CoreScreenSection.transfers),
                  emptyMessage: 'No active transfers.',
                  statusMessage: controller.sectionMessage(
                    CoreScreenSection.transfers,
                  ),
                  busy: controller.sectionBusy(CoreScreenSection.transfers),
                  actions: [
                    SizedBox(
                      width: 260,
                      child: TextField(
                        key: const Key('downloadTargetInput'),
                        decoration: const InputDecoration(labelText: 'Transfer'),
                        onChanged: controller.setDownloadTarget,
                      ),
                    ),
                    _sectionActionButton(
                      label: 'Queue Download',
                      semanticsLabel: 'Queue download for transfer target',
                      onPressed: () => controller.queueDownload(controller.downloadTarget),
                    ),
                  ],
                ),
                _SectionPane(
                  title: 'Settings',
                  state: controller.stateFor(CoreScreenSection.settings),
                  emptyMessage: 'No settings options available.',
                  statusMessage: controller.sectionMessage(
                    CoreScreenSection.settings,
                  ),
                  busy: controller.sectionBusy(CoreScreenSection.settings),
                ),
                _SectionPane(
                  title: 'Diagnostics',
                  state: controller.stateFor(CoreScreenSection.diagnostics),
                  emptyMessage: 'No diagnostics entries.',
                  statusMessage: controller.sectionMessage(
                    CoreScreenSection.diagnostics,
                  ),
                  busy: controller.sectionBusy(CoreScreenSection.diagnostics),
                ),
                        ],
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
        );
      },
    );
  }
}

Widget _sectionActionButton({
  required String label,
  required String semanticsLabel,
  required VoidCallback onPressed,
}) {
  return DimensionSectionAction(
    label: label,
    semanticsLabel: semanticsLabel,
    onPressed: onPressed,
  );
}

class _SectionPane extends StatelessWidget {
  const _SectionPane({
    required this.title,
    required this.state,
    required this.emptyMessage,
    required this.statusMessage,
    required this.busy,
    this.actions = const <Widget>[],
  });

  final String title;
  final CoreScreensState state;
  final String emptyMessage;
  final String? statusMessage;
  final bool busy;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) {
    Widget content = switch (state.status) {
      CoreScreenStatus.loading => const Center(child: CircularProgressIndicator()),
      CoreScreenStatus.error => Center(
        child: Text(state.errorMessage ?? 'Failed to load $title.'),
      ),
      CoreScreenStatus.ready => state.items.isEmpty
          ? Center(child: Text(emptyMessage))
          : ListView.separated(
              itemCount: state.items.length,
              separatorBuilder: (_, _) => const Divider(height: 1),
              itemBuilder: (_, index) => ListTile(title: Text(state.items[index])),
            ),
    };

    return Column(
      children: [
        DimensionSectionHeader(
          title: title,
          statusMessage: statusMessage,
          actions: actions,
          busy: busy,
          containerKey: Key('core-screen-header.$title'),
        ),
        Expanded(child: content),
      ],
    );
  }
}
