import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/reason_dialog.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import '../data/trips_repository.dart';

/// "Cancellation", driven by GET /api/trip-requests/{id}/cancellation: before the cut-off the tourist cancels
/// with a reason; after it (or in a status that cannot be cancelled) the app shows why and the operator contact.
class CancelTripSection extends ConsumerWidget {
  const CancelTripSection({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SectionCard(
      title: 'Cancellation',
      child: AsyncView<CancellationInfo>(
        value: ref.watch(cancellationInfoProvider(tripId)),
        onRetry: () => ref.invalidate(cancellationInfoProvider(tripId)),
        data: (info) => info.canCancel
            ? _CancelOpen(tripId: tripId, info: info)
            : CancelClosed(info: info),
      ),
    );
  }
}

class _CancelOpen extends ConsumerWidget {
  const _CancelOpen({required this.tripId, required this.info});

  final String tripId;
  final CancellationInfo info;

  Future<void> _cancel(BuildContext context, WidgetRef ref) async {
    final messenger = ScaffoldMessenger.of(context);
    final reason = await showReasonDialog(
      context,
      title: 'Cancel this trip?',
      message: 'Any bookings for it are released. Please tell us why you are cancelling.',
      confirmLabel: 'Cancel trip',
      cancelLabel: 'Keep trip',
    );
    if (reason == null) return;
    try {
      await ref.read(tripsRepositoryProvider).cancel(tripId, reason);
      ref.invalidate(myTripsProvider);
      ref.invalidate(tripDetailProvider(tripId));
      ref.invalidate(tripHistoryProvider(tripId));
      ref.invalidate(cancellationInfoProvider(tripId));
      messenger.showSnackBar(const SnackBar(content: Text('Trip cancelled.')));
    } catch (error) {
      // After the cut-off the API answers 409 with a message that names the operator contact.
      messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'You can cancel until ${formatDate(info.cancelUntil)} '
          '(${info.cutoffDays} days before the start).',
        ),
        const SizedBox(height: 8),
        OutlinedButton.icon(
          icon: const Icon(Icons.cancel_outlined, color: AppColors.danger),
          label: const Text('Cancel trip'),
          onPressed: () => _cancel(context, ref),
        ),
      ],
    );
  }
}

/// Cancelling in the app is closed: why, and how to reach the operator instead.
class CancelClosed extends StatelessWidget {
  const CancelClosed({super.key, required this.info});

  final CancellationInfo info;

  Future<void> _copy(BuildContext context) async {
    final messenger = ScaffoldMessenger.of(context);
    await Clipboard.setData(ClipboardData(text: info.operatorContact));
    messenger.showSnackBar(
      SnackBar(content: Text('Copied ${info.operatorContact}')),
    );
  }

  /// Opens the mail or phone app; when that is not possible the contact is copied instead.
  Future<void> _contact(BuildContext context) async {
    final uri = operatorContactUri(info.operatorContact);
    var opened = false;
    if (uri != null) {
      try {
        opened = await launchUrl(uri);
      } catch (_) {
        opened = false;
      }
    }
    if (!opened && context.mounted) await _copy(context);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          info.closedReason ??
              'This trip can no longer be cancelled in the app.',
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            const Icon(Icons.support_agent, color: AppColors.muted),
            const SizedBox(width: 8),
            Expanded(child: SelectableText(info.operatorContact)),
            IconButton(
              tooltip: 'Copy operator contact',
              icon: const Icon(Icons.copy),
              onPressed: () => _copy(context),
            ),
          ],
        ),
        OutlinedButton.icon(
          icon: const Icon(Icons.mail_outline),
          label: const Text('Contact operator'),
          onPressed: () => _contact(context),
        ),
      ],
    );
  }
}

/// mailto: for an email address, tel: for a phone number, null when the contact is neither.
Uri? operatorContactUri(String contact) {
  final email = RegExp(r'[^\s@/,;]+@[^\s@/,;]+').firstMatch(contact);
  if (email != null) return Uri(scheme: 'mailto', path: email[0]);
  final phone = RegExp(r'\+?[0-9][0-9 \-]{5,}[0-9]').firstMatch(contact);
  if (phone != null) {
    return Uri(scheme: 'tel', path: phone[0]!.replaceAll(RegExp(r'[ \-]'), ''));
  }
  return null;
}
