import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../application/template_providers.dart';
import '../data/template_models.dart';
import 'package_hero.dart';

/// "Trips picked for your mood": the packages as a row of cards that scrolls sideways.
class PackagesRow extends ConsumerWidget {
  const PackagesRow({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SizedBox(
      height: 290,
      child: AsyncView<List<TripTemplate>>(
        value: ref.watch(tripTemplatesProvider),
        onRetry: () => ref.invalidate(tripTemplatesProvider),
        isEmpty: (templates) => templates.isEmpty,
        empty: const EmptyState(
          icon: Icons.travel_explore,
          title: 'No packages right now',
          message: 'Plan your own trip instead.',
        ),
        data: (templates) => ListView.separated(
          scrollDirection: Axis.horizontal,
          itemCount: templates.length,
          separatorBuilder: (_, _) => const SizedBox(width: 12),
          itemBuilder: (_, i) => PackageCard(template: templates[i]),
        ),
      ),
    );
  }
}

/// One package: photo, mood, name, "4 days · Kandy, Ella" and the from-price for two.
class PackageCard extends StatelessWidget {
  const PackageCard({super.key, required this.template});

  final TripTemplate template;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return SizedBox(
      width: 260,
      child: Card(
        clipBehavior: Clip.antiAlias,
        child: InkWell(
          onTap: () => context.push(Routes.package(template.id)),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              PackageHero(slug: template.slug),
              Padding(
                padding: const EdgeInsets.all(12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    MoodChip(mood: template.moodTag),
                    const SizedBox(height: 8),
                    Text(
                      template.name,
                      style: theme.textTheme.titleMedium,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    Text(
                      '${template.days} days · ${template.cities.join(', ')}',
                      style: theme.textTheme.bodySmall,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'From ${formatUsdWhole(template.fromPriceUsd)} for ${template.pricedForPax}',
                      style: theme.textTheme.titleSmall?.copyWith(
                        color: AppColors.brand,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
