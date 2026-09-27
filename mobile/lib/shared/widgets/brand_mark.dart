import 'package:flutter/material.dart';

import '../theme/app_theme.dart';

/// The TripCraft logo: a brand square with a compass and the wordmark (same as web/src/shared/components/Logo.tsx).
class BrandMark extends StatelessWidget {
  const BrandMark({super.key, this.size = 40});

  final double size;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        ExcludeSemantics(
          child: Container(
            width: size,
            height: size,
            decoration: BoxDecoration(
              color: AppColors.brand,
              borderRadius: BorderRadius.circular(AppRadius.control),
            ),
            child: Icon(
              Icons.explore_outlined,
              color: Colors.white,
              size: size * 0.6,
            ),
          ),
        ),
        const SizedBox(width: 12),
        Text(
          'TripCraft',
          style: Theme.of(context).textTheme.headlineMedium?.copyWith(
            fontWeight: FontWeight.w700,
            color: AppColors.brandDeep,
          ),
        ),
      ],
    );
  }
}
