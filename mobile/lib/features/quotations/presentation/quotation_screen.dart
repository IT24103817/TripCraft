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

/// The quotation for a trip: lines, subtotal, margin and total in LKR and USD, with the FX rate.
class QuotationScreen extends ConsumerWidget {
  const QuotationScreen({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final view = ref.watch(quotationViewProvider(tripId));
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
            message: 'It appears here once the agents have planned your trip.',
          ),
          data: (v) => QuotationBody(
            quotation: v.quotation!,
            workflowStatus: v.workflowStatus,
          ),
        ),
      ),
    );
  }
}

/// Separate from the screen so tests can render it with a fixed quotation.
class QuotationBody extends StatelessWidget {
  const QuotationBody({
    super.key,
    required this.quotation,
    required this.workflowStatus,
  });

  final Quotation quotation;
  final String workflowStatus;

  double _usd(double lkr) => lkr / quotation.fxRate;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        SectionCard(
          title: 'Items',
          trailing: StatusChip(status: workflowStatus),
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
        // Accepting needs POST /api/quotations/{id}/accept from Quotations (Student C), which is not merged yet.
        const FilledButton(onPressed: null, child: Text('Accept quotation')),
        const SizedBox(height: 4),
        Text(
          'Accepting will be available when the quotations service is live. '
          'Until then an operator approves your trip.',
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
