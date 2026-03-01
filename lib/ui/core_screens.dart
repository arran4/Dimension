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

class CoreScreensController extends ChangeNotifier {
  final Map<CoreScreenSection, CoreScreensState> _sections =
      <CoreScreenSection, CoreScreensState>{
        for (final section in CoreScreenSection.values)
          section: const CoreScreensState(status: CoreScreenStatus.loading),
      };

  CoreScreensState stateFor(CoreScreenSection section) {
    return _sections[section]!;
  }

  void setLoading(CoreScreenSection section) {
    _sections[section] = const CoreScreensState(status: CoreScreenStatus.loading);
    notifyListeners();
  }

  void setError(CoreScreenSection section, String message) {
    _sections[section] =
        CoreScreensState(status: CoreScreenStatus.error, errorMessage: message);
    notifyListeners();
  }

  void setItems(CoreScreenSection section, List<String> items) {
    _sections[section] = CoreScreensState(
      status: CoreScreenStatus.ready,
      items: List<String>.unmodifiable(items),
    );
    notifyListeners();
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
                ),
                _SectionPane(
                  title: 'Peers',
                  state: controller.stateFor(CoreScreenSection.peers),
                  emptyMessage: 'No peers connected.',
                ),
                _SectionPane(
                  title: 'Chat',
                  state: controller.stateFor(CoreScreenSection.chat),
                  emptyMessage: 'No chat messages yet.',
                ),
                _SectionPane(
                  title: 'Search',
                  state: controller.stateFor(CoreScreenSection.search),
                  emptyMessage: 'No search results.',
                ),
                _SectionPane(
                  title: 'Transfers',
                  state: controller.stateFor(CoreScreenSection.transfers),
                  emptyMessage: 'No active transfers.',
                ),
                _SectionPane(
                  title: 'Settings',
                  state: controller.stateFor(CoreScreenSection.settings),
                  emptyMessage: 'No settings options available.',
                ),
                _SectionPane(
                  title: 'Diagnostics',
                  state: controller.stateFor(CoreScreenSection.diagnostics),
                  emptyMessage: 'No diagnostics entries.',
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
  });

  final String title;
  final CoreScreensState state;
  final String emptyMessage;

  @override
  Widget build(BuildContext context) {
    return switch (state.status) {
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
  }
}
