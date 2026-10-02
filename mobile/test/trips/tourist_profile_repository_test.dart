import 'dart:io';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/trips/data/tourist_profile.dart';
import 'package:tripcraft_mobile/features/trips/data/tourist_profile_repository.dart';

import '../helpers.dart';
import 'trip_fakes.dart';

void main() {
  late MockApiClient api;
  late TouristProfileRepository repository;

  setUpAll(() => registerFallbackValue(FormData()));

  setUp(() {
    api = MockApiClient();
    repository = TouristProfileRepository(api);
  });

  final profileJson = {
    'nationality': 'Australian',
    'passportNumberMasked': '****4567',
    'hasPassportPhoto': true,
  };

  test('me() reads GET /api/tourists/me', () async {
    when(() => api.get('/api/tourists/me'))
        .thenAnswer((_) async => profileJson);

    final profile = await repository.me();

    expect(
      profile,
      const TouristProfile(
        nationality: 'Australian',
        passportNumberMasked: '****4567',
        hasPassportPhoto: true,
      ),
    );
  });

  test('uploadPassportPhoto posts the file as multipart field "file" to /api/tourists/me/passport-photo', () async {
    final folder = Directory.systemTemp.createTempSync('tripcraft_photo');
    addTearDown(() => folder.deleteSync(recursive: true));
    final photo = File('${folder.path}/passport.png')
      ..writeAsBytesSync(pngBytes);
    when(() => api.postMultipart(any(), any()))
        .thenAnswer((_) async => profileJson);

    final profile = await repository.uploadPassportPhoto(photo.path);

    final captured = verify(() => api.postMultipart(captureAny(), captureAny()))
        .captured;
    expect(captured[0], '/api/tourists/me/passport-photo');
    final form = captured[1] as FormData;
    expect(form.files.single.key, 'file');
    expect(form.files.single.value.filename, 'passport.png');
    expect(form.files.single.value.length, pngBytes.length);
    expect(profile.hasPassportPhoto, isTrue);
  });
}
