# Dimension

[![Flutter](https://img.shields.io/badge/Flutter-3.1.0+-02569B?logo=flutter&logoColor=white)](https://flutter.dev)
[![Dart](https://img.shields.io/badge/Dart-3.1.0+-0175C2?logo=dart&logoColor=white)](https://dart.dev)
[![Build Status](https://img.shields.io/github/actions/workflow/status/your-repo/dimension/flutter.yml?branch=main)](https://github.com/your-repo/dimension/actions)

**Dimension** is a modern Peer-to-Peer (P2P) file transfer and communication service.

Currently, Dimension is undergoing a major architecture migration from a legacy C# desktop application to a modern, cross-platform Flutter and Dart application. The goal is to provide a seamless P2P experience across Web, Mobile, and Desktop environments while maintaining a robust, testable, and pure-Dart core model.

## 🚀 Project Status

Dimension is in an **active porting phase**.
We are translating the original C# codebase into idiomatic Dart, moving away from process-blocking multi-threading in favor of Dart's asynchronous `Future` and `Stream` paradigms.

*   **Core Model**: The domain layer (`lib/model/`) is being rewritten in pure Dart. Networking, filesystem access, and other platform dependencies are constructor-injected to allow deterministic unit testing with fakes and mocks.
*   **UI/UX**: The application uses a Flutter-based adaptive workspace (`lib/ui/`) that scales gracefully across mobile, desktop, and web form factors. The UI maintains the spirit of the original desktop app but utilizes modern design principles.
*   **Progress**: Check out [TODO.md](TODO.md) for a comprehensive and active backlog of runtime adapters, UI integration parity, and release readiness tasks.

## 🏗 Architecture Overview

The Flutter port is organized to cleanly separate the business logic from the UI presentation:

*   **`lib/model/`**: Contains the pure-Dart domain model, Kademlia DHT routing logic, P2P network abstractions, and file system watcher components. Designed for maximum testability independent of Flutter.
*   **`lib/ui/`**: Contains the Flutter presentation layer, featuring an adaptive workspace (`AppShell`, responsive bounds, compact/expanded modes) and section primitives.
*   **Dependency Injection**: Heavy use of constructor injection ensures that platform specifics (like sockets, file systems, or window geometry) are decoupled from the core logic.

## 🛠 Getting Started

### Prerequisites

*   [Flutter SDK](https://docs.flutter.dev/get-started/install) (version `>=3.1.0 <4.0.0`)
*   [Dart SDK](https://dart.dev/get-dart)

### Setup & Run

1.  **Install dependencies**:
    ```bash
    flutter pub get
    ```

2.  **Run static analysis**:
    ```bash
    flutter analyze
    ```

3.  **Run the test suite**:
    ```bash
    flutter test
    ```

4.  **Run the application**:
    ```bash
    flutter run
    ```
    *(Note: Web builds are automatically deployed to GitHub Pages via CI when a new release tag is pushed.)*

## 🤝 Contributing

We welcome contributions as we port and stabilize Dimension!
When contributing, please refer to the detailed working notes in [AGENTS.md](AGENTS.md) and track outstanding issues in [TODO.md](TODO.md).

Key guidelines:
*   Prioritize pure Dart implementations for the model layer.
*   Consolidate related C# classes into single Dart files where appropriate.
*   Replace thread-blocking mechanisms (like `Thread.Sleep`) with Dart's `async`/`await`.
*   Represent C# event delegates using lists of callback functions.
*   Keep UI state management simple (`ChangeNotifier` / `ValueNotifier`).
*   Ensure all new features or ported code include corresponding tests.

## 📄 License

This project is licensed under the terms specified in [License.txt](License.txt).
