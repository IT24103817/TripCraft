import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/trips/data/template_models.dart';
import 'package:tripcraft_mobile/features/trips/data/tourist_profile.dart';
import 'package:tripcraft_mobile/features/trips/data/tourist_profile_repository.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_templates_repository.dart';

import '../helpers.dart';

/// A package as GET /api/trip-templates returns it; [withItinerary] adds the days (GET /api/trip-templates/{id}).
Map<String, dynamic> templateJson({
  String id = 'tpl-1',
  String slug = 'hill-country-escape',
  String name = 'Hill-country escape',
  bool withItinerary = false,
}) => {
  'id': id,
  'slug': slug,
  'name': name,
  'moodTag': 'Cool & green',
  'summary': 'Tea hills, misty lakes and the famous Nine Arches Bridge.',
  'objective':
      '4 relaxed days through the hill country: Kandy, Nuwara Eliya and Ella.',
  'days': 4,
  'cities': ['Kandy', 'Nuwara Eliya', 'Ella'],
  'preferences': {'language': 'en', 'transport': 'any', 'pace': 'relaxed'},
  'pricedForPax': 2,
  'fromPriceLkr': 192000,
  'fromPriceUsd': 640,
  'itinerary': withItinerary
      ? [
          {
            'day': 1,
            'city': 'Kandy',
            'stops': [
              {
                'attractionId': 'a1',
                'name': 'Temple of the Sacred Tooth Relic',
                'entryFeeLkr': 2000,
                'latitude': 7.2936,
                'longitude': 80.6413,
              },
            ],
          },
          {
            'day': 2,
            'city': 'Ella',
            'stops': [
              {
                'attractionId': 'a3',
                'name': 'Nine Arches Bridge',
                'entryFeeLkr': 0,
                'latitude': 6.8768,
                'longitude': 81.0608,
              },
            ],
          },
        ]
      : null,
};

/// Serves fixed packages and records a booking instead of calling the API.
/// When [calls] is given, "book" is added to it, so a test can check the order of the calls.
class FakeTripTemplatesRepository extends Fake
    implements TripTemplatesRepository {
  FakeTripTemplatesRepository({this.list, this.calls});

  final List<Map<String, dynamic>>? list;
  final List<String>? calls;
  BookTemplateRequest? booked;

  @override
  Future<List<TripTemplate>> templates() async => [
    for (final json in list ?? [templateJson()]) TripTemplate.fromJson(json),
  ];

  @override
  Future<TripTemplate> template(String id) async =>
      TripTemplate.fromJson(templateJson(id: id, withItinerary: true));

  @override
  Future<BookTemplateResult> book(
    String templateId,
    BookTemplateRequest request,
  ) async {
    calls?.add('book');
    booked = request;
    return BookTemplateResult.fromJson({
      'trip': tripJson(id: 'trip-9', status: 'Planning'),
      'planning': {
        'workflowId': 'wf-9',
        'workflowStatus': 'Planning',
        'tripStatus': 'Planning',
      },
    });
  }
}

/// Serves the tourist's profile (GET /api/tourists/me) and records the passport photo upload.
/// Set [uploadError] to make the upload fail like the API would; "upload" is added to [calls].
class FakeTouristProfileRepository extends Fake
    implements TouristProfileRepository {
  FakeTouristProfileRepository({
    this.hasPassportPhoto = false,
    this.nationality = '',
    this.calls,
  });

  bool hasPassportPhoto;
  final String nationality;
  final List<String>? calls;
  String? uploadedPhotoPath;
  Object? uploadError;

  TouristProfile get _profile => TouristProfile(
    nationality: nationality,
    passportNumberMasked: nationality.isEmpty ? '' : '****4567',
    hasPassportPhoto: hasPassportPhoto,
  );

  @override
  Future<TouristProfile> me() async => _profile;

  @override
  Future<TouristProfile> uploadPassportPhoto(String filePath) async {
    calls?.add('upload');
    final error = uploadError;
    if (error != null) throw error;
    uploadedPhotoPath = filePath;
    hasPassportPhoto = true;
    return _profile;
  }
}
