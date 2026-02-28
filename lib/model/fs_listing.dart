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
}
