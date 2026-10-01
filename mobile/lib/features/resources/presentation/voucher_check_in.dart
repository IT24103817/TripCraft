import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/utils/statuses.dart';
import '../data/guide_models.dart';
import '../data/resources_repository.dart';

/// After the guide scans a tourist's trip voucher ("TRIPCRAFT-VOUCHER:..."): Check in sends it to the API,
/// which checks in the next stop of today and returns the stop name and the trip's new status.
class VoucherCheckIn extends ConsumerStatefulWidget {
  const VoucherCheckIn({super.key, required this.code, this.tripId});

  /// The scanned QR text.
  final String code;

  /// The trip the scanner was opened from, if any.
  final String? tripId;

  @override
  ConsumerState<VoucherCheckIn> createState() => _VoucherCheckInState();
}

class _VoucherCheckInState extends ConsumerState<VoucherCheckIn> {
  CheckInResult? _result;
  String? _error;
  bool _saving = false;

  @override
  void didUpdateWidget(VoucherCheckIn old) {
    super.didUpdateWidget(old);
    if (old.code != widget.code) {
      setState(() {
        _result = null;
        _error = null;
      });
    }
  }

  Future<void> _checkIn() async {
    setState(() {
      _saving = true;
      _error = null;
    });
    try {
      final result = await ref
          .read(resourcesRepositoryProvider)
          .checkInWithVoucher(
            voucherCodeFromScan(widget.code),
            tripRequestId: widget.tripId,
          );
      ref.invalidate(myScheduleProvider);
      setState(() => _result = result);
    } catch (error) {
      setState(() => _error = friendlyMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final result = _result;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const Text('Trip voucher scanned.'),
        const SizedBox(height: 8),
        if (result == null)
          FilledButton.icon(
            onPressed: _saving ? null : _checkIn,
            icon: const Icon(Icons.how_to_reg),
            label: Text(
              _saving ? 'Checking in…' : 'Check in with this voucher',
            ),
          ),
        if (_error != null)
          Semantics(
            liveRegion: true,
            child: Text(
              _error!,
              style: TextStyle(color: Theme.of(context).colorScheme.error),
            ),
          ),
        if (result != null)
          Semantics(
            liveRegion: true,
            child: Card(
              child: ListTile(
                leading: const Icon(
                  Icons.check_circle,
                  color: AppColors.success,
                ),
                title: Text('Checked in at ${result.stopName}'),
                subtitle: Text(
                  'Trip is now ${statusLabel(result.tripStatus).toLowerCase()}.',
                ),
              ),
            ),
          ),
      ],
    );
  }
}
