import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/money_text.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/quotation_providers.dart';
import '../data/quotation_models.dart';
import 'quotation_actions.dart';

/// On the trip's Overview at QuotationSent: the total, Accept, Decline (with a reason) and a link to the
/// full quotation. [onDecided] reloads the trip once the tourist has decided.
class QuotationDecisionCard extends ConsumerWidget {
  const QuotationDecisionCard({
    super.key,
    required this.tripId,
    required this.onDecided,
  });

  final String tripId;
  final VoidCallback onDecided;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    Future<void> after(bool decided) async {
      if (!decided || !context.mounted) return;
      ref.invalidate(quotationViewProvider(tripId));
      onDecided();
    }

    return SectionCard(
      title: 'Your quotation is ready',
      child: AsyncView<QuotationView>(
        value: ref.watch(quotationViewProvider(tripId)),
        onRetry: () => ref.invalidate(quotationViewProvider(tripId)),
        isEmpty: (v) => v.quotation == null,
        empty: const Text('The quotation is on its way.'),
        data: (v) {
          final id = v.quotationId;
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      'Total',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                  MoneyText(
                    lkr: v.quotation!.totalLkr,
                    usd: v.quotation!.totalUsd,
                    emphasise: true,
                  ),
                ],
              ),
              const SizedBox(height: 8),
              const Text(
                'Accept to go ahead, or decline and tell the operator what to change.',
              ),
              const SizedBox(height: 12),
              FilledButton(
                onPressed: id == null
                    ? null
                    : () async =>
                          after(await acceptQuotation(context, ref, id)),
                child: const Text('Accept quotation'),
              ),
              const SizedBox(height: 8),
              OutlinedButton(
                onPressed: id == null
                    ? null
                    : () async =>
                          after(await declineQuotation(context, ref, id)),
                child: const Text('Decline'),
              ),
              TextButton(
                onPressed: () => context.push(Routes.quotation(tripId)),
                child: const Text('See the full quotation'),
              ),
            ],
          );
        },
      ),
    );
  }
}
