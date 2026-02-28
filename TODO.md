# Dimension Flutter Port TODO List

This document outlines the tasks required to port the C# application to a Dart/Flutter application.

## File: `./DimensionLib/App.cs`

- [ ] Port `App.cs` to Dart
  - **Classes**:
    - [ ] `class App`
  - **Public Methods**:
    - [ ] `downloadUpdates()`
    - [ ] `checkForUpdates()`
    - [ ] `doCleanup()`
    - [ ] `udpSend()`
    - [ ] `udpSend()`
    - [ ] `doLoad()`
    - [ ] `doPrivateChatReceived()`
    - [ ] `doFlash()`
  - **Public Properties**:
    - [ ] `isMono`
    - [ ] `comicSansOnly`

## File: `./DimensionLib/Model/ReliableIncomingConnection.cs`

- [ ] Port `ReliableIncomingConnection.cs` to Dart
  - **Classes**:
    - [ ] `class ReliableIncomingConnection`
  - **Public Methods**:
    - [ ] `send()`

## File: `./DimensionLib/Model/UdtOutgoingConnection.cs`

- [ ] Port `UdtOutgoingConnection.cs` to Dart
  - **Classes**:
    - [ ] `class UdtOutgoingConnection`
  - **Public Methods**:
    - [ ] `send()`
  - **Public Properties**:
    - [ ] `connecting`

## File: `./DimensionLib/Model/OutgoingConnection.cs`

- [x] Port `OutgoingConnection.cs` to Dart
  - **Classes**:
    - [x] `class OutgoingConnection`

## File: `./DimensionLib/Model/FileListDatabase.cs`

- [ ] Port `FileListDatabase.cs` to Dart
  - **Classes**:
    - [ ] `class FileListDatabase`
  - **Public Methods**:
    - [ ] `close()`
    - [ ] `close()`
    - [ ] `setString()`
    - [ ] `getString()`
    - [ ] `allocateId()`
    - [ ] `getRootShares()`
    - [ ] `deleteObject()`
    - [ ] `setInt()`
    - [ ] `setULong()`
    - [ ] `getInt()`
    - [ ] `getULong()`

## File: `./DimensionLib/Model/NetConstants.cs`

- [ ] Port `NetConstants.cs` to Dart
  - **Classes**:
    - [ ] `class NetConstants`

## File: `./DimensionLib/Model/Core.cs`

- [ ] Port `Core.cs` to Dart
  - **Classes**:
    - [ ] `class Core`
  - **Public Methods**:
    - [ ] `Dispose()`
    - [ ] `beginSearch()`
    - [ ] `leaveCircle()`
    - [ ] `joinCircle()`
    - [ ] `addPeer()`
    - [ ] `sendChat()`
    - [ ] `addIncomingConnection()`
    - [ ] `removeIncomingConnection()`
    - [ ] `chatReceived()`
    - [ ] `getIdleTime()`
  - **Public Properties**:
    - [ ] `isMono`

## File: `./DimensionLib/Model/FileList.cs`

- [ ] Port `FileList.cs` to Dart
  - **Classes**:
    - [ ] `class FileList`
  - **Public Methods**:
    - [ ] `update()`
    - [ ] `clear()`
    - [ ] `Dispose()`
    - [ ] `getRootShare()`
    - [ ] `getFolder()`
    - [ ] `getFile()`
    - [ ] `getFullPath()`
    - [ ] `getFSListing()`
    - [ ] `getFSListing()`
    - [ ] `doSave()`
    - [ ] `startUpdate()`

## File: `./DimensionLib/Model/Bootstrap.cs`

- [ ] Port `Bootstrap.cs` to Dart
  - **Classes**:
    - [ ] `class Bootstrap`
  - **Public Methods**:
    - [ ] `Dispose()`
    - [ ] `WriteLine()`
    - [ ] `Write()`
    - [ ] `join()`

## File: `./DimensionLib/Model/IncomingConnection.cs`

- [x] Port `IncomingConnection.cs` to Dart
  - **Classes**:
    - [x] `class IncomingConnection`

## File: `./DimensionLib/Model/Serializer.cs`

- [ ] Port `Serializer.cs` to Dart
  - **Classes**:
    - [ ] `class Serializer`
  - **Public Methods**:
    - [ ] `serialize()`
    - [ ] `getType()`
    - [ ] `getText()`

## File: `./DimensionLib/Model/LoopbackIncomingConnection.cs`

- [ ] Port `LoopbackIncomingConnection.cs` to Dart
  - **Classes**:
    - [ ] `class LoopbackIncomingConnection`
  - **Public Methods**:
    - [ ] `send()`
    - [ ] `received()`

## File: `./DimensionLib/Model/Settings.cs`

- [ ] Port `Settings.cs` to Dart
  - **Classes**:
    - [ ] `class Settings`
  - **Public Methods**:
    - [ ] `save()`
    - [ ] `getStringArray()`
    - [ ] `addStringToArrayNoDup()`
    - [ ] `removeStringToArrayNoDup()`
    - [ ] `setStringArray()`
    - [ ] `getULong()`
    - [ ] `setBool()`
    - [ ] `getBool()`
    - [ ] `setString()`
    - [ ] `getString()`
    - [ ] `setULong()`
    - [ ] `setInt()`
    - [ ] `getInt()`

## File: `./DimensionLib/Model/UdtIncomingConnection.cs`

- [ ] Port `UdtIncomingConnection.cs` to Dart
  - **Classes**:
    - [ ] `class UdtIncomingConnection`
  - **Public Methods**:
    - [ ] `send()`
  - **Public Properties**:
    - [ ] `connecting`

## File: `./DimensionLib/Model/Peer.cs`

- [ ] Port `Peer.cs` to Dart
  - **Classes**:
    - [ ] `class Peer`
  - **Public Methods**:
    - [ ] `downloadElement()`
    - [ ] `endpointIsInHistory()`
    - [ ] `addEndpointToHistory()`
    - [ ] `downloadFilePath()`
    - [ ] `commandReceived()`
    - [ ] `updateTransfers()`
    - [ ] `sendCommand()`
    - [ ] `reverseConnect()`
    - [ ] `createConnection()`
    - [ ] `endPunch()`
    - [ ] `releasePunch()`
    - [ ] `chatReceived()`
  - **Public Properties**:
    - [ ] `maybeDead`
    - [ ] `probablyDead`
    - [ ] `assumingDead`
    - [ ] `isLocal`

## File: `./DimensionLib/Model/SystemLog.cs`

- [ ] Port `SystemLog.cs` to Dart
  - **Classes**:
    - [ ] `class SystemLog`
  - **Public Methods**:
    - [ ] `addEntry()`

## File: `./DimensionLib/Model/GlobalSpeedLimiter.cs`

- [ ] Port `GlobalSpeedLimiter.cs` to Dart
  - **Classes**:
    - [ ] `class GlobalSpeedLimiter`
  - **Public Methods**:
    - [ ] `Dispose()`
    - [ ] `limitUpload()`
    - [ ] `limitDownload()`

## File: `./DimensionLib/Model/Folder.cs`

- [ ] Port `Folder.cs` to Dart
  - **Classes**:
    - [ ] `class Folder`

## File: `./DimensionLib/Model/PeerManager.cs`

- [ ] Port `PeerManager.cs` to Dart
  - **Classes**:
    - [ ] `class PeerManager`
  - **Public Methods**:
    - [ ] `allPeersInCircle()`
    - [ ] `doPeerRemoved()`
    - [ ] `havePeerWithAddress()`
    - [ ] `parseMiniHello()`
    - [ ] `parseHello()`
  - **Public Properties**:
    - [ ] `allPeers`

## File: `./DimensionLib/Model/FSListing.cs`

- [ ] Port `FSListing.cs` to Dart
  - **Classes**:
    - [ ] `class FSListing`

## File: `./DimensionLib/Model/Transfer.cs`

- [ ] Port `Transfer.cs` to Dart
  - **Classes**:
    - [ ] `class Transfer`

## File: `./DimensionLib/Model/ReliableOutgoingConnection.cs`

- [ ] Port `ReliableOutgoingConnection.cs` to Dart
  - **Classes**:
    - [ ] `class ReliableOutgoingConnection`
  - **Public Methods**:
    - [ ] `send()`

## File: `./DimensionLib/Model/Kademlia.cs`

- [ ] Port `Kademlia.cs` to Dart
  - **Classes**:
    - [ ] `class Kademlia`
  - **Public Methods**:
    - [ ] `Dispose()`
    - [ ] `initialize()`
    - [ ] `announce()`
    - [ ] `doLookup()`

## File: `./DimensionLib/Model/LoopbackOutgoingConnection.cs`

- [ ] Port `LoopbackOutgoingConnection.cs` to Dart
  - **Classes**:
    - [ ] `class LoopbackOutgoingConnection`
  - **Public Methods**:
    - [ ] `send()`
    - [ ] `received()`

## File: `./DimensionLib/Model/RootShare.cs`

- [ ] Port `RootShare.cs` to Dart
  - **Classes**:
    - [ ] `class RootShare`

## File: `./DimensionLib/Model/ByteCounter.cs`

- [ ] Port `ByteCounter.cs` to Dart
  - **Classes**:
    - [ ] `class ByteCounter`
  - **Public Methods**:
    - [ ] `addBytes()`
    - [ ] `addBytes()`

## File: `./DimensionLib/Model/File.cs`

- [ ] Port `File.cs` to Dart
  - **Classes**:
    - [ ] `class File`

## File: `./DimensionLib/Model/Commands/BeginPunchCommand.cs`

- [ ] Port `BeginPunchCommand.cs` to Dart
  - **Classes**:
    - [ ] `class BeginPunchCommand`

## File: `./DimensionLib/Model/Commands/RoomChatCommand.cs`

- [ ] Port `RoomChatCommand.cs` to Dart
  - **Classes**:
    - [ ] `class RoomChatCommand`

## File: `./DimensionLib/Model/Commands/SearchResultCommand.cs`

- [ ] Port `SearchResultCommand.cs` to Dart
  - **Classes**:
    - [ ] `class SearchResultCommand`

## File: `./DimensionLib/Model/Commands/Quitting.cs`

- [ ] Port `Quitting.cs` to Dart
  - **Classes**:
    - [ ] `class Quitting`

## File: `./DimensionLib/Model/Commands/InitRendezvous.cs`

- [ ] Port `InitRendezvous.cs` to Dart
  - **Classes**:
    - [ ] `class InitRendezvous`

## File: `./DimensionLib/Model/Commands/FileListing.cs`

- [ ] Port `FileListing.cs` to Dart
  - **Classes**:
    - [ ] `class FileListing`

## File: `./DimensionLib/Model/Commands/CancelCommand.cs`

- [ ] Port `CancelCommand.cs` to Dart
  - **Classes**:
    - [ ] `class CancelCommand`

## File: `./DimensionLib/Model/Commands/RequestChunks.cs`

- [ ] Port `RequestChunks.cs` to Dart
  - **Classes**:
    - [ ] `class RequestChunks`

## File: `./DimensionLib/Model/Commands/PrivateChatCommand.cs`

- [ ] Port `PrivateChatCommand.cs` to Dart
  - **Classes**:
    - [ ] `class PrivateChatCommand`

## File: `./DimensionLib/Model/Commands/KeepAlive.cs`

- [ ] Port `KeepAlive.cs` to Dart
  - **Classes**:
    - [ ] `class KeepAlive`

## File: `./DimensionLib/Model/Commands/GetFileListing.cs`

- [ ] Port `GetFileListing.cs` to Dart
  - **Classes**:
    - [ ] `class GetFileListing`

## File: `./DimensionLib/Model/Commands/GossipCommand.cs`

- [ ] Port `GossipCommand.cs` to Dart
  - **Classes**:
    - [ ] `class GossipCommand`

## File: `./DimensionLib/Model/Commands/FileChunk.cs`

- [ ] Port `FileChunk.cs` to Dart
  - **Classes**:
    - [ ] `class FileChunk`

## File: `./DimensionLib/Model/Commands/PeerList.cs`

- [ ] Port `PeerList.cs` to Dart
  - **Classes**:
    - [ ] `class PeerList`

## File: `./DimensionLib/Model/Commands/KeywordSearchCommand.cs`

- [ ] Port `KeywordSearchCommand.cs` to Dart
  - **Classes**:
    - [ ] `class KeywordSearchCommand`

## File: `./DimensionLib/Model/Commands/ConnectToMe.cs`

- [ ] Port `ConnectToMe.cs` to Dart
  - **Classes**:
    - [ ] `class ConnectToMe`

## File: `./DimensionLib/Model/Commands/HelloCommand.cs`

- [ ] Port `HelloCommand.cs` to Dart
  - **Classes**:
    - [ ] `class HelloCommand`

## File: `./DimensionLib/Model/Commands/Command.cs`

- [ ] Port `Command.cs` to Dart
  - **Classes**:
    - [ ] `class Command`

## File: `./DimensionLib/Model/Commands/FSListing.cs`

- [ ] Port `FSListing.cs` to Dart
  - **Classes**:
    - [ ] `class FSListing`

## File: `./DimensionLib/Model/Commands/SearchCommand.cs`

- [ ] Port `SearchCommand.cs` to Dart
  - **Classes**:
    - [ ] `class SearchCommand`

## File: `./DimensionLib/Model/Commands/PunchCommand.cs`

- [ ] Port `PunchCommand.cs` to Dart
  - **Classes**:
    - [ ] `class PunchCommand`

## File: `./DimensionLib/Model/Commands/EndPunchCommand.cs`

- [ ] Port `EndPunchCommand.cs` to Dart
  - **Classes**:
    - [ ] `class EndPunchCommand`

## File: `./DimensionLib/Model/Commands/GossipPeer.cs`

- [ ] Port `GossipPeer.cs` to Dart
  - **Classes**:
    - [ ] `class GossipPeer`

## File: `./DimensionLib/Model/Commands/DataCommand.cs`

- [ ] Port `DataCommand.cs` to Dart
  - **Classes**:
    - [ ] `class DataCommand`

## File: `./DimensionLib/Model/Commands/RequestFolderContents.cs`

- [ ] Port `RequestFolderContents.cs` to Dart
  - **Classes**:
    - [ ] `class RequestFolderContents`

## File: `./DimensionLib/Model/Commands/MiniHello.cs`

- [ ] Port `MiniHello.cs` to Dart
  - **Classes**:
    - [ ] `class MiniHello`

## File: `./DimensionLib/Model/Commands/ReverseConnectionType.cs`

- [ ] Port `ReverseConnectionType.cs` to Dart
  - **Classes**:
    - [ ] `class ReverseConnectionType`

## File: `./Updater/UpdatingForm.cs`

- [ ] Port `UpdatingForm.cs` to Dart
  - **Classes**:
    - [ ] `class UpdatingForm`

## File: `./Updater/Program.cs`

- [ ] Port `Program.cs` to Dart
  - **Classes**:
    - [ ] `class Program`
  - **Public Methods**:
    - [ ] `needsUpdate()`
    - [ ] `downloadPath()`

## File: `./Dimension/Program.cs`

- [ ] Port `Program.cs` to Dart
  - **Classes**:
    - [ ] `class Program`

## File: `./Dimension/UI/AboutForm.cs`

- [ ] Port `AboutForm.cs` to Dart
  - **Classes**:
    - [ ] `class AboutForm`

## File: `./Dimension/UI/DoubleBufferedListView.cs`

- [ ] Port `DoubleBufferedListView.cs` to Dart
  - **Classes**:
    - [ ] `class DoubleBufferedListView`

## File: `./Dimension/UI/FlashWindow.cs`

- [ ] Port `FlashWindow.cs` to Dart
  - **Classes**:
    - [ ] `class FlashWindow`
  - **Public Methods**:
    - [ ] `Flash()`
    - [ ] `Flash()`
    - [ ] `Start()`
    - [ ] `Stop()`
    - [ ] `ApplicationIsActivated()`

## File: `./Dimension/UI/DownloadQueuePanel.cs`

- [ ] Port `DownloadQueuePanel.cs` to Dart
  - **Classes**:
    - [ ] `class DownloadQueuePanel`

## File: `./Dimension/UI/LoadingForm.cs`

- [ ] Port `LoadingForm.cs` to Dart
  - **Classes**:
    - [ ] `class LoadingForm`

## File: `./Dimension/UI/ByteFormatter.cs`

- [ ] Port `ByteFormatter.cs` to Dart
  - **Classes**:
    - [ ] `class ByteFormatter`
  - **Public Methods**:
    - [ ] `formatBytes()`

## File: `./Dimension/UI/LimitChangeDialog.cs`

- [ ] Port `LimitChangeDialog.cs` to Dart
  - **Classes**:
    - [ ] `class LimitChangeDialog`

## File: `./Dimension/UI/DateFormatter.cs`

- [ ] Port `DateFormatter.cs` to Dart
  - **Classes**:
    - [ ] `class DateFormatter`
  - **Public Methods**:
    - [ ] `formatDate()`

## File: `./Dimension/UI/HTMLPanel.cs`

- [ ] Port `HTMLPanel.cs` to Dart
  - **Classes**:
    - [ ] `class HTMLPanel`
  - **Public Properties**:
    - [ ] `isMono`

## File: `./Dimension/UI/SettingsForm.cs`

- [ ] Port `SettingsForm.cs` to Dart
  - **Classes**:
    - [ ] `class SettingsForm`

## File: `./Dimension/UI/HashProgressForm.cs`

- [ ] Port `HashProgressForm.cs` to Dart
  - **Classes**:
    - [ ] `class HashProgressForm`

## File: `./Dimension/UI/IconReader.cs`

- [ ] Port `IconReader.cs` to Dart
  - **Classes**:
    - [ ] `class IconReader`
    - [ ] `class Shell32`
    - [ ] `class User32`

## File: `./Dimension/UI/TransfersPanel.cs`

- [ ] Port `TransfersPanel.cs` to Dart
  - **Classes**:
    - [ ] `class TransfersPanel`
  - **Public Properties**:
    - [ ] `isMono`

## File: `./Dimension/UI/RenameShareForm.cs`

- [ ] Port `RenameShareForm.cs` to Dart
  - **Classes**:
    - [ ] `class RenameShareForm`

## File: `./Dimension/UI/SearchPanel.cs`

- [ ] Port `SearchPanel.cs` to Dart
  - **Classes**:
    - [ ] `class SearchPanel`
    - [ ] `class SearchThingy`

## File: `./Dimension/UI/NetworkStatusPanel.cs`

- [ ] Port `NetworkStatusPanel.cs` to Dart
  - **Classes**:
    - [ ] `class NetworkStatusPanel`

## File: `./Dimension/UI/CirclePanel.cs`

- [ ] Port `CirclePanel.cs` to Dart
  - **Classes**:
    - [ ] `class CirclePanel`
  - **Public Methods**:
    - [ ] `close()`
    - [ ] `chatReceived()`
    - [ ] `select()`
    - [ ] `unselect()`
  - **Public Properties**:
    - [ ] `isMono`

## File: `./Dimension/UI/FinishedTransfersPanel.cs`

- [ ] Port `FinishedTransfersPanel.cs` to Dart
  - **Classes**:
    - [ ] `class FinishedTransfersPanel`

## File: `./Dimension/UI/UserPanel.cs`

- [ ] Port `UserPanel.cs` to Dart
  - **Classes**:
    - [ ] `class UserPanel`
  - **Public Methods**:
    - [ ] `close()`
    - [ ] `selectChat()`
    - [ ] `displayMessage()`
    - [ ] `addLine()`

## File: `./Dimension/UI/JoinCircleForm.cs`

- [ ] Port `JoinCircleForm.cs` to Dart
  - **Classes**:
    - [ ] `class JoinCircleForm`
  - **Public Methods**:
    - [ ] `joinCircle()`

## File: `./Dimension/UI/MainForm.cs`

- [ ] Port `MainForm.cs` to Dart
  - **Classes**:
    - [ ] `class MainForm`
  - **Public Methods**:
    - [ ] `selectUser()`
    - [ ] `privateChatReceived()`
    - [ ] `flash()`
    - [ ] `addOrSelectPanel()`
    - [ ] `addInternetCircle()`
    - [ ] `setColors()`
  - **Public Properties**:
    - [ ] `isMono`
