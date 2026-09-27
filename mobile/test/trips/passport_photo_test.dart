import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_models.dart';
import 'package:tripcraft_mobile/features/trips/data/trips_repository.dart';
import 'package:tripcraft_mobile/features/trips/presentation/new_trip_screen.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';

import '../helpers.dart';

/// Stands in for the camera / gallery: always "picks" [file].
class FakeImagePicker extends Fake implements ImagePicker {
  FakeImagePicker(this.file);

  final File file;
  ImageSource? usedSource;

  @override
  Future<XFile?> pickImage({
    required ImageSource source,
    double? maxWidth,
    double? maxHeight,
    int? imageQuality,
    CameraDevice preferredCameraDevice = CameraDevice.rear,
    bool requestFullMetadata = true,
  }) async {
    usedSource = source;
    return XFile(file.path);
  }
}

/// Records what the screen sends instead of calling the API.
class FakeTripsRepository extends Fake implements TripsRepository {
  CreateTripRequest? created;
  String? uploadedPhotoPath;
  bool planningStarted = false;

  @override
  Future<TripRequest> create(CreateTripRequest request) async {
    created = request;
    return TripRequest.fromJson(tripJson(id: 'trip-9'));
  }

  @override
  Future<void> uploadPassportPhoto(String tripId, String filePath) async {
    uploadedPhotoPath = filePath;
  }

  @override
  Future<StartPlanningResult> startPlanning(String tripId) async {
    planningStarted = true;
    return StartPlanningResult.fromJson({
      'workflowId': 'wf-9',
      'workflowStatus': 'Planning',
      'tripStatus': 'Planning',
    });
  }
}

/// A tiny valid PNG (1 x 1 pixel).
const _pngBytes = <int>[
  0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, //
  0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
  0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00, 0x00, 0x00,
  0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
  0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49,
  0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82,
];

void main() {
  late File photo;
  late FakeImagePicker picker;
  late FakeTripsRepository repository;

  setUp(() {
    final folder = Directory.systemTemp.createTempSync('tripcraft_photo');
    photo = File('${folder.path}/passport.png')..writeAsBytesSync(_pngBytes);
    addTearDown(() => folder.deleteSync(recursive: true));
    picker = FakeImagePicker(photo);
    repository = FakeTripsRepository();
  });

  Future<void> pumpNewTrip(WidgetTester tester) async {
    final router = GoRouter(
      initialLocation: '/trips/new',
      routes: [
        GoRoute(
          path: '/trips/new',
          builder: (_, _) =>
              NewTripScreen(imagePicker: picker, today: DateTime(2026, 9, 26)),
        ),
        GoRoute(
          path: '/trips/:id',
          builder: (_, s) => Text('Trip page ${s.pathParameters['id']}'),
        ),
      ],
    );
    await tester.pumpWidget(
      ProviderScope(
        retry: (_, _) => null,
        overrides: [
          apiClientProvider.overrideWithValue(MockApiClient()),
          sessionStorageProvider.overrideWithValue(InMemorySessionStorage()),
          tripsRepositoryProvider.overrideWithValue(repository),
        ],
        child: MaterialApp.router(theme: buildAppTheme(), routerConfig: router),
      ),
    );
    await tester.pumpAndSettle();
  }

  Finder form() => find.byType(Scrollable).first;

  testWidgets('picking from the gallery shows the passport photo as selected', (
    tester,
  ) async {
    await pumpNewTrip(tester);

    await tester.scrollUntilVisible(
      find.text('Gallery'),
      200,
      scrollable: form(),
    );
    await tester.tap(find.text('Gallery'));
    await tester.pumpAndSettle();

    expect(picker.usedSource, ImageSource.gallery);
    expect(find.bySemanticsLabel('Passport photo selected'), findsOneWidget);
    expect(find.text('Selected: passport.png'), findsOneWidget);
    expect(find.text('Add a photo of your passport.'), findsNothing);
  });

  testWidgets('submitting uploads the picked photo and starts planning', (
    tester,
  ) async {
    await pumpNewTrip(tester);

    await tester.enterText(
      find.widgetWithText(TextFormField, 'What would you like to do?'),
      '5 days in Kandy and Ella',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Budget (USD)'),
      '1500',
    );

    // Choose the dates by typing them (the picker's text input mode).
    await tester.tap(find.text('Choose dates'));
    await tester.pumpAndSettle();
    await tester.tap(find.byIcon(Icons.edit_outlined));
    await tester.pumpAndSettle();
    await tester.enterText(
      find.widgetWithText(TextField, 'Start Date'),
      '10/10/2026',
    );
    await tester.enterText(
      find.widgetWithText(TextField, 'End Date'),
      '10/14/2026',
    );
    await tester.tap(find.text('OK'));
    await tester.pumpAndSettle();

    await tester.scrollUntilVisible(
      find.widgetWithText(TextFormField, 'Nationality'),
      200,
      scrollable: form(),
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Nationality'),
      'German',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Passport number'),
      'N1234567',
    );
    await tester.scrollUntilVisible(
      find.text('Camera'),
      200,
      scrollable: form(),
    );
    await tester.tap(find.text('Camera'));
    await tester.pumpAndSettle();
    await tester.scrollUntilVisible(
      find.text('Submit trip request'),
      200,
      scrollable: form(),
    );
    await tester.tap(find.text('Submit trip request'));
    await tester.pumpAndSettle();

    expect(picker.usedSource, ImageSource.camera);
    expect(repository.created?.startDate, '2026-10-10');
    expect(repository.created?.endDate, '2026-10-14');
    expect(repository.uploadedPhotoPath, photo.path);
    expect(repository.planningStarted, isTrue);
    expect(find.text('Trip page trip-9'), findsOneWidget);
  });
}
