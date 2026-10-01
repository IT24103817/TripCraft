import 'package:flutter/material.dart';

/// Validation for the new-trip form. Mirrors TripDetailsValidator / CreateTripRequestRequestValidator in the API.
class TripFormRules {
  const TripFormRules._();

  static const maxTripDays = 30;
  static const maxPax = 50;
  static const maxBudgetUsd = 1000000;
  static const maxCities = 10;

  static String? objective(String? value) {
    final text = value?.trim() ?? '';
    if (text.isEmpty) return 'Tell us what you would like to do.';
    if (text.length < 10) return 'Please write at least 10 characters.';
    if (text.length > 2000) return 'Please keep it under 2000 characters.';
    return null;
  }

  static String? dates(DateTimeRange? range, DateTime today) {
    if (range == null) return 'Choose your travel dates.';
    final start = DateUtils.dateOnly(range.start);
    if (start.isBefore(DateUtils.dateOnly(today))) {
      return 'Start date cannot be in the past.';
    }
    if (range.end.isBefore(range.start)) {
      return 'End date must be on or after the start date.';
    }
    if (DateUtils.dateOnly(range.end).difference(start).inDays + 1 >
        maxTripDays) {
      return 'Trips can be at most $maxTripDays days.';
    }
    return null;
  }

  /// Cities come from the list (so they are always valid names): 1–10 of them, and at most one per trip day.
  static String? cities(List<String> chosen, DateTimeRange? dates) {
    if (chosen.isEmpty) return 'Choose at least one city.';
    if (chosen.length > maxCities) return 'Choose at most $maxCities cities.';
    if (dates != null) {
      final days =
          DateUtils.dateOnly(dates.end)
              .difference(DateUtils.dateOnly(dates.start))
              .inDays +
          1;
      if (chosen.length > days) {
        return 'At most one city per day: your trip has $days day${days == 1 ? '' : 's'}.';
      }
    }
    return null;
  }

  static String? pax(String? value) {
    final pax = int.tryParse(value ?? '');
    if (pax == null) return 'Enter the number of travellers.';
    if (pax < 1) return 'At least 1 traveller.';
    if (pax > maxPax) return 'At most $maxPax travellers.';
    return null;
  }

  static String? budget(String? value) {
    final budget = double.tryParse(value ?? '');
    if (budget == null) return 'Enter your budget in USD.';
    if (budget <= 0) return 'Budget must be more than 0.';
    if (budget > maxBudgetUsd) return 'Budget must be at most USD 1,000,000.';
    return null;
  }

  static String? passportNumber(String? value) {
    final text = value?.trim() ?? '';
    if (text.isEmpty) return 'Passport number is required.';
    return RegExp(r'^[A-Za-z0-9 ]{6,20}$').hasMatch(text)
        ? null
        : 'Passport number must be 6–20 letters or digits.';
  }
}

/// Preference chips and the JSON they add to "preferences".
class PreferenceOption {
  const PreferenceOption(this.label, this.key, this.value);
  final String label;
  final String key;
  final Object value;
}

const preferenceOptions = [
  PreferenceOption('Hill-country train', 'transport', 'train'),
  PreferenceOption('English-speaking guide', 'language', 'en'),
  PreferenceOption('Vegetarian meals', 'diet', 'vegetarian'),
  PreferenceOption('Relaxed pace', 'pace', 'relaxed'),
  PreferenceOption('Wildlife', 'wildlife', true),
  PreferenceOption('Beaches', 'beaches', true),
];
