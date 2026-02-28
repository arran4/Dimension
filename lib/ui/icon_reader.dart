import 'package:flutter/widgets.dart';

enum IconSize { small, large }

enum FolderType { open, closed }

class IconDescriptor {
  const IconDescriptor({
    required this.iconData,
    required this.size,
  });

  final IconData iconData;
  final double size;
}

abstract class FileIconResolver {
  IconData iconForFilePath(String path);
}

class ExtensionFileIconResolver implements FileIconResolver {
  static const _archiveExtensions = {'zip', 'rar', '7z', 'tar', 'gz'};
  static const _audioExtensions = {'mp3', 'wav', 'aac', 'flac'};
  static const _videoExtensions = {'mp4', 'mkv', 'avi', 'mov'};
  static const _imageExtensions = {'png', 'jpg', 'jpeg', 'gif', 'webp'};

  @override
  IconData iconForFilePath(String path) {
    final extension = _extractExtension(path);
    if (_archiveExtensions.contains(extension)) {
      return const IconData(0xe149, fontFamily: 'MaterialIcons'); // folder_zip
    }
    if (_audioExtensions.contains(extension)) {
      return const IconData(0xe405, fontFamily: 'MaterialIcons'); // audiotrack
    }
    if (_videoExtensions.contains(extension)) {
      return const IconData(0xe04b, fontFamily: 'MaterialIcons'); // movie
    }
    if (_imageExtensions.contains(extension)) {
      return const IconData(0xe3f4, fontFamily: 'MaterialIcons'); // image
    }
    return const IconData(0xe24d, fontFamily: 'MaterialIcons'); // insert_drive_file
  }

  String _extractExtension(String path) {
    final lastDot = path.lastIndexOf('.');
    if (lastDot < 0 || lastDot >= path.length - 1) {
      return '';
    }
    return path.substring(lastDot + 1).toLowerCase();
  }
}

class IconReader {
  IconReader({FileIconResolver? fileIconResolver})
    : _fileIconResolver = fileIconResolver ?? ExtensionFileIconResolver();

  final FileIconResolver _fileIconResolver;

  IconDescriptor getFileIcon(String path, IconSize size) {
    return IconDescriptor(
      iconData: _fileIconResolver.iconForFilePath(path),
      size: _sizeFor(size),
    );
  }

  IconDescriptor getFolderIcon(IconSize size, FolderType folderType) {
    final iconData = folderType == FolderType.open
        ? const IconData(0xe2c8, fontFamily: 'MaterialIcons') // folder_open
        : const IconData(0xe2c7, fontFamily: 'MaterialIcons'); // folder
    return IconDescriptor(iconData: iconData, size: _sizeFor(size));
  }

  double _sizeFor(IconSize size) => size == IconSize.small ? 16 : 32;
}

/// Compatibility placeholders retained so references from the original porting checklist remain valid.
class Shell32 {}

class User32 {}
