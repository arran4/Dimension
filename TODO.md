# Dimension Flutter Port TODO (Active Backlog)

This file intentionally tracks only **remaining** work. Completed milestones were removed to keep the backlog actionable.

## 0) Environment & CI reliability

- [ ] Ensure local/dev container Flutter + Dart SDK availability for contributor workflows.
  - CI now runs formatting/analyze/model tests, but this container still lacks local CLIs.
- [ ] Add a short contributor runbook (`flutter pub get`, `flutter analyze`, targeted test commands, troubleshooting) for consistent local validation.

## 1) Runtime adapter completion (model parity unblockers)

- [ ] Wire concrete runtime adapters into app bootstrap for already-ported interfaces:
  - UDP transport adapter
  - serializer/command codec registration
  - lifecycle bridge
  - settings persistence bridge
- [ ] Complete native transport integration path:
  - finalize `dart-udt`/FFI-backed implementation behind existing UDT abstractions
  - preserve in-memory mock backend for deterministic tests
- [x] Finish Core command routing parity beyond current partial handling (`CancelCommand`, `GetFileListing`), including search-result and transfer/file-chunk paths.
  - [x] Route `SearchResultCommand` through injected sink abstraction (`CoreSearchResultSink`) for deterministic handling in tests.
  - [x] Route transfer chunk commands (`FileChunk`, `RequestChunks`) through injected transfer router (`CoreTransferRouter`) without direct socket/filesystem coupling.
  - [x] Track in-core command side effects for routed paths (keyword-indexed search results + latest transfer event state) through pure-Dart state so runtime adapters can consume deterministic command outcomes.
- [x] Replace temporary `FileList` snapshot scanner bridge with incremental filesystem watcher adapter during bootstrap.
  - [x] Add injectable incremental watcher abstraction (`FileListIncrementalWatcher`) and wire live snapshot application in `FileList`.
  - [x] Implement concrete filesystem-backed watcher adapter in bootstrap/runtime wiring (current tests use in-memory stream watcher fakes).
  - [x] Persist and reconcile watcher-driven deltas with hashing/index update lifecycle parity.
- [x] Complete bootstrap network/NAT traversal launch path via injectable socket/STUN abstractions.

## 2) UI integration parity (beyond scaffolding)

- [x] Replace temporary/mock-driven UI actions with live `Core`/transport-backed dispatch where model adapters are complete.
- [x] Connect `MainFormController` tab/actions to finalized app routing + transfer command dispatch.
- [x] Standardize reusable component primitives used across screens (buttons/inputs/dialogs/list rows/status indicators/transfer progress).
- [-] Validate desktop behavior at common and ultrawide resolutions with manual QA and fix discovered layout issues. (Skipped here: UI validation is handled manually by people.)
- [-] Validate browser compatibility/fidelity across Chromium, Firefox, and Safari-class engines. (Skipped here: UI validation is handled manually by people.)
- [ ] Optimize web initial load strategy (defer non-critical modules where practical).

## 3) Performance and QA process

- [ ] Profile list-heavy surfaces (chat/search/transfers) and add prioritized optimizations (pagination/lazy loading/rebuild reduction).
- [-] Keep a lightweight manual UI QA checklist per milestone (mobile/desktop/web smoke pass for navigation, accessibility, core flows). (Skipped here: manual UI QA is handled by people.)
- [-] Add a minimal integration test plan for critical end-to-end user journeys once adapter wiring stabilizes. (Skipped here: UI-side validation/testing is handled by people.)

## 4) Release readiness

- [x] Mobile release readiness
  - [x] Android+iOS permissions, icons, splash screens, and store metadata final pass
- [x] Desktop release readiness
  - [x] packaging/signing/notarization strategy + updater workflow alignment
- [x] Web release readiness
  - [x] production hosting config, cache strategy, rollout/versioning plan
- [ ] Maintain a per-platform parity checklist for major C# user-visible feature coverage.

## Notes

- Model layer remains pure-Dart first; networking/filesystem/platform concerns continue behind constructor-injected interfaces.
- KISS UI state management policy remains in effect (`ChangeNotifier` / `ValueNotifier` only unless justified).
