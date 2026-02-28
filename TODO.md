# Dimension Flutter Port TODO List

This document outlines the tasks required to port the C# application to a Dart/Flutter application.

## Environment / Tooling Follow-ups

- [ ] Install or provision Flutter/Dart SDK in CI/dev container so `flutter analyze` and widget tests can run during porting PRs.
  - Current container check (2026-02-28): `flutter` and `dart` CLIs are unavailable (`command not found`), so analysis/test commands remain blocked in this environment.

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
    - [x] `isMono`
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
    - [x] `Dispose()`
    - [ ] `beginSearch()`
    - [ ] `leaveCircle()`
    - [ ] `joinCircle()`
    - [ ] `addPeer()`
    - [ ] `sendChat()`
    - [ ] `addIncomingConnection()`
    - [ ] `removeIncomingConnection()`
    - [x] `chatReceived()`
    - [ ] `getIdleTime()`
  - **Public Properties**:
    - [x] `isMono`
  - **TODO**:
    - [ ] Pending line-by-line parity for peer lifecycle, search fan-out, and transfer routing against the original C# flow.

## File: `./DimensionLib/Model/FileList.cs`

- [ ] Port `FileList.cs` to Dart
  - **Classes**:
    - [ ] `class FileList`
  - **Public Methods**:
    - [ ] `update()`
    - [ ] `clear()`
    - [x] `Dispose()`
    - [ ] `getRootShare()`
    - [ ] `getFolder()`
    - [ ] `getFile()`
    - [ ] `getFullPath()`
    - [ ] `getFSListing()`
    - [ ] `getFSListing()`
    - [ ] `doSave()`
    - [ ] `startUpdate()`

## File: `./DimensionLib/Model/Bootstrap.cs`

- [x] Port `Bootstrap.cs` to Dart
  - **Classes**:
    - [x] `class Bootstrap`
  - **Public Methods**:
    - [x] `Dispose()`
    - [x] `WriteLine()`
    - [x] `Write()`
    - [x] `join()`
  - **TODO**:
    - [ ] Port `launch()` network bootstrap/NAT traversal path with injectable socket + STUN abstractions (currently intentionally omitted to keep tests pure-Dart and mock-driven).

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
    - [x] Remove temporary compatibility shim exports once all imports have been consolidated on `lib/model/udt_connection.dart`.

## File: `./DimensionLib/Model/Peer.cs`

- [ ] Port `Peer.cs` to Dart
  - **Classes**:
    - [ ] `class Peer`
  - **Public Methods**:
    - [ ] `downloadElement()`
    - [x] `endpointIsInHistory()`
    - [x] `addEndpointToHistory()`
    - [ ] `downloadFilePath()`
    - [ ] `commandReceived()`
    - [ ] `updateTransfers()`
    - [ ] `sendCommand()`
    - [ ] `reverseConnect()`
    - [ ] `createConnection()`
    - [ ] `endPunch()`
    - [ ] `releasePunch()`
    - [x] `chatReceived()`
  - **Public Properties**:
    - [x] `maybeDead`
    - [x] `probablyDead`
    - [x] `assumingDead`
    - [x] `isLocal`

## File: `./DimensionLib/Model/SystemLog.cs`

- [x] Port `SystemLog.cs` to Dart
  - **Classes**:
    - [x] `class SystemLog`
  - **Public Methods**:
    - [x] `addEntry()`

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

- [x] Port `Kademlia.cs` to Dart
  - **Classes**:
    - [x] `class Kademlia`
  - **Public Methods**:
    - [x] `Dispose()`
    - [x] `initialize()`
    - [x] `announce()`
    - [x] `doLookup()`


  - **TODO**:
    - [ ] Wire `Kademlia` backend to a production DHT transport during app bootstrap (current implementation is pure-Dart and constructor-injected for testability).

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

- [x] Port `UpdatingForm.cs` to Dart
  - **Classes**:
    - [x] `class UpdatingForm`
  - **TODO**:
    - [ ] Current updater implementation is an injectable pure-Dart controller (`UpdatingFormController`) without direct filesystem/network access; wire concrete downloader/installer adapters during app bootstrap.

## File: `./Updater/Program.cs`

- [x] Port `Program.cs` to Dart
  - **Classes**:
    - [x] `class Program`
  - **Public Methods**:
    - [x] `needsUpdate()`
    - [x] `downloadPath()`
  - **TODO**:
    - [ ] `Program` is now represented by a pure-Dart startup orchestrator with injected update gate and single-instance guard; add production adapters for OS mutex and updater endpoints.

## File: `./Dimension/Program.cs`

- [x] Port `Program.cs` to Dart
  - **Classes**:
    - [x] `class Program`
  - **TODO**:
    - [ ] Integrate the pure-Dart startup `Program` with real Flutter app shell entrypoints (`runApp` + loading/main route orchestration).

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
  - **TODO**:
    - [ ] Centralize user-facing invalid-link messaging in app-level notification/translation plumbing once available.

## File: `./Dimension/UI/SettingsForm.cs`

- [x] Port `SettingsForm.cs` to Dart
  - **Classes**:
    - [x] `class SettingsForm`
  - **TODO**:
    - [ ] Expand the current Flutter `SettingsForm` coverage beyond username/description/play-sounds controls as additional settings flows are ported.

## File: `./Dimension/UI/HashProgressForm.cs`

- [x] Port `HashProgressForm.cs` to Dart
  - **Classes**:
    - [x] `class HashProgressForm`

## File: `./Dimension/UI/IconReader.cs`

- [x] Port `IconReader.cs` to Dart
  - **Classes**:
    - [x] `class IconReader`
    - [x] `class Shell32`
    - [x] `class User32`
  - **TODO**:
    - [ ] Desktop shell parity currently uses a pure-Dart icon descriptor/fallback mapper; add optional platform-channel icon extraction if exact native icons are required.

## File: `./Dimension/UI/TransfersPanel.cs`

- [x] Port `TransfersPanel.cs` to Dart
  - **Classes**:
    - [x] `class TransfersPanel`
  - **Public Properties**:
    - [x] `isMono`
  - **TODO**:
    - [ ] Wire `TransfersPanel` row actions (cancel/retry/open) to finalized transfer commands once `MainForm` command routing is fully ported.

## File: `./Dimension/UI/RenameShareForm.cs`

- [x] Port `RenameShareForm.cs` to Dart
  - **Classes**:
    - [x] `class RenameShareForm`

## File: `./Dimension/UI/SearchPanel.cs`

- [x] Port `SearchPanel.cs` to Dart
  - **Classes**:
    - [x] `class SearchPanel`
    - [x] `class SearchThingy`
  - **TODO**:
    - [ ] Wire `SearchPanelController` to live `Core` search streams and peer download hooks once `MainForm` orchestration is fully ported.

## File: `./Dimension/UI/NetworkStatusPanel.cs`

- [x] Port `NetworkStatusPanel.cs` to Dart
  - **Classes**:
    - [x] `class NetworkStatusPanel`
  - **TODO**:
    - [ ] Replace temporary snapshot provider wiring with live `Core`/`Bootstrap` adapters when those Flutter-first surfaces are stabilized.

## File: `./Dimension/UI/CirclePanel.cs`

- [x] Port `CirclePanel.cs` to Dart
  - **Classes**:
    - [x] `class CirclePanel`
  - **Public Methods**:
    - [x] `close()`
    - [x] `chatReceived()`
    - [x] `select()`
    - [x] `unselect()`
  - **Public Properties**:
    - [x] `isMono`
  - **TODO**:
    - [ ] Wire `CirclePanel` file browsing actions to live `Peer.controlConnection` (`GetFileListing`) and download commands when `MainForm` orchestration is finalized.

## File: `./Dimension/UI/FinishedTransfersPanel.cs`

- [x] Port `FinishedTransfersPanel.cs` to Dart
  - **Classes**:
    - [x] `class FinishedTransfersPanel`
  - **TODO**:
    - [ ] Replace temporary status strings with transfer-history data sourced from the finalized transfer domain adapters.

## File: `./Dimension/UI/UserPanel.cs`

- [x] Port `UserPanel.cs` to Dart
  - **Classes**:
    - [x] `class UserPanel`
  - **Public Methods**:
    - [x] `close()`
    - [x] `selectChat()`
    - [x] `displayMessage()`
    - [x] `addLine()`
  - **TODO**:
    - [ ] Wire `UserPanel` send-message/reconnect actions to live peer control/data connections when `MainForm` command routing is ported.

## File: `./Dimension/UI/JoinCircleForm.cs`

- [x] Port `JoinCircleForm.cs` to Dart
  - **Classes**:
    - [x] `class JoinCircleForm`
  - **Public Methods**:
    - [x] `joinCircle()`
  - **TODO**:
    - [ ] Surface bootstrap-join validation errors in the app-wide notification/translation system when `MainForm` UX wiring is ported.

## File: `./Dimension/UI/MainForm.cs`

- [x] Port `MainForm.cs` to Dart
  - **Classes**:
    - [x] `class MainForm`
  - **Public Methods**:
    - [x] `selectUser()`
    - [x] `privateChatReceived()`
    - [x] `flash()`
    - [x] `addOrSelectPanel()`
    - [x] `addInternetCircle()`
    - [x] `setColors()`
  - **Public Properties**:
    - [x] `isMono`
  - **TODO**:
    - [ ] Wire `TransfersPanel` row actions (cancel/retry/open) to finalized transfer commands once `MainForm` command routing is fully ported.
    - [ ] Replace temporary in-controller tab registry with route-aware navigation state once app-wide deep linking is introduced.
