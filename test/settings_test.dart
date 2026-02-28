import 'package:dimension/model/file_list_database.dart';
import 'package:dimension/model/settings.dart';
import 'package:flutter_test/flutter_test.dart';

class FakeSavableStore extends InMemoryStringKeyValueStore
    implements SavableStringKeyValueStore {
  int saveCallCount = 0;

  @override
  void save() {
    saveCallCount += 1;
  }
}

void main() {
  group('Settings', () {
    test('reads and writes primitive values', () {
      final settings = Settings();

      expect(settings.getString('missing', 'fallback'), 'fallback');
      expect(settings.getInt('missing-int', 42), 42);
      expect(settings.getULong('missing-ulong', 7), 7);
      expect(settings.getBool('missing-bool', true), isTrue);

      settings.setString('username', 'alice');
      settings.setInt('max peers', 88);
      settings.setULong('quota', 1024);
      settings.setBool('enabled', false);

      expect(settings.getString('username', 'fallback'), 'alice');
      expect(settings.getInt('max peers', 0), 88);
      expect(settings.getULong('quota', 0), 1024);
      expect(settings.getBool('enabled', true), isFalse);
    });

    test('deduplicates and removes string-array values', () {
      final settings = Settings();

      settings.addStringToArrayNoDup('circles', 'alpha');
      settings.addStringToArrayNoDup('circles', 'alpha');
      settings.addStringToArrayNoDup('circles', 'beta');

      expect(settings.getStringArray('circles'), <String>['alpha', 'beta']);

      settings.removeStringToArrayNoDup('circles', 'alpha');
      expect(settings.getStringArray('circles'), <String>['beta']);
    });

    test('save delegates to backend when supported', () {
      final store = FakeSavableStore();
      final settings = Settings(store: store);

      settings.save();

      expect(store.saveCallCount, 1);
    });
  });
}
