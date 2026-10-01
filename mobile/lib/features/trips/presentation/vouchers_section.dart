import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';

/// "Vouchers" of a confirmed trip (GET /api/trips/{id}/vouchers): the trip voucher first (the guide scans it at
/// each stop), then one voucher per hotel night. Each is shown as a QR code of its signed payload.
class VouchersSection extends ConsumerWidget {
  const VouchersSection({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SectionCard(
      title: 'Vouchers',
      child: AsyncView<List<TripVoucher>>(
        value: ref.watch(tripVouchersProvider(tripId)),
        onRetry: () => ref.invalidate(tripVouchersProvider(tripId)),
        isEmpty: (vouchers) => vouchers.isEmpty,
        empty: const Text(
          'Your vouchers appear here once the trip is confirmed.',
        ),
        data: (vouchers) => Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            for (final voucher in vouchers) VoucherCard(voucher: voucher),
          ],
        ),
      ),
    );
  }
}

/// One voucher: what it is for, its QR code and the code as text (for reading out over the phone).
class VoucherCard extends StatelessWidget {
  const VoucherCard({super.key, required this.voucher});

  final TripVoucher voucher;

  bool get _isTrip => voucher.type == 'Trip';

  String get _title =>
      _isTrip ? 'Trip voucher' : (voucher.hotelName ?? 'Hotel voucher');

  String get _subtitle => _isTrip
      ? 'Show this to your guide at each stop.'
      : 'Night of ${formatDate(voucher.night)} · '
            '${voucher.rooms} room${voucher.rooms == 1 ? '' : 's'}';

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Icon(
                _isTrip ? Icons.confirmation_number_outlined : Icons.hotel,
                color: AppColors.brand,
              ),
              const SizedBox(width: 8),
              Expanded(child: Text(_title, style: theme.textTheme.titleSmall)),
            ],
          ),
          Text(_subtitle, style: theme.textTheme.bodySmall),
          const SizedBox(height: 8),
          Center(
            child: QrImageView(
              // The key carries the payload, so tests can check what the QR code holds.
              key: ValueKey('qr-${voucher.qrPayload}'),
              data: voucher.qrPayload,
              size: 200,
              backgroundColor: Colors.white,
              semanticsLabel: 'QR code for $_title',
            ),
          ),
          SelectableText(
            voucher.code,
            textAlign: TextAlign.center,
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}
