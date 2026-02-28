# AGENTS.md

## Repository Working Notes

- Prefer pure Dart implementations for model-layer ports, with Flutter UI dependencies only where UI rendering is required.
- For networking and file-system paths under porting, design constructor-injected interfaces first so unit tests can run with fakes/mocks and without external resources.
- If a C# unit is only partially ported, keep temporary compatibility shims (`PascalCase` wrappers) only when they ease line-by-line migration; track remaining parity work in `TODO.md`.
- Keep `TODO.md` checkboxes truthful: check an item only when implementation and reasonable tests are in place.
- No UI unit testing is required for WinForms-to-Flutter ports; prioritize implementation parity and keep TODO tracking accurate.
- Environment note: this container currently lacks `dart` and `flutter` CLIs, so formatting/analyze/test commands cannot be executed here until toolchain provisioning is added.
