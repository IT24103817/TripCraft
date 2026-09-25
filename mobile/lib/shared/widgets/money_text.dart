import 'package:flutter/material.dart';

import '../utils/formatters.dart';

/// An amount in LKR with its USD equivalent underneath, e.g. "LKR 187,220.00" / "USD 624.07".
class MoneyText extends StatelessWidget {
  const MoneyText({
    super.key,
    required this.lkr,
    required this.usd,
    this.emphasise = false,
    this.align = CrossAxisAlignment.end,
  });

  final num lkr;
  final num usd;
  final bool emphasise;
  final CrossAxisAlignment align;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: align,
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          formatLkr(lkr),
          style: emphasise
              ? theme.textTheme.titleMedium
              : theme.textTheme.bodyMedium,
        ),
        Text(
          formatUsd(usd),
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.outline,
          ),
        ),
      ],
    );
  }
}
