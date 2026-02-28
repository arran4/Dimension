import 'command.dart';

class HelloCommand extends Command {
  bool? debugBuild;
  bool? afk;
  int id = 0;
  int myShare = 0;
  String username = '';
  String description = '';
  Map<int, int> peerCount = {};
  int externalControlPort = 0;
  int externalDataPort = 0;
  int internalControlPort = 0;
  int internalDataPort = 0;
  int internalUdtPort = 0;
  String externalIP = '';
  List<int> myCircles = [];
  bool useUDT = false;
  List<String> internalIPs = [];
  int buildNumber = 0;
  bool behindDoubleNAT = false;
  bool requestingHelloBack = false;
  List<String> extensions = [];
}
