import 'package:dartobjectutils/dartobjectutils.dart';

class FSListing {
  int id = 0;
  int parentId = 0;
  String name = '';
  int size = 0;
  int lastModified = 0;
  bool isFolder = false;
}

class Folder extends FSListing {
  List<int> folderIds = [];
  List<int> fileIds = [];

  Folder() {
    isFolder = true;
  }
}

class File extends FSListing {
  File() {
    isFolder = false;
  }
}

class RootShare extends Folder {
  int index = 0;
  String fullPath = '';
  int totalBytes = 0;
  int quickHashedBytes = 0;
  int fullHashedBytes = 0;

  static RootShare fromJson(Map<String, dynamic> json) {
    return RootShare()
      ..id = getNumberPropOrDefault(json, 'id', 0).toInt()
      ..parentId = getNumberPropOrDefault(json, 'parentId', 0).toInt()
      ..name = getStringPropOrDefault(json, 'name', '')
      ..size = getNumberPropOrDefault(json, 'size', 0).toInt()
      ..lastModified = getNumberPropOrDefault(json, 'lastModified', 0).toInt()
      ..index = getNumberPropOrDefault(json, 'index', 0).toInt()
      ..fullPath = getStringPropOrDefault(json, 'fullPath', '')
      ..totalBytes = getNumberPropOrDefault(json, 'totalBytes', 0).toInt()
      ..quickHashedBytes =
          getNumberPropOrDefault(json, 'quickHashedBytes', 0).toInt()
      ..fullHashedBytes =
          getNumberPropOrDefault(json, 'fullHashedBytes', 0).toInt()
      ..folderIds = getNumberArrayPropOrDefault(json, 'folderIds', <num>[])
          .map((e) => e.toInt())
          .toList()
      ..fileIds = getNumberArrayPropOrDefault(json, 'fileIds', <num>[])
          .map((e) => e.toInt())
          .toList();
  }

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'id': id,
      'parentId': parentId,
      'name': name,
      'size': size,
      'lastModified': lastModified,
      'index': index,
      'fullPath': fullPath,
      'totalBytes': totalBytes,
      'quickHashedBytes': quickHashedBytes,
      'fullHashedBytes': fullHashedBytes,
      'folderIds': folderIds,
      'fileIds': fileIds,
    };
  }
}
