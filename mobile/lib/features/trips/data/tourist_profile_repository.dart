import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import 'tourist_profile.dart';

part 'tourist_profile_repository.g.dart';

/// The signed-in tourist's own profile (v1.1): used by the package "Book as is" sheet.
class TouristProfileRepository {
  TouristProfileRepository(this._api);

  final ApiClient _api;

  /// GET /api/tourists/me: nationality, masked passport number and whether a passport photo is on file.
  Future<TouristProfile> me() async => TouristProfile.fromJson(
    await _api.get('/api/tourists/me') as Map<String, dynamic>,
  );

  /// POST /api/tourists/me/passport-photo as multipart field "file" (JPEG/PNG, ≤ 5 MB).
  /// Returns the updated profile; a bad file is a 400 with the reason.
  Future<TouristProfile> uploadPassportPhoto(String filePath) async {
    final form = FormData.fromMap({
      'file': await MultipartFile.fromFile(filePath),
    });
    return TouristProfile.fromJson(
      await _api.postMultipart('/api/tourists/me/passport-photo', form)
          as Map<String, dynamic>,
    );
  }
}

@riverpod
TouristProfileRepository touristProfileRepository(Ref ref) =>
    TouristProfileRepository(ref.watch(apiClientProvider));
