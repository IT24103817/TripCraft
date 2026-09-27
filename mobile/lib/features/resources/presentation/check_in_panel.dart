import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/utils/statuses.dart';
import '../../../shared/widgets/primary_button.dart';
import '../data/check_in.dart';
import '../data/resources_repository.dart';

/// Locate the guide, show the distance to the stop, and allow Check in only within 500 m.
/// The API checks the same rule again and records the check-in (POST /api/check-ins).
class CheckInPanel extends ConsumerStatefulWidget {
  const CheckInPanel({super.key, required this.stop});

  final GuideStop stop;

  @override
  ConsumerState<CheckInPanel> createState() => _CheckInPanelState();
}

class _CheckInPanelState extends ConsumerState<CheckInPanel> {
  double? _distance;
  LocationFix? _fix;
  String? _error;
  String? _checkedInAt;
  bool _locating = false;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _checkedInAt = widget.stop.checkedInAt;
  }

  Future<void> _locate() async {
    setState(() {
      _locating = true;
      _error = null;
    });
    final location = ref.read(locationServiceProvider);
    try {
      final fix = await location.currentPosition();
      setState(() {
        _fix = fix;
        _distance = location.distanceMeters(
          fix,
          widget.stop.latitude,
          widget.stop.longitude,
        );
      });
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

  Future<void> _checkIn() async {
    final fix = _fix;
    if (fix == null) return;
    final messenger = ScaffoldMessenger.of(context);
    setState(() => _saving = true);
    try {
      final result = await ref
          .read(resourcesRepositoryProvider)
          .checkIn(widget.stop.id, fix.latitude, fix.longitude);
      ref.invalidate(myScheduleProvider);
      setState(() => _checkedInAt = result.checkedInAt);
      messenger.showSnackBar(
        SnackBar(
          content: Text(
            'Checked in at ${widget.stop.name}. Trip is ${statusLabel(result.tripStatus).toLowerCase()}.',
          ),
        ),
      );
    } catch (error) {
      messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final checkedInAt = _checkedInAt;
    if (checkedInAt != null) {
      return ListTile(
        contentPadding: EdgeInsets.zero,
        leading: const Icon(Icons.check_circle, color: AppColors.success),
        title: Text('Checked in ${formatDateTime(checkedInAt)}'),
      );
    }
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
        PrimaryButton(
          label: _saving ? 'Checking in…' : 'Check in',
          onPressed: allowed && !_saving ? _checkIn : null,
        ),
      ],
    );
  }
}
