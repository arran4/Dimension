import 'dart:io' show Platform;

import 'package:flutter/material.dart';

import '../model/closable_tab.dart';
import '../model/commands/file_listing.dart';
import '../model/commands/fs_listing.dart';
import '../model/commands/private_chat_command.dart';
import 'date_formatter.dart';
import 'selectable_tab.dart';

class CirclePanelController extends ChangeNotifier
    implements ClosableTab, SelectableTab {
  CirclePanelController({required this.circleName, this.displayName = ''});

  final String circleName;
  final String displayName;

  bool connected = false;
  bool _selected = false;
  bool _closed = false;
  String currentPath = '/';

  final List<String> chatLines = <String>[];
  List<FSListing> folders = <FSListing>[];
  List<FSListing> files = <FSListing>[];

  bool get isSelected => _selected;
  bool get isClosed => _closed;

  void chatReceived(PrivateChatCommand command, {required String username}) {
    final now = DateTime.now();
    final hh = now.hour.toString().padLeft(2, '0');
    final mm = now.minute.toString().padLeft(2, '0');

    for (final line in command.content.split('\n')) {
      if (line.trim().isEmpty) {
        continue;
      }
      chatLines.add('$hh:$mm $username: $line');
    }
    notifyListeners();
  }

  void applyFileListing(FileListing listing) {
    connected = true;
    currentPath = listing.path;
    folders = List<FSListing>.from(listing.folders);
    files = List<FSListing>.from(listing.files);
    notifyListeners();
  }

  void navigateUp() {
    if (currentPath == '/') {
      return;
    }

    final parts = currentPath.split('/')..removeWhere((part) => part.isEmpty);
    if (parts.isNotEmpty) {
      parts.removeLast();
    }

    currentPath = parts.isEmpty ? '/' : '/${parts.join('/')}';
    notifyListeners();
  }

  @override
  void select() {
    _selected = true;
    notifyListeners();
  }

  @override
  void unselect() {
    _selected = false;
    notifyListeners();
  }

  @override
  void close() {
    _closed = true;
    notifyListeners();
  }
}

class CirclePanel extends StatelessWidget {
  const CirclePanel({
    super.key,
    required this.controller,
    this.onTapFolder,
    this.onTapFile,
    this.onClose,
    bool? isMono,
  }) : _isMonoOverride = isMono;

  final CirclePanelController controller;
  final Future<void> Function(String path)? onTapFolder;
  final Future<void> Function(FSListing file)? onTapFile;
  final VoidCallback? onClose;
  final bool? _isMonoOverride;

  bool get isMono => _isMonoOverride ?? _platformIsMono();

  static bool _platformIsMono() {
    return Platform.environment.containsKey('MONO_ENV_OPTIONS') ||
        Platform.environment.containsKey('MONO_PATH');
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: controller,
      builder: (context, _) {
        return Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            ListTile(
              dense: !isMono,
              title: Text(controller.circleName),
              subtitle: Text(controller.currentPath),
              trailing: IconButton(
                onPressed: () {
                  controller.close();
                  onClose?.call();
                },
                icon: const Icon(Icons.close),
              ),
            ),
            Wrap(
              spacing: 8,
              children: [
                FilterChip(
                  selected: controller.isSelected,
                  onSelected: (selected) {
                    if (selected) {
                      controller.select();
                    } else {
                      controller.unselect();
                    }
                  },
                  label: const Text('Selected'),
                ),
                ActionChip(
                  onPressed: controller.currentPath == '/'
                      ? null
                      : () {
                          controller.navigateUp();
                          onTapFolder?.call(controller.currentPath);
                        },
                  label: const Text('Up'),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Expanded(
              child: Row(
                children: [
                  Expanded(
                    child: _FilesPane(
                      folders: controller.folders,
                      files: controller.files,
                      onTapFolder: (folder) async {
                        final nextPath = controller.currentPath == '/'
                            ? '/${folder.name}'
                            : '${controller.currentPath}/${folder.name}';
                        await onTapFolder?.call(nextPath);
                      },
                      onTapFile: onTapFile,
                    ),
                  ),
                  const VerticalDivider(width: 1),
                  Expanded(
                    child: _ChatPane(lines: controller.chatLines),
                  ),
                ],
              ),
            ),
          ],
        );
      },
    );
  }
}

class _FilesPane extends StatelessWidget {
  const _FilesPane({
    required this.folders,
    required this.files,
    required this.onTapFolder,
    required this.onTapFile,
  });

  final List<FSListing> folders;
  final List<FSListing> files;
  final Future<void> Function(FSListing folder) onTapFolder;
  final Future<void> Function(FSListing file)? onTapFile;

  @override
  Widget build(BuildContext context) {
    final entries = <FSListing>[...folders, ...files];

    if (entries.isEmpty) {
      return const Center(child: Text('No files in this folder.'));
    }

    return ListView.separated(
      itemCount: entries.length,
      separatorBuilder: (_, _) => const Divider(height: 1),
      itemBuilder: (context, index) {
        final item = entries[index];
        return ListTile(
          leading: Icon(item.isFolder ? Icons.folder : Icons.description),
          title: Text(item.name),
          subtitle: Text(
            '${item.size} bytes • ${DateFormatter.formatDate(item.updated)}',
          ),
          onTap: () async {
            if (item.isFolder) {
              await onTapFolder(item);
            } else {
              await onTapFile?.call(item);
            }
          },
        );
      },
    );
  }
}

class _ChatPane extends StatelessWidget {
  const _ChatPane({required this.lines});

  final List<String> lines;

  @override
  Widget build(BuildContext context) {
    if (lines.isEmpty) {
      return const Center(child: Text('No messages yet.'));
    }

    return ListView.builder(
      itemCount: lines.length,
      itemBuilder: (context, index) => ListTile(
        dense: true,
        title: Text(lines[index]),
      ),
    );
  }
}
