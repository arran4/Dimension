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
      ..index = json['index'] as int? ?? 0
      ..fullPath = json['fullPath'] as String? ?? ''
      ..totalBytes = json['totalBytes'] as int? ?? 0
      ..quickHashedBytes = json['quickHashedBytes'] as int? ?? 0
      ..fullHashedBytes = json['fullHashedBytes'] as int? ?? 0;
  }

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'index': index,
      'fullPath': fullPath,
      'totalBytes': totalBytes,
      'quickHashedBytes': quickHashedBytes,
      'fullHashedBytes': fullHashedBytes,
    };
  }
}
