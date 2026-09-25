import 'package:geolocator/geolocator.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

part 'check_in.g.dart';

/// A stop the guide can check in at.
class GuideStop {
  const GuideStop({
    required this.id,
    required this.name,
    required this.latitude,
    required this.longitude,
  });

  final String id;
  final String name;
  final double latitude;
  final double longitude;
}

/// Operator rule: a guide may only check in when within 500 m of the stop.
class CheckInRule {
  const CheckInRule._();

  static const maxDistanceMeters = 500.0;

  static bool canCheckIn(double distanceMeters) =>
      distanceMeters <= maxDistanceMeters;
}

class LocationFix {
  const LocationFix(this.latitude, this.longitude);
  final double latitude;
  final double longitude;
}

/// Where the phone is. An interface so tests can place the guide anywhere.
abstract class LocationService {
  /// Asks for permission when needed. Throws [LocationUnavailable] with a friendly message on failure.
  Future<LocationFix> currentPosition();

  double distanceMeters(
    LocationFix from,
    double toLatitude,
    double toLongitude,
  );
}

class LocationUnavailable implements Exception {
  const LocationUnavailable(this.message);
  final String message;

  @override
  String toString() => message;
}

/// geolocator implementation: checks the location service, requests permission, reads a GPS fix.
class GeolocatorLocationService implements LocationService {
  @override
  Future<LocationFix> currentPosition() async {
    if (!await Geolocator.isLocationServiceEnabled()) {
      throw const LocationUnavailable('Turn on location services to check in.');
    }
    var permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
    }
    if (permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever) {
      throw const LocationUnavailable(
        'TripCraft needs your location to check you in at a stop.',
      );
    }
    final position = await Geolocator.getCurrentPosition(
      locationSettings: const LocationSettings(
        accuracy: LocationAccuracy.high,
        timeLimit: Duration(seconds: 20),
      ),
    );
    return LocationFix(position.latitude, position.longitude);
  }

  @override
  double distanceMeters(
    LocationFix from,
    double toLatitude,
    double toLongitude,
  ) => Geolocator.distanceBetween(
    from.latitude,
    from.longitude,
    toLatitude,
    toLongitude,
  );
}

@riverpod
LocationService locationService(Ref ref) => GeolocatorLocationService();
