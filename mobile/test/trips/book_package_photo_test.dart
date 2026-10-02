import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/trips/data/template_models.dart';
import 'package:tripcraft_mobile/features/trips/data/tourist_profile_repository.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_templates_repository.dart';
import 'package:tripcraft_mobile/features/trips/presentation/book_package_sheet.dart';
import 'package:tripcraft_mobile/features/trips/presentation/passport_photo_picker.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';

import '../helpers.dart';
import 'template_fakes.dart';
import 'trip_fakes.dart';

/// The passport photo step of the package "Book as is" sheet.
void main() {
  final today = DateTime(2026, 10, 2, 9);
  late File photo;
  late FakeImagePicker picker;
  late List<String> calls;
  late FakeTripTemplatesRepository templates;
  late GoRouter router;

  setUp(() {
    final folder = Directory.systemTemp.createTempSync('tripcraft_photo');
    photo = File('${folder.path}/passport.png')..writeAsBytesSync(pngBytes);
    addTearDown(() => folder.deleteSync(recursive: true));
    picker = FakeImagePicker(photo);
    calls = [];
    templates = FakeTripTemplatesRepository(calls: calls);
  });

  /// A page with an "Open" button that opens the sheet, inside a router so booking can open the new trip.
  Future<void> openSheet(
    WidgetTester tester,
    FakeTouristProfileRepository profiles,
  ) async {
    usePhoneSize(tester, const Size(412, 915));
    final template = TripTemplate.fromJson(templateJson());
    router = GoRouter(
      routes: [
        GoRoute(
          path: '/',
          builder: (_, _) => Scaffold(
            body: Builder(
              builder: (context) => TextButton(
                onPressed: () => showBookPackageSheet(context, template),
                child: const Text('Open'),
              ),
            ),
          ),
        ),
        GoRoute(
          path: '/trips/:id',
          builder: (_, s) =>
              Scaffold(body: Text('trip ${s.pathParameters['id']}')),
        ),
      ],
    );
    await tester.pumpWidget(
      ProviderScope(
        retry: (_, _) => null,
        overrides: [
          apiClientProvider.overrideWithValue(MockApiClient()),
          sessionStorageProvider.overrideWithValue(InMemorySessionStorage()),
          tripTemplatesRepositoryProvider.overrideWithValue(templates),
          touristProfileRepositoryProvider.overrideWithValue(profiles),
          imagePickerProvider.overrideWithValue(picker),
          fixedClock(today),
        ],
        child: MaterialApp.router(theme: buildAppTheme(), routerConfig: router),
      ),
    );
    await tester.tap(find.text('Open'));
    await tester.pumpAndSettle();
  }

  Finder sheet() => find
      .descendant(
        of: find.byType(BottomSheet),
        matching: find.byType(Scrollable),
      )
      .first;

  Future<void> tapInSheet(WidgetTester tester, String label) async {
    final target = find.text(label);
    await tester.scrollUntilVisible(target, 200, scrollable: sheet());
    await tester.tap(target);
    await tester.pumpAndSettle();
  }

  /// Picks 10 October as the start date and types the passport number (and [nationality] when given).
  Future<void> fillDetails(WidgetTester tester, {String? nationality}) async {
    await tester.tap(find.text('Choose a start date'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('10'));
    await tester.tap(find.text('OK'));
    await tester.pumpAndSettle();
    if (nationality != null) {
      await tester.enterText(
        find.widgetWithText(TextFormField, 'Nationality'),
        nationality,
      );
    }
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Passport number'),
      'N1234567',
    );
  }

  testWidgets(
    'without a photo on file the sheet asks for one and blocks Book until it is taken',
    (tester) async {
      final profiles = FakeTouristProfileRepository(calls: calls);
      await openSheet(tester, profiles);

      expect(find.text('Passport photo'), findsOneWidget);
      expect(find.text('Passport photo on file ✓'), findsNothing);

      await fillDetails(tester, nationality: 'Australian');
      await tapInSheet(tester, 'Book this trip');

      expect(find.text('Add a photo of your passport.'), findsOneWidget);
      expect(calls, isEmpty);
      expect(templates.booked, isNull);

      await tapInSheet(tester, 'Camera');
      expect(picker.usedSource, ImageSource.camera);
      expect(find.bySemanticsLabel('Passport photo selected'), findsOneWidget);
      expect(find.text('Add a photo of your passport.'), findsNothing);
    },
  );

  testWidgets('Book uploads the photo first, then books the package', (
    tester,
  ) async {
    final profiles = FakeTouristProfileRepository(calls: calls);
    await openSheet(tester, profiles);

    await fillDetails(tester, nationality: 'Australian');
    await tapInSheet(tester, 'Gallery');
    await tapInSheet(tester, 'Book this trip');

    expect(calls, ['upload', 'book']);
    expect(profiles.uploadedPhotoPath, photo.path);
    expect(templates.booked?.passportNumber, 'N1234567');
    expect(router.state.matchedLocation, '/trips/trip-9');
  });

  testWidgets(
    'with a photo on file there is no photo step and the nationality comes from the profile',
    (tester) async {
      final profiles = FakeTouristProfileRepository(
        hasPassportPhoto: true,
        nationality: 'Japanese',
        calls: calls,
      );
      await openSheet(tester, profiles);

      expect(find.text('Passport photo on file ✓'), findsOneWidget);
      expect(find.text('Camera'), findsNothing);
      expect(find.widgetWithText(TextFormField, 'Japanese'), findsOneWidget);

      await fillDetails(tester);
      await tapInSheet(tester, 'Book this trip');

      expect(calls, ['book']);
      expect(profiles.uploadedPhotoPath, isNull);
      expect(templates.booked?.nationality, 'Japanese');
      expect(router.state.matchedLocation, '/trips/trip-9');
    },
  );

  testWidgets('a rejected photo shows the API message and nothing is booked', (
    tester,
  ) async {
    const message = 'The passport photo must be a JPEG or PNG image.';
    final profiles = FakeTouristProfileRepository(calls: calls)
      ..uploadError = const UserFacingException(message, statusCode: 400);
    await openSheet(tester, profiles);

    await fillDetails(tester, nationality: 'Australian');
    await tapInSheet(tester, 'Camera');
    await tapInSheet(tester, 'Book this trip');

    expect(calls, ['upload']);
    expect(templates.booked, isNull);
    expect(find.text(message), findsOneWidget);
    // The sheet stays open so the tourist can pick another photo.
    expect(find.byType(BottomSheet), findsOneWidget);
    expect(router.state.matchedLocation, '/');
  });

  test(
    'the camera falls back to the gallery when it is not available',
    () async {
      final noCamera = _NoCameraPicker();

      final picked = await pickPassportPhoto(noCamera, ImageSource.camera);

      expect(noCamera.sources, [ImageSource.camera, ImageSource.gallery]);
      expect(picked?.path, 'from-gallery.png');
    },
  );
}

/// An image picker on a device without a camera: the camera throws, the gallery returns a file.
class _NoCameraPicker extends FakeImagePicker {
  _NoCameraPicker() : super(File('unused'));

  final sources = <ImageSource>[];

  @override
  Future<XFile?> pickImage({
    required ImageSource source,
    double? maxWidth,
    double? maxHeight,
    int? imageQuality,
    CameraDevice preferredCameraDevice = CameraDevice.rear,
    bool requestFullMetadata = true,
  }) async {
    sources.add(source);
    if (source == ImageSource.camera) throw Exception('no_available_camera');
    return XFile('from-gallery.png');
  }
}
