# Bug Report: Failing model tests in `main`

The automated test suite has 3 failing model layer tests existing on the default branch that require further refactoring in the `core` and `file_list` domain objects.

## Failures
1. **`test/core_test.dart`**: `sendChat handles /nick locally and clips username`
   - *Issue*: `settings.values['Username']` expects `'super-long-userna'`, but actual string clipping yields `'super-long-usern'`.
2. **`test/file_list_test.dart`**: `update + scanner populate listing indexes`
   - *Issue*: The File/Folder query by path returns `<null>` instead of the expected directory tree objects. The path traversal or folder relationships inside `FileList` are not properly populated by the mock `_Scanner`.
3. **`test/file_list_test.dart`**: `snapshot reconcile removes stale entries and keeps hash progress bounded`
   - *Issue*: Also fails to resolve the mock `File` structure during snapshot reconciliation due to incomplete index matching logic or root share ID linkages.

These require codebase refactoring to properly address path traversal inside `FileList` which are outside the scope of CI migration.
