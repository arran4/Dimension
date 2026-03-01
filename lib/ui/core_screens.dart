import 'package:flutter/material.dart';

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

  CoreScreensState stateFor(CoreScreenSection section) {
    return _sections[section]!;
  }

  bool sectionBusy(CoreScreenSection section) => _busySections.contains(section);

  String? sectionMessage(CoreScreenSection section) => _statusMessages[section];

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
                    TextButton(
                      onPressed: () => controller.joinCircle('LAN'),
                      child: const Text('Join LAN'),
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
                    TextButton(
                      onPressed: controller.refreshPeers,
                      child: const Text('Refresh'),
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
                    TextButton(
                      onPressed: () => controller.runSearch('example'),
                      child: const Text('Search'),
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
                    TextButton(
                      onPressed: () => controller.queueDownload('example.bin'),
                      child: const Text('Queue Download'),
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
        );
      },
    );
  }
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
        if (actions.isNotEmpty || statusMessage != null || busy)
          Padding(
            padding: const EdgeInsets.fromLTRB(12, 8, 12, 0),
            child: Row(
              children: [
                ...actions,
                if (statusMessage != null)
                  Expanded(
                    child: Text(
                      statusMessage!,
                      key: Key('core-screen-status.$title'),
                      textAlign: TextAlign.end,
                    ),
                  ),
              ],
            ),
          ),
        if (busy) const LinearProgressIndicator(),
        Expanded(child: content),
      ],
    );
  }
}
