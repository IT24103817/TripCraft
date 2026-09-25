import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

/// Strict feature folders: a feature may import core and shared, never another feature;
/// shared never imports core or features.
void main() {
  final features = Directory('lib/features')
      .listSync()
      .whereType<Directory>()
      .map((d) => d.uri.pathSegments[d.uri.pathSegments.length - 2])
      .toList();

  Iterable<(String, String)> importsIn(String folder) sync* {
    for (final file in Directory(
      folder,
    ).listSync(recursive: true).whereType<File>()) {
      if (!file.path.endsWith('.dart') ||
          file.path.endsWith('.g.dart') ||
          file.path.endsWith('.freezed.dart')) {
        continue;
      }
      for (final line in file.readAsLinesSync().where(
        (l) => l.startsWith('import ') || l.startsWith('export '),
      )) {
        yield (file.path, line);
      }
    }
  }

  test(
    'features exist',
    () => expect(features, containsAll(['trips', 'resources', 'quotations'])),
  );

  for (final feature in ['trips', 'resources', 'quotations']) {
    test('$feature does not import another feature', () {
      final others = features.where((f) => f != feature);
      final violations = importsIn('lib/features/$feature')
          .where(
            (e) => others.any(
              (o) => e.$2.contains('features/$o/') || e.$2.contains('../$o/'),
            ),
          )
          .toList();
      expect(violations, isEmpty);
    });
  }

  test('shared depends on nothing else in the app', () {
    final violations = importsIn('lib/shared')
        .where((e) => e.$2.contains('core/') || e.$2.contains('features/'))
        .toList();
    expect(violations, isEmpty);
  });
}
