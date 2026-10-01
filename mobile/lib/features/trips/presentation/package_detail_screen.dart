import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/primary_button.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/template_providers.dart';
import '../data/template_models.dart';
import '../data/trip_models.dart';
import 'book_package_sheet.dart';
import 'package_hero.dart';
import 'trip_map.dart';
import 'trip_prefill.dart';

/// One mood package: photo, summary, the day-by-day plan with a map, and three ways on:
/// "Book as is", "Customize with the planner" (the new-trip form filled in) and "Back".
class PackageDetailScreen extends ConsumerWidget {
  const PackageDetailScreen({super.key, required this.templateId});

  final String templateId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final template = ref.watch(tripTemplateProvider(templateId));
    return Scaffold(
      appBar: AppBar(title: Text(template.value?.name ?? 'Package')),
      body: AsyncView<TripTemplate>(
        value: template,
        onRetry: () => ref.invalidate(tripTemplateProvider(templateId)),
        data: (t) => _PackageBody(template: t),
      ),
    );
  }
}

class _PackageBody extends StatelessWidget {
  const _PackageBody({required this.template});

  final TripTemplate template;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final days = template.itinerary ?? const <TemplateDay>[];
    final mapStops = packageMapStops(days);
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        ClipRRect(
          borderRadius: BorderRadius.circular(AppRadius.card),
          child: PackageHero(slug: template.slug, height: 200),
        ),
        const SizedBox(height: 16),
        Align(
          alignment: Alignment.centerLeft,
          child: MoodChip(mood: template.moodTag),
        ),
        const SizedBox(height: 8),
        Text(template.name, style: theme.textTheme.headlineSmall),
        const SizedBox(height: 4),
        Text(template.summary),
        const SizedBox(height: 8),
        Text(
          '${template.days} days · ${template.cities.join(' → ')}',
          style: theme.textTheme.bodyMedium?.copyWith(color: AppColors.muted),
        ),
        Text(
          'From ${formatUsdWhole(template.fromPriceUsd)} for ${template.pricedForPax} travellers',
          style: theme.textTheme.titleMedium?.copyWith(color: AppColors.brand),
        ),
        const SizedBox(height: 16),
        SectionCard(
          title: 'Day by day',
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              for (final day in days) _PackageDayTile(day: day),
              if (mapStops.isNotEmpty) ...[
                const SizedBox(height: 8),
                TripMap(stops: mapStops),
              ],
            ],
          ),
        ),
        const SizedBox(height: 24),
        PrimaryButton(
          label: 'Book as is',
          icon: Icons.check_circle_outline,
          onPressed: () => showBookPackageSheet(context, template),
        ),
        const SizedBox(height: 8),
        OutlinedButton.icon(
          icon: const Icon(Icons.tune),
          label: const Text('Customize with the planner'),
          onPressed: () => context.push(
            Routes.newTrip,
            extra: TripPrefill.fromTemplate(template),
          ),
        ),
        const SizedBox(height: 8),
        TextButton(
          // Opened from a link there may be nothing to go back to: then go home.
          onPressed: () =>
              context.canPop() ? context.pop() : context.go(Routes.home),
          child: const Text('Back'),
        ),
      ],
    );
  }
}

class _PackageDayTile extends StatelessWidget {
  const _PackageDayTile({required this.day});

  final TemplateDay day;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Day ${day.day} — ${day.city}',
            style: Theme.of(context).textTheme.titleSmall,
          ),
          for (final stop in day.stops)
            Padding(
              padding: const EdgeInsets.only(left: 8, top: 2),
              child: Text('• ${stop.name}'),
            ),
        ],
      ),
    );
  }
}

/// The package's stops that have a position, in visiting order, for the map's markers and route line.
List<Attraction> packageMapStops(List<TemplateDay> days) => [
  for (final day in days)
    for (final stop in day.stops)
      if (stop.latitude != null && stop.longitude != null)
        Attraction(
          id: stop.attractionId ?? stop.name,
          name: stop.name,
          city: day.city,
          latitude: stop.latitude!,
          longitude: stop.longitude!,
        ),
];
