import 'package:dimension/model/commands/fs_listing.dart';
import 'package:dimension/model/commands/search_result_command.dart';
import 'package:dimension/ui/byte_formatter.dart';
import 'package:flutter/material.dart';

class SearchThingy {
  const SearchThingy({required this.fsListing, required this.userId});

  final FSListing fsListing;
  final int userId;
}

class SearchRow {
  const SearchRow({required this.entry, required this.username});

  final SearchThingy entry;
  final String username;
}

abstract class SearchPanelBackend {
  Future<void> beginSearch(String keyword);

  String usernameForPeer(int peerId);

  Future<void> downloadElement({required int peerId, required FSListing listing});
}

class SearchPanelController {
  SearchPanelController({required SearchPanelBackend backend}) : _backend = backend;

  final SearchPanelBackend _backend;

  String? _keyword;
  final List<SearchRow> _rows = <SearchRow>[];

  List<SearchRow> get rows => List<SearchRow>.unmodifiable(_rows);

  Future<void> doSearch(String keyword) async {
    _rows.clear();
    _keyword = keyword;
    await _backend.beginSearch(keyword);
  }

  /// C# compatibility shim.
  Future<void> DoSearch(String keyword) => doSearch(keyword);

  void searchCallback(SearchResultCommand command) {
    if (command.keyword != _keyword) {
      return;
    }

    final username = _backend.usernameForPeer(command.myId);
    for (final folder in command.folders) {
      _rows.add(
        SearchRow(
          entry: SearchThingy(fsListing: folder, userId: command.myId),
          username: username,
        ),
      );
    }
    for (final file in command.files) {
      _rows.add(
        SearchRow(
          entry: SearchThingy(fsListing: file, userId: command.myId),
          username: username,
        ),
      );
    }
  }

  /// C# compatibility shim.
  void SearchCallback(SearchResultCommand command) => searchCallback(command);

  Future<void> downloadSelections(Iterable<SearchThingy> selections) async {
    for (final selection in selections) {
      await _backend.downloadElement(
        peerId: selection.userId,
        listing: selection.fsListing,
      );
    }
  }

  /// C# compatibility shim.
  Future<void> DownloadSelections(Iterable<SearchThingy> selections) =>
      downloadSelections(selections);
}

class SearchPanel extends StatefulWidget {
  const SearchPanel({super.key, required this.controller});

  final SearchPanelController controller;

  @override
  State<SearchPanel> createState() => _SearchPanelState();
}

class _SearchPanelState extends State<SearchPanel> {
  late final TextEditingController _searchController;
  final Set<SearchThingy> _selection = <SearchThingy>{};

  @override
  void initState() {
    super.initState();
    _searchController = TextEditingController();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _runSearch() async {
    await widget.controller.doSearch(_searchController.text);
    if (mounted) {
      setState(() {});
    }
  }

  Future<void> _downloadSelected() async {
    await widget.controller.downloadSelections(_selection);
    _selection.clear();
    if (mounted) {
      setState(() {});
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Row(
          children: [
            Expanded(
              child: TextField(
                key: const Key('search.input'),
                controller: _searchController,
                onSubmitted: (_) => _runSearch(),
                decoration: const InputDecoration(labelText: 'Keyword'),
              ),
            ),
            const SizedBox(width: 8),
            FilledButton(
              key: const Key('search.submit'),
              onPressed: _runSearch,
              child: const Text('Search'),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Expanded(
          child: ListView.builder(
            itemCount: widget.controller.rows.length,
            itemBuilder: (context, index) {
              final row = widget.controller.rows[index];
              final selected = _selection.contains(row.entry);
              return ListTile(
                key: Key('search.row.$index'),
                selected: selected,
                onTap: () {
                  setState(() {
                    if (!selected) {
                      _selection.add(row.entry);
                    } else {
                      _selection.remove(row.entry);
                    }
                  });
                },
                title: Text(row.entry.fsListing.name),
                subtitle: Text(
                  '${ByteFormatter.formatBytes(row.entry.fsListing.size)} • ${row.username}',
                ),
                trailing: row.entry.fsListing.isFolder
                    ? const Icon(Icons.folder)
                    : const Icon(Icons.description),
              );
            },
          ),
        ),
        Align(
          alignment: Alignment.centerRight,
          child: FilledButton.tonal(
            key: const Key('search.downloadSelected'),
            onPressed: _selection.isEmpty ? null : _downloadSelected,
            child: const Text('Download selected'),
          ),
        ),
      ],
    );
  }
}
