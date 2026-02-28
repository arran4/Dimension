# Dimension Flutter Port TODO List

This document outlines the tasks required to port the C# application to a Dart/Flutter application.

## Environment / Tooling Follow-ups

- [ ] Install or provision Flutter/Dart SDK in CI/dev container so `flutter analyze` and widget tests can run during porting PRs.

## UI Port Plan (Flutter: Mobile, Desktop, Web)

- [ ] Build and track the Flutter UI port as a first-class stream alongside backend model parity.

### 1) UI Architecture and Design System
- [ ] Define a shared design system for all targets.
  - [ ] Create `ThemeData` for light/dark themes, typography scale, spacing tokens, and semantic color roles.
  - [ ] Standardize reusable components (buttons, inputs, dialogs, list rows, status indicators, transfer progress bars).
  - [ ] Add a responsive breakpoint strategy (`compact`, `medium`, `expanded`) for layout switching.
- [ ] Introduce app-level navigation that supports deep-linkable routes (especially required for web).
- [ ] Choose and document state management approach for UI (e.g., Provider/Riverpod/Bloc) and enforce it consistently.

### 2) Core Screens and Flows (Shared Across All Platforms)
- [ ] Port the primary screens from the C# client into Flutter widgets.
  - [ ] Circles / server list management
  - [ ] Peer list and peer details
  - [ ] Chat view (public + private conversations)
  - [ ] Search + result list + download actions
  - [ ] Transfers queue (uploads/downloads, status, speed, ETA)
  - [ ] Settings / preferences
  - [ ] System log / diagnostics panel
- [ ] Create loading, empty, and error states for every major screen.
- [ ] Add optimistic UI updates and status feedback for long-running network operations.

### 3) Mobile Plan (Android/iOS)
- [ ] Implement compact mobile layouts for portrait and landscape orientations.
  - [ ] Replace multi-pane desktop layouts with tabbed or stacked navigation flows.
  - [ ] Ensure touch targets meet accessibility guidance.
  - [ ] Add pull-to-refresh and platform-appropriate gestures where helpful.
- [ ] Validate safe-area handling, keyboard avoidance, and small-screen overflow behavior.
- [ ] Add mobile-specific QA checklist (low-memory behavior, background/foreground transitions, intermittent network).

### 4) Desktop Plan (Windows/macOS/Linux)
- [ ] Implement expanded desktop layouts with multi-pane information density.
  - [ ] Persistent side navigation / split-view interactions.
  - [ ] Resizable columns and tables for peers, searches, and transfers.
  - [ ] Keyboard shortcuts for power-user workflows.
- [ ] Add native-feeling desktop affordances:
  - [ ] Right-click context menus.
  - [ ] Hover states and tooltips.
  - [ ] Window size persistence and restoration.
- [ ] Validate behavior at common desktop resolutions and ultrawide layouts.

### 5) Web Plan (Flutter Web)
- [ ] Build a web-ready shell with responsive navigation and browser-friendly route handling.
- [ ] Ensure links, history navigation, and refresh behavior preserve application state where possible.
- [ ] Address web input patterns (focus traversal, scroll behavior, text selection, copy/paste).
- [ ] Validate compatibility and layout fidelity on Chromium, Firefox, and Safari-class browsers.
- [ ] Optimize initial load bundle and defer non-critical UI modules when possible.

### 6) Cross-Platform Accessibility, Performance, and Quality
- [ ] Accessibility
  - [ ] Semantic labels for interactive controls.
  - [ ] High-contrast compliance and scalable text support.
  - [ ] Keyboard-only navigation and focus indicators.
- [ ] Performance
  - [ ] Profile and optimize list-heavy screens (chat, search results, transfers) using lazy builders and pagination.
  - [ ] Reduce unnecessary widget rebuilds and monitor frame timing on low-end devices.
- [ ] Quality and testing
  - [ ] Add widget tests for core UI components and screen states.
  - [ ] Add golden tests for major breakpoints (mobile, desktop, web widths).
  - [ ] Add integration tests covering critical user journeys end-to-end.

### 7) Release Readiness by Platform
- [ ] Mobile release readiness
  - [ ] Final pass for Android + iOS permissions, icons, splash screens, and store metadata.
- [ ] Desktop release readiness
  - [ ] Platform packaging, signing/notarization strategy, and updater workflow alignment.
- [ ] Web release readiness
  - [ ] Production hosting config, cache strategy, and version rollout plan.
- [ ] Maintain a per-platform parity checklist to verify each major C# UI feature is represented in Flutter.

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

- [x] Port `ReliableIncomingConnection.cs` to Dart
  - **Classes**:
    - [x] `class ReliableIncomingConnection`
  - **Public Methods**:
    - [x] `send()`

## File: `./DimensionLib/Model/UdtOutgoingConnection.cs`

- [x] Port `UdtOutgoingConnection.cs` to Dart
  - **Classes**:
    - [x] `class UdtOutgoingConnection`
  - **Public Methods**:
    - [x] `send()`
  - **Public Properties**:
    - [x] `connecting`
  - **TODO**:
    - [ ] Implement UDT via Dart FFI (wrapping C/C++ library) or rewrite using `RawDatagramSocket`. Currently, Dart implementations are stubs.
    - [x] Added a temporary pure-Dart `UdtTransport` abstraction and serializer-driven command flow so behavior can be unit tested with mocks while FFI/native transport is pending.

## File: `./DimensionLib/Model/OutgoingConnection.cs`

- [x] Port `OutgoingConnection.cs` to Dart
  - **Classes**:
    - [x] `class OutgoingConnection`

## File: `./DimensionLib/Model/FileListDatabase.cs`

- [x] Port `FileListDatabase.cs` to Dart
  - **Classes**:
    - [x] `class FileListDatabase`
  - **Public Methods**:
    - [x] `close()`
    - [x] `close()`
    - [x] `setString()`
    - [x] `getString()`
    - [x] `allocateId()`
    - [x] `getRootShares()`
    - [x] `deleteObject()`
    - [x] `setInt()`
    - [x] `setULong()`
    - [x] `getInt()`
    - [x] `getULong()`

## File: `./DimensionLib/Model/NetConstants.cs`

- [x] Port `NetConstants.cs` to Dart
  - **Classes**:
    - [x] `class NetConstants`

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

- [x] Port `Serializer.cs` to Dart
  - **Classes**:
    - [x] `class Serializer`
  - **Public Methods**:
    - [x] `serialize()`
    - [x] `getType()`
    - [x] `getText()`

  - **TODO**:
    - [ ] Register all concrete command codecs in app startup so runtime networking paths can deserialize every command type.

## File: `./DimensionLib/Model/LoopbackIncomingConnection.cs`

- [x] Port `LoopbackIncomingConnection.cs` to Dart
  - **Classes**:
    - [x] `class LoopbackIncomingConnection`
  - **Public Methods**:
    - [x] `send()`
    - [x] `received()`

## File: `./DimensionLib/Model/Settings.cs`

- [x] Port `Settings.cs` to Dart
  - **TODO**:
    - [ ] Replace `FileListDatabase.settingsStore` temporary bridge with the concrete Dart `Settings` implementation once available.
    - [ ] Optional persistence hook currently relies on `SavableStringKeyValueStore`; wire a real disk-backed settings store during app bootstrap.
  - **Classes**:
    - [x] `class Settings`
  - **Public Methods**:
    - [x] `save()`
    - [x] `getStringArray()`
    - [x] `addStringToArrayNoDup()`
    - [x] `removeStringToArrayNoDup()`
    - [x] `setStringArray()`
    - [x] `getULong()`
    - [x] `setBool()`
    - [x] `getBool()`
    - [x] `setString()`
    - [x] `getString()`
    - [x] `setULong()`
    - [x] `setInt()`
    - [x] `getInt()`

## File: `./DimensionLib/Model/UdtIncomingConnection.cs`

- [x] Port `UdtIncomingConnection.cs` to Dart
  - **Classes**:
    - [x] `class UdtIncomingConnection`
  - **Public Methods**:
    - [x] `send()`
  - **Public Properties**:
    - [x] `connecting`
  - **TODO**:
    - [ ] Implement UDT via Dart FFI (wrapping C/C++ library) or rewrite using `RawDatagramSocket`. Currently, Dart implementations are stubs.
    - [ ] Currently porting `dart-udt`; in the meantime, use a mock transport or mark related behavior as not implemented until the port is ready. Additional details will be added later.
    - [ ] Remove temporary compatibility shim exports once all imports have been consolidated on `lib/model/udt_connection.dart`.

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

- [x] Port `GlobalSpeedLimiter.cs` to Dart
  - **Classes**:
    - [x] `class GlobalSpeedLimiter`
  - **Public Methods**:
    - [x] `Dispose()`
    - [x] `limitUpload()`
    - [x] `limitDownload()`

## File: `./DimensionLib/Model/Folder.cs`

- [x] Port `Folder.cs` to Dart
  - **Classes**:
    - [x] `class Folder`

## File: `./DimensionLib/Model/PeerManager.cs`

- [x] Port `PeerManager.cs` to Dart
  - **Classes**:
    - [x] `class PeerManager`
  - **Public Methods**:
    - [x] `allPeersInCircle()`
    - [x] `doPeerRemoved()`
    - [x] `havePeerWithAddress()`
    - [x] `parseMiniHello()`
    - [x] `parseHello()`
  - **Public Properties**:
    - [x] `allPeers`

## File: `./DimensionLib/Model/FSListing.cs`

- [x] Port `FSListing.cs` to Dart
  - **Classes**:
    - [x] `class FSListing`

## File: `./DimensionLib/Model/Transfer.cs`

- [x] Port `Transfer.cs` to Dart
  - **Classes**:
    - [x] `class Transfer`

## File: `./DimensionLib/Model/ReliableOutgoingConnection.cs`

- [x] Port `ReliableOutgoingConnection.cs` to Dart
  - **Classes**:
    - [x] `class ReliableOutgoingConnection`
  - **Public Methods**:
    - [x] `send()`

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

- [x] Port `LoopbackOutgoingConnection.cs` to Dart
  - **Classes**:
    - [x] `class LoopbackOutgoingConnection`
  - **Public Methods**:
    - [x] `send()`
    - [x] `received()`

## File: `./DimensionLib/Model/RootShare.cs`

- [x] Port `RootShare.cs` to Dart
  - **Classes**:
    - [x] `class RootShare`

## File: `./DimensionLib/Model/ByteCounter.cs`

- [x] Port `ByteCounter.cs` to Dart
  - **Classes**:
    - [x] `class ByteCounter`
  - **Public Methods**:
    - [x] `addBytes()`
    - [x] `addBytes()`

## File: `./DimensionLib/Model/File.cs`

- [x] Port `File.cs` to Dart
  - **Classes**:
    - [x] `class File`

## File: `./DimensionLib/Model/Commands/BeginPunchCommand.cs`

- [x] Port `BeginPunchCommand.cs` to Dart
  - **Classes**:
    - [x] `class BeginPunchCommand`

## File: `./DimensionLib/Model/Commands/RoomChatCommand.cs`

- [x] Port `RoomChatCommand.cs` to Dart
  - **Classes**:
    - [x] `class RoomChatCommand`

## File: `./DimensionLib/Model/Commands/SearchResultCommand.cs`

- [x] Port `SearchResultCommand.cs` to Dart
  - **Classes**:
    - [x] `class SearchResultCommand`

## File: `./DimensionLib/Model/Commands/Quitting.cs`

- [x] Port `Quitting.cs` to Dart
  - **Classes**:
    - [x] `class Quitting`

## File: `./DimensionLib/Model/Commands/InitRendezvous.cs`

- [x] Port `InitRendezvous.cs` to Dart
  - **Classes**:
    - [x] `class InitRendezvous`

## File: `./DimensionLib/Model/Commands/FileListing.cs`

- [x] Port `FileListing.cs` to Dart
  - **Classes**:
    - [x] `class FileListing`

## File: `./DimensionLib/Model/Commands/CancelCommand.cs`

- [x] Port `CancelCommand.cs` to Dart
  - **Classes**:
    - [x] `class CancelCommand`

## File: `./DimensionLib/Model/Commands/RequestChunks.cs`

- [x] Port `RequestChunks.cs` to Dart
  - **Classes**:
    - [x] `class RequestChunks`

## File: `./DimensionLib/Model/Commands/PrivateChatCommand.cs`

- [x] Port `PrivateChatCommand.cs` to Dart
  - **Classes**:
    - [x] `class PrivateChatCommand`

## File: `./DimensionLib/Model/Commands/KeepAlive.cs`

- [x] Port `KeepAlive.cs` to Dart
  - **Classes**:
    - [x] `class KeepAlive`

## File: `./DimensionLib/Model/Commands/GetFileListing.cs`

- [x] Port `GetFileListing.cs` to Dart
  - **Classes**:
    - [x] `class GetFileListing`

## File: `./DimensionLib/Model/Commands/GossipCommand.cs`

- [x] Port `GossipCommand.cs` to Dart
  - **Classes**:
    - [x] `class GossipCommand`

## File: `./DimensionLib/Model/Commands/FileChunk.cs`

- [x] Port `FileChunk.cs` to Dart
  - **Classes**:
    - [x] `class FileChunk`

## File: `./DimensionLib/Model/Commands/PeerList.cs`

- [x] Port `PeerList.cs` to Dart
  - **Classes**:
    - [x] `class PeerList`

## File: `./DimensionLib/Model/Commands/KeywordSearchCommand.cs`

- [x] Port `KeywordSearchCommand.cs` to Dart
  - **Classes**:
    - [x] `class KeywordSearchCommand`

## File: `./DimensionLib/Model/Commands/ConnectToMe.cs`

- [x] Port `ConnectToMe.cs` to Dart
  - **Classes**:
    - [x] `class ConnectToMe`

## File: `./DimensionLib/Model/Commands/HelloCommand.cs`

- [x] Port `HelloCommand.cs` to Dart
  - **Classes**:
    - [x] `class HelloCommand`

## File: `./DimensionLib/Model/Commands/Command.cs`

- [x] Port `Command.cs` to Dart
  - **Classes**:
    - [x] `class Command`

## File: `./DimensionLib/Model/Commands/FSListing.cs`

- [x] Port `FSListing.cs` to Dart
  - **Classes**:
    - [x] `class FSListing`

## File: `./DimensionLib/Model/Commands/SearchCommand.cs`

- [x] Port `SearchCommand.cs` to Dart
  - **Classes**:
    - [x] `class SearchCommand`

## File: `./DimensionLib/Model/Commands/PunchCommand.cs`

- [x] Port `PunchCommand.cs` to Dart
  - **Classes**:
    - [x] `class PunchCommand`

## File: `./DimensionLib/Model/Commands/EndPunchCommand.cs`

- [x] Port `EndPunchCommand.cs` to Dart
  - **Classes**:
    - [x] `class EndPunchCommand`

## File: `./DimensionLib/Model/Commands/GossipPeer.cs`

- [x] Port `GossipPeer.cs` to Dart
  - **Classes**:
    - [x] `class GossipPeer`

## File: `./DimensionLib/Model/Commands/DataCommand.cs`

- [x] Port `DataCommand.cs` to Dart
  - **Classes**:
    - [x] `class DataCommand`

## File: `./DimensionLib/Model/Commands/RequestFolderContents.cs`

- [x] Port `RequestFolderContents.cs` to Dart
  - **Classes**:
    - [x] `class RequestFolderContents`

## File: `./DimensionLib/Model/Commands/MiniHello.cs`

- [x] Port `MiniHello.cs` to Dart
  - **Classes**:
    - [x] `class MiniHello`

## File: `./DimensionLib/Model/Commands/ReverseConnectionType.cs`

- [x] Port `ReverseConnectionType.cs` to Dart
  - **Classes**:
    - [x] `class ReverseConnectionType`

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

- [x] Port `AboutForm.cs` to Dart
  - **Classes**:
    - [x] `class AboutForm`

## File: `./Dimension/UI/DoubleBufferedListView.cs`

- [x] Port `DoubleBufferedListView.cs` to Dart
  - **Classes**:
    - [x] `class DoubleBufferedListView`

## File: `./Dimension/UI/FlashWindow.cs`

- [x] Port `FlashWindow.cs` to Dart
  - **Classes**:
    - [x] `class FlashWindow`
  - **Public Methods**:
    - [x] `Flash()`
    - [x] `Flash()`
    - [x] `Start()`
    - [x] `Stop()`
    - [x] `ApplicationIsActivated()`
  - **TODO**:
    - [ ] Hook a production `FlashWindowDriver` implementation into the Flutter desktop shell once `MainForm` is fully ported.

## File: `./Dimension/UI/DownloadQueuePanel.cs`

- [x] Port `DownloadQueuePanel.cs` to Dart
  - **Classes**:
    - [x] `class DownloadQueuePanel`
  - **TODO**:
    - [ ] Replace temporary empty-state panel with live queued-transfer data once `TransfersPanel` wiring is ported.

## File: `./Dimension/UI/LoadingForm.cs`

- [x] Port `LoadingForm.cs` to Dart
  - **Classes**:
    - [x] `class LoadingForm`
  - **TODO**:
    - [ ] Wire `LoadingForm` to the final app bootstrap lifecycle once `App`/`MainForm` startup orchestration is fully ported.

## File: `./Dimension/UI/ByteFormatter.cs`

- [x] Port `ByteFormatter.cs` to Dart
  - **Classes**:
    - [x] `class ByteFormatter`
  - **Public Methods**:
    - [x] `formatBytes()`

## File: `./Dimension/UI/LimitChangeDialog.cs`

- [x] Port `LimitChangeDialog.cs` to Dart
  - **Classes**:
    - [x] `class LimitChangeDialog`
  - **TODO**:
    - [ ] Replace temporary `InMemorySpeedLimitSettings` usage with app-wide settings wiring when `SettingsForm` and `MainForm` are ported.

## File: `./Dimension/UI/DateFormatter.cs`

- [x] Port `DateFormatter.cs` to Dart
  - **Classes**:
    - [x] `class DateFormatter`
  - **Public Methods**:
    - [x] `formatDate()`

## File: `./Dimension/UI/HTMLPanel.cs`

- [x] Port `HTMLPanel.cs` to Dart
  - **Classes**:
    - [x] `class HTMLPanel`
  - **Public Properties**:
    - [x] `isMono`
  - **TODO**:
    - [ ] Hook `HTMLPanel` join-circle callbacks into the final `JoinCircleForm`/`MainForm` flow once those Flutter ports are in place.

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

- [x] Port `RenameShareForm.cs` to Dart
  - **Classes**:
    - [x] `class RenameShareForm`

## File: `./Dimension/UI/SearchPanel.cs`

- [ ] Port `SearchPanel.cs` to Dart
  - **Classes**:
    - [ ] `class SearchPanel`
    - [ ] `class SearchThingy`

## File: `./Dimension/UI/NetworkStatusPanel.cs`

- [x] Port `NetworkStatusPanel.cs` to Dart
  - **Classes**:
    - [x] `class NetworkStatusPanel`
  - **TODO**:
    - [ ] Replace temporary snapshot provider wiring with live `Core`/`Bootstrap` adapters when those Flutter-first surfaces are stabilized.

## File: `./Dimension/UI/CirclePanel.cs`

- [ ] Port `CirclePanel.cs` to Dart
  - **Classes**:
    - [ ] `class CirclePanel`
  - **Public Methods**:
    - [x] `close()`
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
    - [x] `close()`
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
