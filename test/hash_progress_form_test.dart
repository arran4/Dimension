import 'package:dimension/ui/hash_progress_form.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('calculates progress ratio from snapshot', () {
    const snapshot = HashProgressSnapshot(
      currentFileLabel: 'foo/bar.txt',
      processedFiles: 25,
      totalFiles: 100,
    );

    expect(snapshot.progress, 0.25);
  });

  testWidgets('renders progress content', (tester) async {
    const snapshot = HashProgressSnapshot(
      currentFileLabel: 'foo/bar.txt',
      processedFiles: 3,
      totalFiles: 4,
    );

    await tester.pumpWidget(
      const MaterialApp(home: Scaffold(body: HashProgressForm(snapshot: snapshot))),
    );

    expect(find.text('Hashing progress'), findsOneWidget);
    expect(find.text('foo/bar.txt'), findsOneWidget);
    expect(find.text('3 / 4 files'), findsOneWidget);
  });
}
