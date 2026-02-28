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
