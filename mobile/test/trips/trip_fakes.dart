import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:image_picker/image_picker.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_detail_screen.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_models.dart';
import 'package:tripcraft_mobile/features/trips/data/trips_repository.dart';

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

/// Records what the new-trip screen sends instead of calling the API.
/// Set [createError] to make "create" fail like the API would.
class FakeTripsRepository extends Fake implements TripsRepository {
  CreateTripRequest? created;
  String? uploadedPhotoPath;
  bool planningStarted = false;
  Object? createError;

  @override
  Future<List<String>> cities() async => demoCities;

  @override
  Future<TripRequest> create(CreateTripRequest request) async {
    created = request;
    final error = createError;
    if (error != null) throw error;
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
const pngBytes = <int>[
  0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, //
  0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
  0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00, 0x00, 0x00,
  0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
  0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49,
  0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82,
];

const notFound = UserFacingException(
  'We could not find that.',
  statusCode: 404,
);

/// Stubs every call the trip screen makes for trip-1 in [status]. A null [workflow] or [itinerary] is a 404,
/// as before planning; a null [rating] means the guide is not rated yet.
void stubTrip(
  MockApiClient api,
  String status, {
  Map<String, dynamic>? workflow,
  Map<String, dynamic>? itinerary,
  Map<String, dynamic>? cancellation,
  Map<String, dynamic>? rating,
  List<Object> vouchers = const [],
}) {
  when(() => api.get('/api/trip-requests/trip-1'))
      .thenAnswer((_) async => tripJson(status: status));
  when(() => api.get('/api/trip-requests/trip-1/workflow'))
      .thenAnswer((_) async => workflow ?? (throw notFound));
  when(() => api.get('/api/trip-requests/trip-1/itinerary'))
      .thenAnswer((_) async => itinerary ?? (throw notFound));
  when(() => api.get('/api/trip-requests/trip-1/cancellation'))
      .thenAnswer((_) async => cancellation ?? cancellationJson());
  when(() => api.get('/api/trips/trip-1/vouchers'))
      .thenAnswer((_) async => vouchers);
  when(() => api.get('/api/trip-requests/trip-1/assignment')).thenAnswer(
    (_) async => {
      'tripRequestId': 'trip-1',
      'guideId': 'g1',
      'guideName': 'Nimal Perera',
      'guidePhone': '+94 77 123 4567',
      'vehicleRegistrationNo': 'CAB-1234',
      'vehicleType': 'Van',
      'vehicleSeats': 9,
    },
  );
  when(() => api.get('/api/trip-requests/trip-1/guide-rating'))
      .thenAnswer((_) async => rating ?? (throw notFound));
  when(() => api.get('/api/trip-requests/trip-1/history')).thenAnswer(
    (_) async => [
      {
        'at': '2026-09-26T04:12:54Z',
        'action': 'TripRequestCreated',
        'actor': 'Tourist',
        'toStatus': 'Submitted',
      },
      {
        'at': '2026-09-26T04:13:43Z',
        'action': 'TripRequestStatusChanged',
        'actor': 'System',
        'fromStatus': 'Planning',
        'toStatus': 'Submitted',
      },
    ],
  );
}

/// The trip screen for trip-1. The Accept / Decline panel is a stand-in that only shows its trip id.
Future<void> pumpTripDetail(WidgetTester tester, MockApiClient api) async {
  await pumpScreen(
    tester,
    TripDetailScreen(
      tripId: 'trip-1',
      decisionPanel: (tripId, _) => Text('decision-panel-$tripId'),
    ),
    api: api,
  );
  await tester.pumpAndSettle();
}

/// Scrolls the open tab until [finder] is on screen. Cards that come into view load their data and grow,
/// so after settling the target is brought on screen once more.
Future<void> scrollTo(WidgetTester tester, Finder finder) async {
  await tester.scrollUntilVisible(
    finder,
    200,
    scrollable: verticalScrollable(),
  );
  await tester.pumpAndSettle();
  await tester.ensureVisible(finder);
  await tester.pumpAndSettle();
}

/// Opens a tab of the trip screen ("Overview", "Itinerary" or "Vouchers").
Future<void> openTab(WidgetTester tester, String tab) async {
  await tester.tap(find.widgetWithText(Tab, tab));
  await tester.pumpAndSettle();
}
