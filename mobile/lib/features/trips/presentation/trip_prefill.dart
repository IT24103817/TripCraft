import '../data/template_models.dart';
import 'trip_form_rules.dart';

/// What "Customize with the planner" puts into the new-trip form from a package. The tourist can change
/// everything before submitting; the agents then plan the edited request as usual.
class TripPrefill {
  const TripPrefill({
    required this.objective,
    required this.cities,
    required this.preferences,
    required this.days,
    this.budgetUsd,
  });

  final String objective;

  /// In the order the package visits them.
  final List<String> cities;

  /// The form's preference chips that match the package's preferences.
  final Set<PreferenceOption> preferences;

  /// The package length: once a start date is picked, the end date is start + days - 1.
  final int days;
  final double? budgetUsd;

  factory TripPrefill.fromTemplate(TripTemplate template) => TripPrefill(
    objective: template.objective,
    cities: template.cities,
    preferences: {
      for (final option in preferenceOptions)
        if (template.preferences[option.key] == option.value) option,
    },
    days: template.days,
    budgetUsd: suggestedBudgetUsd(template, template.pricedForPax),
  );
}

/// A sensible budget for [pax] travellers: the package's from-price (for 2) scaled to [pax], rounded up.
double suggestedBudgetUsd(TripTemplate template, int pax) {
  final perPerson = template.fromPriceUsd / template.pricedForPax;
  return (perPerson * pax).ceilToDouble();
}
