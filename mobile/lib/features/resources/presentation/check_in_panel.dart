import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/widgets/primary_button.dart';
import '../data/check_in.dart';

/// Locate the guide, show the distance to the stop, and allow Check in only within 500 m.
class CheckInPanel extends ConsumerStatefulWidget {
  const CheckInPanel({super.key, required this.stop});

  final GuideStop stop;

  @override
  ConsumerState<CheckInPanel> createState() => _CheckInPanelState();
}

class _CheckInPanelState extends ConsumerState<CheckInPanel> {
  double? _distance;
  String? _error;
  bool _locating = false;

  Future<void> _locate() async {
    setState(() {
      _locating = true;
      _error = null;
    });
    final location = ref.read(locationServiceProvider);
    try {
      final fix = await location.currentPosition();
      setState(
        () => _distance = location.distanceMeters(
          fix,
          widget.stop.latitude,
          widget.stop.longitude,
        ),
      );
    } on LocationUnavailable catch (e) {
      setState(() => _error = e.message);
    } catch (_) {
      setState(
        () => _error = 'Could not get your location. Try again outside.',
      );
    } finally {
      if (mounted) setState(() => _locating = false);
    }
  }

  void _checkIn() {
    // The check-in endpoint belongs to Resource Management (Student B) and is not merged yet.
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text(
          'You are close enough. Check-in will be saved once the schedule service is live.',
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final distance = _distance;
    final allowed = distance != null && CheckInRule.canCheckIn(distance);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        OutlinedButton.icon(
          onPressed: _locating ? null : _locate,
          icon: const Icon(Icons.my_location),
          label: Text(_locating ? 'Locating…' : 'Find my location'),
        ),
        const SizedBox(height: 8),
        if (distance != null)
          Semantics(
            liveRegion: true,
            child: Text(
              allowed
                  ? 'You are ${distance.round()} m from ${widget.stop.name}.'
                  : 'You are ${distance.round()} m from ${widget.stop.name}. Get within '
                        '${CheckInRule.maxDistanceMeters.round()} m to check in.',
            ),
          ),
        if (_error != null)
          Text(
            _error!,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        const SizedBox(height: 8),
        PrimaryButton(label: 'Check in', onPressed: allowed ? _checkIn : null),
      ],
    );
  }
}
