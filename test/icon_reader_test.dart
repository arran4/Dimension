import 'package:dimension/ui/icon_reader.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('returns folder icon with expected size mapping', () {
    final reader = IconReader();

    final smallOpen = reader.getFolderIcon(IconSize.small, FolderType.open);
    final largeClosed = reader.getFolderIcon(IconSize.large, FolderType.closed);

    expect(smallOpen.size, 16);
    expect(largeClosed.size, 32);
    expect(smallOpen.iconData.codePoint, isNot(largeClosed.iconData.codePoint));
  });

  test('maps file extensions to deterministic icon groups', () {
    final reader = IconReader();

    final audio = reader.getFileIcon('song.mp3', IconSize.small);
    final archive = reader.getFileIcon('archive.zip', IconSize.small);
    final unknown = reader.getFileIcon('README', IconSize.small);

    expect(audio.iconData.codePoint, isNot(unknown.iconData.codePoint));
    expect(archive.iconData.codePoint, isNot(unknown.iconData.codePoint));
  });
}
