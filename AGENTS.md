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
