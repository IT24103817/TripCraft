import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/money_text.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../application/quotation_providers.dart';
import '../data/quotation_models.dart';
import 'quotation_actions.dart';
import 'quote_notes.dart';

/// The quotation for a trip: lines, subtotal, margin and total in LKR and USD, with the FX rate, and the budget
/// note when it is the best price we can offer. While the trip is QuotationSent the tourist can accept it, or
/// decline it with a reason (asked in a bottom sheet). A resent quote says "Updated quote (version N)".
class QuotationScreen extends ConsumerWidget {
  const QuotationScreen({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final view = ref.watch(quotationViewProvider(tripId));

    /// Runs accept or decline (they show their own snackbar), then reloads the quotation.
    Future<void> decide(Future<bool> Function() call) async {
      await call();
      if (context.mounted) ref.invalidate(quotationViewProvider(tripId));
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
            message: 'It appears here as soon as our planner agents have priced your trip.',
          ),
          data: (v) {
            final id = v.quotationId;
            final canDecide = id != null && v.tripStatus == 'QuotationSent';
            return QuotationBody(
              quotation: v.quotation!,
              tripStatus: v.tripStatus,
              version: v.version,
              quotationStatus: v.quotationStatus,
              acceptedAt: v.acceptedAt,
              onAccept: canDecide
                  ? () => decide(() => acceptQuotation(context, ref, id))
                  : null,
              onDecline: canDecide
                  ? () => decide(() => declineQuotation(context, ref, id))
                  : null,
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
    this.version,
    this.quotationStatus,
    this.acceptedAt,
    this.onAccept,
    this.onDecline,
  });

  final Quotation quotation;
  final String tripStatus;

  /// 2 or more once the operator edited and resent the quote; null while it only exists in the proposal.
  final int? version;

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
      return 'You declined this version. The operator will replan or contact you.';
    }
    if (tripStatus == 'QuotationSent') {
      return 'Accept to go ahead, or decline and tell the operator what to change.';
    }
    return 'You can accept or decline a quote while it is waiting for your answer.';
  }

  double _usd(double lkr) => lkr / quotation.fxRate;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        UpdatedQuoteLabel(version: version),
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
        if (quotation.bestAvailablePrice) ...[
          const SizedBox(height: 12),
          BestPriceNote(quotation: quotation),
        ],
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
