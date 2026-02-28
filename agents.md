# Agent Guidelines for Dimension Flutter Port

This file contains instructions and context for agents working on this repository.

## General Porting Guidelines
- **Idiomatic Dart:** When porting from C#, ensure the resulting code follows Dart conventions (e.g., camelCase for variables/methods, PascalCase for classes). Avoid blindly copying C# idioms if Dart has a better standard approach.
- **File Consolidation:** The original C# project followed a strict one-class-per-file rule. In Dart, it is often more idiomatic to group closely related, small classes into a single file (e.g., grouping `ReliableIncomingConnection` and `ReliableOutgoingConnection` into `reliable_connection.dart`).
- **Async/Await:** Dart relies heavily on single-threaded event loops. Replace C# `Thread.Sleep` and synchronous blocking calls with `Future` and `async`/`await`.
- **Memory Management:** Be careful with static lists and long-lived timers (e.g., tracking download rates). Use `dispose()` methods and clean up event listeners/streams when connections close to avoid memory leaks.

## UI/UX Guidelines
- **Modern UI:** The user interface should look similar to the original C# desktop application in terms of layout and feature set, but it must use modern UI/UX principles.
- **Mobile Scalability:** The application is built with Flutter and should be responsive. Ensure that panels, lists, and controls scale down gracefully to mobile device screens. Use Flutter's layout widgets (e.g., `LayoutBuilder`, `Flexible`, `Expanded`, `MediaQuery`) to adapt the UI for both desktop and mobile form factors.
- **Libraries:** Feel free to incorporate modern, well-maintained Flutter UI libraries to achieve a polished look and feel, as long as they fit within the general design goals of the app.

## Porting Notes (2026-02)
- `lib/model/file_list_database.dart` is now a pure-Dart implementation backed by an injected `StringKeyValueStore` abstraction.
- Default storage is in-memory (`InMemoryStringKeyValueStore`) to keep unit tests deterministic and avoid filesystem/network dependencies.
- `getObject`/`setObject` rely on explicit JSON serializers instead of reflection, which keeps code mockable and Dart-native.
- Temporary bridge: `FileListDatabase` includes a `settingsStore` until `Settings` is fully ported and wired.
- `lib/model/serializer.dart` is now a pure-Dart packet serializer with an explicit codec registry (`register<T>`), so command serialization/deserialization can be tested with mocks and does not rely on reflection.
- Temporary follow-up: command codec registration is intentionally externalized; wire all command types during app bootstrap once `App`/startup flow is fully Dart-native.
- `lib/model/settings.dart` is now a pure-Dart implementation with an injected `StringKeyValueStore`, deterministic in-memory defaults, and explicit JSON handling for string-array settings.
- Temporary follow-up: `Settings.save()` only persists when the injected backend implements `SavableStringKeyValueStore`; wire a disk-backed implementation during app bootstrap.

- `lib/model/udt_connection.dart` now uses a pure-Dart `UdtTransport` abstraction plus injected `Serializer`, replacing throw-only stubs and enabling deterministic unit tests with in-memory/mock transports.
- `lib/model/udt_incoming_connection.dart` and `lib/model/udt_outgoing_connection.dart` are now temporary compatibility shims that export the pure-Dart `udt_connection.dart` classes, preventing accidental drift back to commented C# snapshots.
- `lib/model/global_speed_limiter.dart` now supports injected settings lookup (`SpeedLimitProvider`) and configurable tick intervals so throttling behavior can be tested without filesystem/network dependencies.
- Temporary follow-up: wire a production `UdtTransport` implementation (FFI or `RawDatagramSocket`) during bootstrap; current implementation intentionally focuses on command framing/callback behavior.
- Temporary follow-up: after legacy imports are cleaned up, remove the UDT compatibility shim files and import `udt_connection.dart` directly everywhere.
- `lib/ui/flash_window.dart` is now a pure-Dart façade over an injectable `FlashWindowDriver`, so flash behavior can be mocked in tests without Win32 bindings.
- Temporary follow-up: add a production desktop `FlashWindowDriver` (via platform channels or FFI) when the Flutter desktop shell is connected.
- `lib/ui/limit_change_dialog.dart` is now a Flutter `AlertDialog` backed by pure-Dart conversion logic (`LimitChangeLogic`) and an injected `SpeedLimitSettings` abstraction for deterministic widget/unit tests.
- Temporary follow-up: replace temporary in-memory settings wiring with real app settings once `MainForm`/`SettingsForm` Flutter ports are in place.
- `lib/ui/about_form.dart` is now a Flutter `AlertDialog`-based implementation with injectable app/version/description content so UI behavior stays testable without native resources.
- `lib/ui/loading_form.dart` is now a pure-Dart/Flutter loading widget driven by an injected `LoadingStatusSource`, enabling deterministic polling/startup tests with mocks.
- `lib/ui/network_status_panel.dart` is now a Flutter panel backed by injectable snapshot/provider abstractions plus pure formatter helpers for deterministic unit/widget tests.
- `lib/ui/html_panel.dart` is now a pure Flutter panel with explicit `dimensionbootstrap://` / `dimensionlan://` parsing and injectable join callbacks, so link-handling behavior is unit/widget testable without networking.
- `lib/ui/rename_share_form.dart`, `lib/ui/double_buffered_list_view.dart`, and `lib/ui/download_queue_panel.dart` are now pure Flutter ports with deterministic widget behavior and no WinForms dependencies.
- Temporary follow-up: wire `HTMLPanel` and `DownloadQueuePanel` to `MainForm`/transfer state adapters once those larger UI surfaces are fully ported.
- Temporary follow-up: connect `LoadingStatusSource` and `NetworkStatusProvider` to real app bootstrap/core adapters once `App` and `MainForm` orchestration is fully ported.
