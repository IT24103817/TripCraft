import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/friendly_error.dart';
import '../data/guide_models.dart';
import '../data/resources_repository.dart';

/// After a voucher QR is scanned: look the hotel up through the API (GET /api/hotels/{id}) and show it.
class VoucherLookup extends ConsumerStatefulWidget {
  const VoucherLookup({super.key, required this.code});

  final String? code;

  @override
  ConsumerState<VoucherLookup> createState() => _VoucherLookupState();
}

class _VoucherLookupState extends ConsumerState<VoucherLookup> {
  HotelInfo? _hotel;
  String? _error;
  bool _loading = false;

  @override
  void didUpdateWidget(VoucherLookup old) {
    super.didUpdateWidget(old);
    if (old.code != widget.code) {
      setState(() {
        _hotel = null;
        _error = null;
      });
    }
  }

  Future<void> _lookUp(String hotelId) async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final hotel = await ref.read(resourcesRepositoryProvider).hotel(hotelId);
      setState(() => _hotel = hotel);
    } catch (error) {
      setState(() => _error = friendlyMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final code = widget.code;
    final hotelId = code == null ? null : hotelIdFromVoucher(code);
    final hotel = _hotel;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Semantics(
          liveRegion: true,
          child: Text(
            code == null
                ? 'Point the camera at the voucher QR code.'
                : hotelId == null
                ? 'This QR code is not a TripCraft hotel voucher.'
                : 'Voucher scanned.',
          ),
        ),
        const SizedBox(height: 8),
        FilledButton(
          onPressed: hotelId == null || _loading
              ? null
              : () => _lookUp(hotelId),
          child: Text(_loading ? 'Looking up…' : 'Look up hotel'),
        ),
        if (_error != null)
          Text(
            _error!,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        if (hotel != null)
          Card(
            child: ListTile(
              leading: const Icon(Icons.hotel),
              title: Text('${hotel.name} · ${'★' * hotel.starRating}'),
              subtitle: Text(
                '${hotel.city}\n${hotel.roomTypes.map((r) => '${r.name} (sleeps ${r.capacity})').join(', ')}',
              ),
              isThreeLine: true,
            ),
          ),
      ],
    );
  }
}
