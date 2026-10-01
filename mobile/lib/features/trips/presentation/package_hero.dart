import 'package:flutter/material.dart';

import '../../../shared/theme/app_theme.dart';

/// A package's photo (assets/packages/{slug}.jpg). A package without a photo gets a calm brand-coloured
/// placeholder instead, so the layout never breaks.
class PackageHero extends StatelessWidget {
  const PackageHero({super.key, required this.slug, this.height = 140});

  final String slug;
  final double height;

  @override
  Widget build(BuildContext context) {
    return Image.asset(
      'assets/packages/$slug.jpg',
      height: height,
      width: double.infinity,
      fit: BoxFit.cover,
      // The photo is decoration: the package name next to it says what it is.
      excludeFromSemantics: true,
      errorBuilder: (_, _, _) => Container(
        key: const ValueKey('package-hero-placeholder'),
        height: height,
        width: double.infinity,
        color: AppColors.brandSoft,
        child: const Icon(
          Icons.landscape_outlined,
          size: 40,
          color: AppColors.brand,
        ),
      ),
    );
  }
}

/// The package's mood, e.g. "Cool & green", as a small brand pill.
class MoodChip extends StatelessWidget {
  const MoodChip({super.key, required this.mood});

  final String mood;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.brandSoft,
        borderRadius: BorderRadius.circular(AppRadius.pill),
      ),
      child: Text(
        mood,
        style: Theme.of(context).textTheme.labelMedium
            ?.copyWith(color: AppColors.brand, fontWeight: FontWeight.w600),
      ),
    );
  }
}
