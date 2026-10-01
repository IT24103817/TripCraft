import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/trips/data/trips_repository.dart';
import 'package:tripcraft_mobile/features/trips/presentation/new_trip_screen.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';

import '../helpers.dart';
import 'trip_fakes.dart';

void main() {
  late File photo;
  late FakeImagePicker picker;
  late FakeTripsRepository repository;

  setUp(() {
    final folder = Directory.systemTemp.createTempSync('tripcraft_photo');
    photo = File('${folder.path}/passport.png')..writeAsBytesSync(pngBytes);
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

  /// Fills every field, taps [cities] in that order, takes the photo and submits.
  Future<void> fillAndSubmit(WidgetTester tester, List<String> cities) async {
    await tester.enterText(
      find.widgetWithText(TextFormField, 'What would you like to do?'),
      '5 days in Kandy and Ella',
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

    for (final city in cities) {
      final chip = find.widgetWithText(FilterChip, city);
      await tester.scrollUntilVisible(chip, 200, scrollable: form());
      await tester.tap(chip);
      await tester.pumpAndSettle();
    }

    await tester.scrollUntilVisible(
      find.widgetWithText(TextFormField, 'Budget (USD)'),
      200,
      scrollable: form(),
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Budget (USD)'),
      '1500',
    );
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
  }

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

  testWidgets(
    'submitting sends the cities in the order tapped, uploads the photo and starts planning',
    (tester) async {
      await pumpNewTrip(tester);

      await fillAndSubmit(tester, ['Kandy', 'Ella']);

      expect(picker.usedSource, ImageSource.camera);
      expect(repository.created?.startDate, '2026-10-10');
      expect(repository.created?.endDate, '2026-10-14');
      expect(repository.created?.cities, ['Kandy', 'Ella']);
      expect(repository.created?.toJson()['cities'], ['Kandy', 'Ella']);
      expect(repository.uploadedPhotoPath, photo.path);
      expect(repository.planningStarted, isTrue);
      expect(find.text('Trip page trip-9'), findsOneWidget);
    },
  );

  testWidgets(
    'the chips show the route order and tapping again removes a city',
    (tester) async {
      await pumpNewTrip(tester);
      final ella = find.widgetWithText(FilterChip, 'Ella');
      await tester.scrollUntilVisible(ella, 200, scrollable: form());

      await tester.tap(ella);
      await tester.pumpAndSettle();
      await tester.tap(find.widgetWithText(FilterChip, 'Galle'));
      await tester.pumpAndSettle();
      expect(find.text('1. Ella'), findsOneWidget);
      expect(find.text('2. Galle'), findsOneWidget);
      expect(find.text('Route: Ella → Galle'), findsOneWidget);

      await tester.tap(find.widgetWithText(FilterChip, '1. Ella'));
      await tester.pumpAndSettle();
      expect(find.text('1. Galle'), findsOneWidget);
      expect(find.text('Route: Galle'), findsOneWidget);
    },
  );

  testWidgets('a 400 about the cities shows the API message under the cities', (
    tester,
  ) async {
    const message =
        "'Atlantis' is not a city we cover. Supported cities: Colombo, Ella, Galle, Kandy, Nuwara Eliya, Sigiriya.";
    repository.createError = const UserFacingException(
      message,
      statusCode: 400,
      fieldErrors: {
        'cities': [message],
      },
    );
    await pumpNewTrip(tester);

    await fillAndSubmit(tester, ['Kandy']);

    expect(repository.planningStarted, isFalse);
    await tester.scrollUntilVisible(
      find.text('Cities to visit'),
      -200,
      scrollable: form(),
    );
    // Once under the cities and once in the snackbar.
    expect(find.text(message), findsNWidgets(2));
  });
}
