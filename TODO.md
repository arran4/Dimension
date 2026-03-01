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
- [ ] Finish Core command routing parity beyond current partial handling (`CancelCommand`, `GetFileListing`), including search-result and transfer/file-chunk paths.
- [ ] Replace temporary `FileList` snapshot scanner bridge with incremental filesystem watcher adapter during bootstrap.
- [ ] Complete bootstrap network/NAT traversal launch path via injectable socket/STUN abstractions.

## 2) UI integration parity (beyond scaffolding)

- [ ] Replace temporary/mock-driven UI actions with live `Core`/transport-backed dispatch where model adapters are complete.
- [ ] Connect `MainFormController` tab/actions to finalized app routing + transfer command dispatch.
- [ ] Standardize reusable component primitives used across screens (buttons/inputs/dialogs/list rows/status indicators/transfer progress).
- [ ] Validate desktop behavior at common and ultrawide resolutions with manual QA and fix discovered layout issues.
- [ ] Validate browser compatibility/fidelity across Chromium, Firefox, and Safari-class engines.
- [ ] Optimize web initial load strategy (defer non-critical modules where practical).

## 3) Performance and QA process

- [ ] Profile list-heavy surfaces (chat/search/transfers) and add prioritized optimizations (pagination/lazy loading/rebuild reduction).
- [ ] Keep a lightweight manual UI QA checklist per milestone (mobile/desktop/web smoke pass for navigation, accessibility, core flows).
- [ ] Add a minimal integration test plan for critical end-to-end user journeys once adapter wiring stabilizes.

## 4) Release readiness

- [ ] Mobile release readiness
  - [ ] Android+iOS permissions, icons, splash screens, and store metadata final pass
- [ ] Desktop release readiness
  - [ ] packaging/signing/notarization strategy + updater workflow alignment
- [ ] Web release readiness
  - [ ] production hosting config, cache strategy, rollout/versioning plan
- [ ] Maintain a per-platform parity checklist for major C# user-visible feature coverage.

## Notes

- Model layer remains pure-Dart first; networking/filesystem/platform concerns continue behind constructor-injected interfaces.
- KISS UI state management policy remains in effect (`ChangeNotifier` / `ValueNotifier` only unless justified).
