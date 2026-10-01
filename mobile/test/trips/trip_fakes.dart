import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:image_picker/image_picker.dart';
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
