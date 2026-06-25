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
    final output = RootShare();
    output.id = json['id'] as int? ?? 0;
    output.parentId = json['parentId'] as int? ?? 0;
    output.name = json['name'] as String? ?? '';
    output.size = json['size'] as int? ?? 0;
    output.lastModified = json['lastModified'] as int? ?? 0;
    output.index = json['index'] as int? ?? 0;
    output.fullPath = json['fullPath'] as String? ?? '';
    output.totalBytes = json['totalBytes'] as int? ?? 0;
    output.quickHashedBytes = json['quickHashedBytes'] as int? ?? 0;
    output.fullHashedBytes = json['fullHashedBytes'] as int? ?? 0;
    output.folderIds = (json['folderIds'] as List<dynamic>? ?? <dynamic>[])
        .map((value) => value as int)
        .toList();
    output.fileIds = (json['fileIds'] as List<dynamic>? ?? <dynamic>[])
        .map((value) => value as int)
        .toList();
    return output;
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
