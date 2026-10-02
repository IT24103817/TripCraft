import 'package:flutter/material.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../data/quotation_models.dart';

/// The budget sentence for a quote that is over the tourist's budget even after the agents' cheapest plan,
/// e.g. "Best price we can offer — USD 120.00 above your budget". Null when the quote is within budget.
/// The API sends the sentence as `budgetNote`; it is built from `overBudgetUsd` only if that is missing.
String? bestPriceNote(Quotation quotation) {
  if (!quotation.bestAvailablePrice) return null;
  final note = quotation.budgetNote;
  if (note != null && note.trim().isNotEmpty) return note;
  final over = quotation.overBudgetUsd;
  return over == null
      ? 'Best price we can offer'
      : 'Best price we can offer — ${formatUsd(over)} above your budget';
}

/// "Updated quote (version 2)" once the operator has edited and resent the quote; null for the first version.
String? updatedQuoteLabel(int? version) =>
    version != null && version > 1 ? 'Updated quote (version $version)' : null;

/// The budget sentence in a warning box, so it is not missed next to the total. Shows nothing within budget.
class BestPriceNote extends StatelessWidget {
  const BestPriceNote({super.key, required this.quotation});

  final Quotation quotation;

  @override
  Widget build(BuildContext context) {
    final note = bestPriceNote(quotation);
    if (note == null) return const SizedBox.shrink();
    return Container(
      key: const ValueKey('best-price-note'),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: AppColors.warning.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(AppRadius.control),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.info_outline, color: AppColors.warning, size: 20),
          const SizedBox(width: 8),
          Expanded(child: Text(note)),
        ],
      ),
    );
  }
}

/// "Updated quote (version N)" above the total when the operator resent a new version. Shows nothing for version 1.
class UpdatedQuoteLabel extends StatelessWidget {
  const UpdatedQuoteLabel({super.key, required this.version});

  final int? version;

  @override
  Widget build(BuildContext context) {
    final label = updatedQuoteLabel(version);
    if (label == null) return const SizedBox.shrink();
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Text(
        label,
        key: const ValueKey('updated-quote-label'),
        style: Theme.of(context).textTheme.titleSmall
            ?.copyWith(color: AppColors.brand, fontWeight: FontWeight.w600),
      ),
    );
  }
}
