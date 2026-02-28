import 'fs_listing.dart';
import 'command.dart';

class SearchResultCommand extends Command {
  int myId = 0;
  String keyword = '';
  List<FSListing> files = [];
  List<FSListing> folders = [];
}
