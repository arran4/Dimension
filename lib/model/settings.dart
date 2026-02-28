import 'dart:convert';

import 'file_list_database.dart';

/// Optional extension point for stores that support explicit persistence.
abstract class SavableStringKeyValueStore implements StringKeyValueStore {
  void save();
}

/// Pure-Dart settings store with injected key-value backend.
class Settings {
  Settings({StringKeyValueStore? store})
      : _store = store ?? InMemoryStringKeyValueStore();

  final StringKeyValueStore _store;
  final Map<String, String> _cache = <String, String>{};

  void save() {
    if (_store is SavableStringKeyValueStore) {
      (_store as SavableStringKeyValueStore).save();
    }
  }

  List<String> getStringArray(String name) {
    final key = 'qs$name';
    final cached = _cache[key];
    if (cached != null) {
      return _decodeStringArray(cached);
    }

    final raw = _store.get(key);
    final normalized = (raw == null || raw.isEmpty) ? '[]' : raw;
    _cache[key] = normalized;
    return _decodeStringArray(normalized);
  }

  void addStringToArrayNoDup(String name, String value) {
    final values = getStringArray(name);
    if (values.contains(value)) {
      return;
    }

    setStringArray(name, <String>[...values, value]);
  }

  void removeStringToArrayNoDup(String name, String value) {
    if (value.isEmpty) {
      return;
    }

    final values = getStringArray(name);
    if (!values.contains(value)) {
      return;
    }

    setStringArray(
      name,
      values.where((entry) => entry != value).toList(growable: false),
    );
  }

  void setStringArray(String name, List<String> value) {
    final key = 'qs$name';
    final encoded = jsonEncode(value);
    _cache[key] = encoded;
    _store.set(key, encoded);
  }

  int getULong(String name, int defaultValue) {
    return _readInt('i$name', defaultValue);
  }

  void setBool(String name, bool value) {
    final key = 'b$name';
    final encoded = value.toString();
    _cache[key] = encoded;
    _store.set(key, encoded);
  }

  bool getBool(String name, bool defaultValue) {
    final key = 'b$name';
    final cached = _cache[key];
    if (cached != null) {
      return _parseBool(cached, defaultValue);
    }

    final raw = _store.get(key);
    final normalized = raw == null || raw.isEmpty ? defaultValue.toString() : raw;
    _cache[key] = normalized;
    return _parseBool(normalized, defaultValue);
  }

  void setString(String name, String value) {
    final key = 's$name';
    _cache[key] = value;
    _store.set(key, value);
  }

  String getString(String name, String defaultValue) {
    final key = 's$name';
    final cached = _cache[key];
    if (cached != null) {
      return cached;
    }

    final raw = _store.get(key);
    if (raw == null || raw.isEmpty) {
      return defaultValue;
    }

    _cache[key] = raw;
    return raw;
  }

  void setULong(String name, int value) {
    _writeInt('i$name', value);
  }

  void setInt(String name, int value) {
    _writeInt('i$name', value);
  }

  int getInt(String name, int defaultValue) {
    return _readInt('i$name', defaultValue);
  }

  List<String> _decodeStringArray(String encoded) {
    final decoded = jsonDecode(encoded);
    if (decoded is! List<dynamic>) {
      return <String>[];
    }
    return decoded.map((entry) => '$entry').toList(growable: false);
  }

  bool _parseBool(String value, bool defaultValue) {
    final normalized = value.trim().toLowerCase();
    if (normalized == 'true') {
      return true;
    }
    if (normalized == 'false') {
      return false;
    }
    return defaultValue;
  }

  void _writeInt(String key, int value) {
    final encoded = '$value';
    _cache[key] = encoded;
    _store.set(key, encoded);
  }

  int _readInt(String key, int defaultValue) {
    final cached = _cache[key];
    if (cached != null) {
      return int.tryParse(cached) ?? defaultValue;
    }

    final raw = _store.get(key);
    if (raw == null || raw.isEmpty) {
      return defaultValue;
    }

    _cache[key] = raw;
    return int.tryParse(raw) ?? defaultValue;
  }
}
