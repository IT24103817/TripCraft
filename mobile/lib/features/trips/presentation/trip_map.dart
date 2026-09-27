import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../shared/theme/app_theme.dart';
import '../data/trip_models.dart';

/// OpenStreetMap tiles (no key) with one marker per stop.
class TripMap extends StatelessWidget {
  const TripMap({super.key, required this.stops});

  final List<Attraction> stops;

  @override
  Widget build(BuildContext context) {
    final points = [for (final s in stops) LatLng(s.latitude, s.longitude)];
    return ClipRRect(
      borderRadius: BorderRadius.circular(AppRadius.control),
      child: SizedBox(
        height: 240,
        child: FlutterMap(
          options: MapOptions(
            initialCameraFit: points.length > 1
                ? CameraFit.coordinates(
                    coordinates: points,
                    padding: const EdgeInsets.all(32),
                  )
                : null,
            initialCenter: points.first,
            initialZoom: 12,
          ),
          children: [
            TileLayer(
              urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
              userAgentPackageName: 'lk.tripcraft.app',
            ),
            MarkerLayer(
              markers: [
                for (final stop in stops)
                  Marker(
                    point: LatLng(stop.latitude, stop.longitude),
                    width: 40,
                    height: 40,
                    child: Tooltip(
                      message: stop.name,
                      child: const Icon(
                        Icons.location_on,
                        color: AppColors.accent,
                        size: 36,
                      ),
                    ),
                  ),
              ],
            ),
            RichAttributionWidget(
              attributions: [
                TextSourceAttribution(
                  'OpenStreetMap contributors',
                  onTap: () => launchUrl(
                    Uri.parse('https://openstreetmap.org/copyright'),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
