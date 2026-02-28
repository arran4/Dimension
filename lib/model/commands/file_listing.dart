import 'fs_listing.dart';
import 'command.dart';

class FileListing extends Command {
  String path = '';
  List<FSListing> files = [];
  List<FSListing> folders = [];
}
