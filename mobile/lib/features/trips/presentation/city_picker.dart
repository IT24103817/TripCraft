import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/friendly_error.dart';
import '../application/trips_providers.dart';

/// Multi-select of the cities from GET /api/attractions/cities. [selected] keeps the order the tourist
/// tapped them in, which is the order the trip visits them. Free text is not possible (the API rejects it).
class CityPicker extends ConsumerWidget {
  const CityPicker({
    super.key,
    required this.selected,
    required this.onChanged,
    this.errorText,
  });

  final List<String> selected;
  final ValueChanged<List<String>> onChanged;
  final String? errorText;

  void _toggle(String city) {
    final next = [...selected];
    if (next.contains(city)) {
      next.remove(city);
    } else {
      next.add(city);
    }
    onChanged(next);
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final cities = ref.watch(tripCitiesProvider);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Cities to visit', style: theme.textTheme.titleSmall),
        const SizedBox(height: 4),
        Text(
          'Tap them in the order you want to visit them.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: 8),
        cities.when(
          skipLoadingOnRefresh: true,
          loading: () => const Padding(
            padding: EdgeInsets.all(8),
            child: SizedBox(
              width: 20,
              height: 20,
              child: CircularProgressIndicator(
                strokeWidth: 2,
                semanticsLabel: 'Loading cities',
              ),
            ),
          ),
          error: (error, _) => Row(
            children: [
              Expanded(child: Text(friendlyMessage(error))),
              TextButton(
                onPressed: () => ref.invalidate(tripCitiesProvider),
                child: const Text('Retry'),
              ),
            ],
          ),
          data: (all) => Wrap(
            spacing: 8,
            runSpacing: 4,
            children: [
              for (final city in all)
                FilterChip(
                  // A chosen city shows its place in the route, e.g. "2. Ella".
                  label: Text(
                    selected.contains(city)
                        ? '${selected.indexOf(city) + 1}. $city'
                        : city,
                  ),
                  selected: selected.contains(city),
                  onSelected: (_) => _toggle(city),
                ),
            ],
          ),
        ),
        if (selected.isNotEmpty) ...[
          const SizedBox(height: 4),
          Text('Route: ${selected.join(' → ')}'),
        ],
        if (errorText != null) ...[
          const SizedBox(height: 4),
          Text(
            errorText!,
            style: theme.textTheme.bodySmall?.copyWith(
              color: theme.colorScheme.error,
            ),
          ),
        ],
      ],
    );
  }
}
