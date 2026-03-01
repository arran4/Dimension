# AGENTS.md

## Repository Working Notes

- Prefer pure Dart implementations for model-layer ports, with Flutter UI dependencies only where UI rendering is required.
- For networking and file-system paths under porting, design constructor-injected interfaces first so unit tests can run with fakes/mocks and without external resources.
- If a C# unit is only partially ported, keep temporary compatibility shims (`PascalCase` wrappers) only when they ease line-by-line migration; track remaining parity work in `TODO.md`.
- Keep `TODO.md` checkboxes truthful: check an item only when implementation and reasonable tests are in place.
- No UI unit testing is required for WinForms-to-Flutter ports; prioritize implementation parity and keep TODO tracking accurate.
- Environment note: this container currently lacks `dart` and `flutter` CLIs, so formatting/analyze/test commands cannot be executed here until toolchain provisioning is added.

## Porting Notes (2026-02)
- `lib/model/system_log.dart` now provides a pure-Dart `SystemLog` with injectable clock/disposal/writer hooks so logging remains deterministic in tests and avoids direct filesystem coupling.
- `lib/model/kademlia.dart` now uses an injected `KademliaBackend` interface and pure-Dart hashing/deduped lookup behavior, so announce/lookup logic can be tested with mocks before wiring real DHT transport.
- `lib/model/peer.dart` now tracks endpoint history via pure-Dart `PeerEndpoint` value objects (with dedupe + bounded history) to preserve migration parity without networking dependencies.
- `lib/ui/main_form.dart` now provides a Flutter-native `MainFormController`/`MainForm` with injectable flash/sound/settings dependencies and deterministic tab orchestration (`addOrSelectPanel`, `selectUser`, `privateChatReceived`, `addInternetCircle`, `setColors`) for mock-driven tests.
- Temporary follow-up: connect `MainFormController` tab/actions to finalized routing + transfer command dispatch once `App`/`Core` orchestration is fully ported.
- `lib/model/core.dart` now provides a pure-Dart `Core` with constructor-injected peer/settings/idle-time abstractions, deterministic chat sequencing, and temporary compatibility shims (`PascalCase` wrappers) to keep line-by-line migration progress testable without networking/runtime globals.
- `lib/model/file_list.dart` now provides a pure-Dart `FileList` with constructor-injected `FileListDatabase` + optional `FileListShareScanner` bridge so lookup/update behavior can be unit-tested with fakes before real filesystem watcher wiring.
- `lib/ui/app_shell.dart` now introduces a Flutter app shell layer with shared theme tokens, responsive breakpoints, and deep-linkable route state (`AppShellController`) so UI architecture parity can grow independently of model/network bootstrap completion.
- `lib/app.dart` now provides a pure-Dart `App` orchestrator with constructor-injected update/lifecycle/network/bootstrap interfaces so startup/update/cleanup/flash-chat events can be unit-tested without process, filesystem, or socket dependencies.
- `lib/model/peer.dart` now includes a pure-Dart command/transfer orchestration layer (`downloadElement`, `downloadFilePath`, `sendCommand`, `reverseConnect`, `createConnection`, `updateTransfers`) with queueing + callbacks that remain fully testable with fake outgoing connections while real TCP/UDT pipelines are finalized.
- UI state-management policy for current porting phase: keep KISS with internal Flutter notifiers (`ChangeNotifier`/`ValueNotifier`) only; avoid external state packages unless a specific feature proves they are required.
- `lib/model/core.dart` now includes an `addPeer` path that delegates to mutable injected peer directories (`CorePeerMutableDirectory`) while safely no-oping on read-only directories, keeping peer-lifecycle migration testable without global runtime state.
- `lib/model/core.dart` now partially routes incoming commands (`CancelCommand`, `GetFileListing`) with injectable `CoreFileListingProvider`, so command-side behavior is testable without filesystem/runtime globals while deeper transfer/search routing remains TODO-tracked.
- `lib/ui/core_screens.dart` now provides a notifier-driven shared Core Screens scaffold (Circles, Peers, Chat, Search, Transfers, Settings, Diagnostics) with consistent loading/empty/error states, giving the Flutter port a concrete cross-platform flow shell before live backend actions are fully wired.
- `lib/model/udt_backend.dart` now provides a pluggable UDT backend abstraction with an in-memory mock backend + connection factory so command flow can be tested and wired without real sockets while `dart-udt` integration is completed.
