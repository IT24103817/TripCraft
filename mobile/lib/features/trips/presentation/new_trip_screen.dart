import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';

import '../../../core/api/user_facing_exception.dart';
import '../../../core/auth/auth_repository.dart';
import '../../../core/auth/profile_button.dart';
import '../../../core/router/routes.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/app_text_field.dart';
import '../../../shared/widgets/primary_button.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import '../data/trips_repository.dart';
import 'city_picker.dart';
import 'travellers_field.dart';
import 'trip_form_rules.dart';
import 'trip_prefill.dart';

/// PLAN.md section 6, step 1: the tourist describes the trip, picks dates, the cities to visit (from the
/// API's list), travellers, budget and preferences, adds a passport photo, and submits. Then planning starts.
/// Opened from a package ("Customize with the planner") the form starts filled in with [prefill].
class NewTripScreen extends ConsumerStatefulWidget {
  const NewTripScreen({super.key, this.imagePicker, this.today, this.prefill});

  final ImagePicker? imagePicker;

  /// The package the tourist is customising, if any.
  final TripPrefill? prefill;

  /// Injected by tests so "in the past" is stable.
  final DateTime? today;

  @override
  ConsumerState<NewTripScreen> createState() => _NewTripScreenState();
}

class _NewTripScreenState extends ConsumerState<NewTripScreen> {
  final _form = GlobalKey<FormState>();
  final _objective = TextEditingController();
  final _pax = TextEditingController(text: '2');
  final _budget = TextEditingController();
  final _nationality = TextEditingController();
  final _passport = TextEditingController();
  final Set<PreferenceOption> _preferences = {};

  /// Chosen cities in the order they were tapped.
  List<String> _cities = [];

  /// A 400 from the API about the cities (e.g. "'X' is not a city we cover. Supported cities: ...").
  String? _citiesServerError;
  DateTimeRange? _dates;
  XFile? _photo;
  bool _photoMissing = false;
  bool _submitting = false;

  DateTime get _today => widget.today ?? DateTime.now();

  @override
  void initState() {
    super.initState();
    final prefill = widget.prefill;
    if (prefill != null) {
      _objective.text = prefill.objective;
      _cities = [...prefill.cities];
      _preferences.addAll(prefill.preferences);
      if (prefill.budgetUsd != null) {
        _budget.text = prefill.budgetUsd!.toStringAsFixed(0);
      }
    }
    // Pre-fill the nationality given at registration.
    ref.read(authRepositoryProvider).savedNationality().then((n) {
      if (mounted && n != null && _nationality.text.isEmpty) {
        _nationality.text = n;
      }
    });
  }

  @override
  void dispose() {
    for (final c in [_objective, _pax, _budget, _nationality, _passport]) {
      c.dispose();
    }
    super.dispose();
  }

  /// From a package, the first tap picks only the start date and the end date follows from the package length.
  /// Without a package (or to change the dates later) the tourist picks the whole range.
  Future<DateTimeRange?> _pickDates() async {
    final days = widget.prefill?.days;
    if (days != null && _dates == null) {
      final start = await showDatePicker(
        context: context,
        helpText: 'Start date ($days-day package)',
        firstDate: _today,
        lastDate: _today.add(const Duration(days: 365)),
        initialDate: _today,
      );
      if (start == null) return null;
      return DateTimeRange(
        start: start,
        end: start.add(Duration(days: days - 1)),
      );
    }
    return showDateRangePicker(
      context: context,
      firstDate: _today,
      lastDate: _today.add(const Duration(days: 365)),
      initialDateRange: _dates,
    );
  }

  Future<void> _pickPhoto(ImageSource source) async {
    final picker = widget.imagePicker ?? ImagePicker();
    final photo = await picker.pickImage(
      source: source,
      maxWidth: 1600,
      imageQuality: 85,
    );
    if (photo == null) return;
    setState(() {
      _photo = photo;
      _photoMissing = false;
    });
  }

  Future<void> _submit() async {
    final formValid = _form.currentState!.validate();
    setState(() => _photoMissing = _photo == null);
    if (!formValid || _photo == null) return;

    setState(() => _submitting = true);
    final repository = ref.read(tripsRepositoryProvider);
    final messenger = ScaffoldMessenger.of(context);
    try {
      final trip = await repository.create(
        CreateTripRequest(
          objective: _objective.text.trim(),
          startDate: toApiDate(_dates!.start),
          endDate: toApiDate(_dates!.end),
          pax: int.parse(_pax.text),
          budgetUsd: double.parse(_budget.text),
          preferences: {for (final p in _preferences) p.key: p.value},
          nationality: _nationality.text.trim(),
          passportNumber: _passport.text.trim(),
          cities: _cities,
        ),
      );
      await repository.uploadPassportPhoto(trip.id, _photo!.path);
      final planning = await repository.startPlanning(trip.id);
      ref.invalidate(myTripsProvider);
      messenger.showSnackBar(
        SnackBar(
          content: Text(
            planning.workflowStatus == 'FailedSafely'
                ? 'Trip saved, but planning could not start: ${planning.errorSummary ?? 'please try again later'}'
                : 'Trip submitted. Our agents are planning it now.',
          ),
        ),
      );
      if (mounted) context.go(Routes.trip(trip.id));
    } catch (error) {
      if (error is UserFacingException && error.statusCode == 400) {
        setState(() => _citiesServerError = _citiesError(error));
      }
      messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  /// The API's message about the "cities" field of a 400, if there is one.
  static String? _citiesError(UserFacingException error) {
    for (final entry in error.fieldErrors.entries) {
      if (entry.key.toLowerCase().startsWith('cities')) {
        return entry.value.firstOrNull;
      }
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          widget.prefill == null ? 'New trip request' : 'Customize your trip',
        ),
        actions: const [ProfileButton()],
      ),
      body: Form(
        key: _form,
        // Not a lazy ListView: every field must stay mounted, or Form.validate() skips the ones scrolled away.
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              AppTextField(
                label: 'What would you like to do?',
                hint: 'e.g. 5 days in Kandy and Ella, prefer the hill-country train',
                controller: _objective,
                maxLines: 3,
                validator: TripFormRules.objective,
              ),
              const SizedBox(height: 12),
              FormField<DateTimeRange>(
                key: const ValueKey('dates-field'),
                validator: (_) => TripFormRules.dates(_dates, _today),
                builder: (field) => InputDecorator(
                  decoration: InputDecoration(
                    labelText: 'Travel dates',
                    errorText: field.errorText,
                  ),
                  child: InkWell(
                    onTap: () async {
                      final picked = await _pickDates();
                      if (picked != null) {
                        setState(() => _dates = picked);
                        field.didChange(picked);
                      }
                    },
                    child: Row(
                      children: [
                        const Icon(Icons.date_range),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            _dates == null
                                ? (widget.prefill == null
                                      ? 'Choose dates'
                                      : 'Choose a start date')
                                : '${formatDate(_dates!.start.toIso8601String())} – ${formatDate(_dates!.end.toIso8601String())}',
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              FormField<List<String>>(
                key: const ValueKey('cities-field'),
                validator: (_) => TripFormRules.cities(_cities, _dates),
                builder: (field) => CityPicker(
                  selected: _cities,
                  errorText: field.errorText ?? _citiesServerError,
                  onChanged: (next) {
                    setState(() {
                      _cities = next;
                      _citiesServerError = null;
                    });
                    field.didChange(next);
                  },
                ),
              ),
              const SizedBox(height: 16),
              TravellersField(controller: _pax),
              const SizedBox(height: 12),
              AppTextField(
                label: 'Budget (USD)',
                controller: _budget,
                keyboardType: const TextInputType.numberWithOptions(
                  decimal: true,
                ),
                inputFormatters: [
                  FilteringTextInputFormatter.allow(RegExp(r'[0-9.]')),
                ],
                prefixIcon: Icons.attach_money,
                validator: TripFormRules.budget,
              ),
              const SizedBox(height: 16),
              Text(
                'Preferences',
                style: Theme.of(context).textTheme.titleSmall,
              ),
              const SizedBox(height: 8),
              Wrap(
                spacing: 8,
                runSpacing: 4,
                children: [
                  for (final option in preferenceOptions)
                    FilterChip(
                      label: Text(option.label),
                      selected: _preferences.contains(option),
                      onSelected: (on) => setState(
                        () => on
                            ? _preferences.add(option)
                            : _preferences.remove(option),
                      ),
                    ),
                ],
              ),
              const SizedBox(height: 16),
              AppTextField(
                label: 'Nationality',
                controller: _nationality,
                validator: (v) => (v == null || v.trim().isEmpty)
                    ? 'Nationality is required.'
                    : null,
              ),
              const SizedBox(height: 12),
              AppTextField(
                label: 'Passport number',
                hint: 'Only the last 4 characters are stored',
                controller: _passport,
                validator: TripFormRules.passportNumber,
              ),
              const SizedBox(height: 16),
              _PhotoPicker(
                photo: _photo,
                missing: _photoMissing,
                onCamera: () => _pickPhoto(ImageSource.camera),
                onGallery: () => _pickPhoto(ImageSource.gallery),
              ),
              const SizedBox(height: 24),
              PrimaryButton(
                label: 'Submit trip request',
                loading: _submitting,
                onPressed: _submit,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _PhotoPicker extends StatelessWidget {
  const _PhotoPicker({
    required this.photo,
    required this.missing,
    required this.onCamera,
    required this.onGallery,
  });

  final XFile? photo;
  final bool missing;
  final VoidCallback onCamera;
  final VoidCallback onGallery;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Passport photo', style: theme.textTheme.titleSmall),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onCamera,
                icon: const Icon(Icons.photo_camera),
                label: const Text('Camera'),
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onGallery,
                icon: const Icon(Icons.photo_library),
                label: const Text('Gallery'),
              ),
            ),
          ],
        ),
        if (photo != null)
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Text(
              'Selected: ${photo!.name}',
              semanticsLabel: 'Passport photo selected',
            ),
          ),
        if (missing)
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Text(
              'Add a photo of your passport.',
              style: TextStyle(color: theme.colorScheme.error),
            ),
          ),
      ],
    );
  }
}
