import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/money_text.dart';
import '../../../shared/widgets/reason_dialog.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../../../shared/utils/friendly_error.dart';
import '../application/quotation_providers.dart';
import '../data/quotations_repository.dart';
import '../data/quotation_models.dart';

/// The quotation for a trip: lines, subtotal, margin and total in LKR and USD, with the FX rate.
/// While the trip is QuotationSent the tourist can accept it, or decline it with a reason.
class QuotationScreen extends ConsumerWidget {
  const QuotationScreen({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final view = ref.watch(quotationViewProvider(tripId));

    /// Runs accept or decline, shows [success] (or the API's message), and reloads the quotation.
    Future<void> decide(
      Future<QuotationDecision> Function() call,
      String success,
    ) async {
      final messenger = ScaffoldMessenger.of(context);
      try {
        await call();
        messenger.showSnackBar(SnackBar(content: Text(success)));
      } catch (error) {
        messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
      }
      if (context.mounted) ref.invalidate(quotationViewProvider(tripId));
    }

    Future<void> accept(String quotationId) => decide(
      () => ref.read(quotationsRepositoryProvider).accept(quotationId),
      'Quotation accepted. The operator will now confirm your trip.',
    );

    Future<void> decline(String quotationId) async {
      final reason = await showReasonDialog(
        context,
        title: 'Decline this quotation?',
        message: 'Tell the operator what you would like changed. They will send you a new version.',
        confirmLabel: 'Decline',
      );
      if (reason == null) return;
      await decide(
        () =>
            ref.read(quotationsRepositoryProvider).decline(quotationId, reason),
        'Quotation declined. The operator will prepare a new version.',
      );
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Quotation')),
      body: RefreshIndicator(
        onRefresh: () => ref.refresh(quotationViewProvider(tripId).future),
        child: AsyncView<QuotationView>(
          value: view,
          onRetry: () => ref.invalidate(quotationViewProvider(tripId)),
          isEmpty: (v) => v.quotation == null,
          empty: const EmptyState(
            icon: Icons.receipt_long_outlined,
            title: 'No quotation yet',
            message: 'It appears here once the operator has sent it to you.',
          ),
          data: (v) {
            final id = v.quotationId;
            final canDecide = id != null && v.tripStatus == 'QuotationSent';
            return QuotationBody(
              quotation: v.quotation!,
              tripStatus: v.tripStatus,
              quotationStatus: v.quotationStatus,
              acceptedAt: v.acceptedAt,
              onAccept: canDecide ? () => accept(id) : null,
              onDecline: canDecide ? () => decline(id) : null,
            );
          },
        ),
      ),
    );
  }
}

/// Separate from the screen so tests can render it with a fixed quotation.
/// Accept and Decline are enabled only when [onAccept] / [onDecline] are given (trip status QuotationSent).
class QuotationBody extends StatelessWidget {
  const QuotationBody({
    super.key,
    required this.quotation,
    required this.tripStatus,
    this.quotationStatus,
    this.acceptedAt,
    this.onAccept,
    this.onDecline,
  });

  final Quotation quotation;
  final String tripStatus;

  /// Status of the stored quotation (Pending, Approved = sent, Declined, ...); null while it only exists in
  /// the proposal.
  final String? quotationStatus;
  final String? acceptedAt;
  final VoidCallback? onAccept;
  final VoidCallback? onDecline;

  /// The sentence under the buttons: what the tourist can do now.
  String get _hint {
    if (acceptedAt != null) {
      return 'You accepted this price on ${formatDateTime(acceptedAt)}.';
    }
    if (quotationStatus == 'Declined') {
      return 'You declined this version. The operator is preparing a new one.';
    }
    if (tripStatus == 'QuotationSent') {
      return 'Accept to go ahead, or decline and tell the operator what to change.';
    }
    return 'An operator is checking this quotation. You can accept or decline it once it is sent to you.';
  }

  double _usd(double lkr) => lkr / quotation.fxRate;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        SectionCard(
          title: 'Items',
          trailing: StatusChip(status: tripStatus),
          child: Column(
            children: [
              for (final line in quotation.lines)
                Padding(
                  padding: const EdgeInsets.only(bottom: 10),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(line.description),
                            Text(
                              '${_qty(line.qty)} × ${formatLkr(line.unitLkr)}',
                              style: theme.textTheme.bodySmall,
                            ),
                          ],
                        ),
                      ),
                      MoneyText(lkr: line.amountLkr, usd: _usd(line.amountLkr)),
                    ],
                  ),
                ),
              const Divider(),
              _TotalRow(
                label: 'Subtotal',
                lkr: quotation.subtotalLkr,
                usd: _usd(quotation.subtotalLkr),
              ),
              _TotalRow(
                label: 'Service margin (${_qty(quotation.marginPct)}%)',
                lkr: quotation.marginLkr,
                usd: _usd(quotation.marginLkr),
              ),
              _TotalRow(
                label: 'Total',
                lkr: quotation.totalLkr,
                usd: quotation.totalUsd,
                emphasise: true,
              ),
            ],
          ),
        ),
        const SizedBox(height: 12),
        Text(
          '1 USD = ${quotation.fxRate.toStringAsFixed(2)} LKR · as of ${formatDateTime(quotation.fxAsOf)}',
          style: theme.textTheme.bodySmall,
        ),
        if (quotation.fxStale)
          const Text(
            'The exchange rate may be out of date.',
            style: TextStyle(color: AppColors.warning),
          ),
        const SizedBox(height: 16),
        // POST /api/quotations/{id}/accept or /decline: only while the trip is QuotationSent.
        FilledButton(
          onPressed: onAccept,
          child: const Text('Accept quotation'),
        ),
        const SizedBox(height: 8),
        OutlinedButton(onPressed: onDecline, child: const Text('Decline')),
        const SizedBox(height: 4),
        Text(
          _hint,
          style: theme.textTheme.bodySmall,
          textAlign: TextAlign.center,
        ),
      ],
    );
  }

  static String _qty(double value) => value == value.roundToDouble()
      ? value.toStringAsFixed(0)
      : value.toString();
}

class _TotalRow extends StatelessWidget {
  const _TotalRow({
    required this.label,
    required this.lkr,
    required this.usd,
    this.emphasise = false,
  });

  final String label;
  final double lkr;
  final double usd;
  final bool emphasise;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Expanded(
            child: Text(
              label,
              style: emphasise ? Theme.of(context).textTheme.titleMedium : null,
            ),
          ),
          MoneyText(lkr: lkr, usd: usd, emphasise: emphasise),
        ],
      ),
    );
  }
}
