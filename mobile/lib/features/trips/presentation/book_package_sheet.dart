import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';

import '../../../core/auth/auth_repository.dart';
import '../../../core/router/routes.dart';
import '../../../shared/utils/clock.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/app_text_field.dart';
import '../../../shared/widgets/primary_button.dart';
import '../application/trips_providers.dart';
import '../data/template_models.dart';
import '../data/tourist_profile.dart';
import '../data/tourist_profile_repository.dart';
import '../data/trip_templates_repository.dart';
import 'passport_photo_picker.dart';
import 'travellers_field.dart';
import 'trip_form_rules.dart';
import 'trip_prefill.dart';

/// "Book as is": a bottom sheet with the few details a package still needs (start date, travellers, budget,
/// nationality, passport and — when the profile has none yet — a passport photo). Booking uploads the photo
/// first, then creates the trip, starts planning and opens the new trip.
Future<void> showBookPackageSheet(
  BuildContext context,
  TripTemplate template,
) => showModalBottomSheet<void>(
  context: context,
  isScrollControlled: true,
  showDragHandle: true,
  builder: (_) => BookPackageSheet(template: template),
);

class BookPackageSheet extends ConsumerStatefulWidget {
  const BookPackageSheet({super.key, required this.template});

  final TripTemplate template;

  @override
  ConsumerState<BookPackageSheet> createState() => _BookPackageSheetState();
}

class _BookPackageSheetState extends ConsumerState<BookPackageSheet> {
  final _form = GlobalKey<FormState>();
  final _pax = TextEditingController(text: '2');
  final _budget = TextEditingController();
  final _nationality = TextEditingController();
  final _passport = TextEditingController();
  DateTime? _start;
  bool _saving = false;
  String? _error;

  /// GET /api/tourists/me; null while it loads.
  TouristProfile? _profile;
  XFile? _photo;
  bool _photoMissing = false;

  DateTime get _today => dateOnly(ref.read(clockProvider)());

  @override
  void initState() {
    super.initState();
    _budget.text = _suggestedBudget(2);
    _loadProfile();
  }

  /// Loads the tourist's profile to know whether a passport photo is on file, and pre-fills the nationality
  /// (from the profile, else the one given at registration). If the profile cannot be loaded, the sheet
  /// simply asks for the photo: uploading it again does no harm.
  Future<void> _loadProfile() async {
    TouristProfile profile;
    try {
      profile = await ref.read(touristProfileRepositoryProvider).me();
    } catch (_) {
      profile = const TouristProfile();
    }
    final nationality = profile.nationality.isNotEmpty
        ? profile.nationality
        : await ref.read(authRepositoryProvider).savedNationality();
    if (!mounted) return;
    setState(() => _profile = profile);
    if (nationality != null && _nationality.text.isEmpty) {
      _nationality.text = nationality;
    }
  }

  bool get _needsPhoto => _profile != null && !_profile!.hasPassportPhoto;

  Future<void> _pickPhoto(ImageSource source) async {
    final photo = await pickPassportPhoto(
      ref.read(imagePickerProvider),
      source,
    );
    if (photo == null || !mounted) return;
    setState(() {
      _photo = photo;
      _photoMissing = false;
    });
  }

  @override
  void dispose() {
    for (final c in [_pax, _budget, _nationality, _passport]) {
      c.dispose();
    }
    super.dispose();
  }

  String _suggestedBudget(int pax) =>
      suggestedBudgetUsd(widget.template, pax).toStringAsFixed(0);

  /// The trip's dates once a start date is chosen: the package length decides the end date.
  DateTimeRange? get _dates => _start == null
      ? null
      : DateTimeRange(
          start: _start!,
          end: _start!.add(Duration(days: widget.template.days - 1)),
        );

  Future<void> _book() async {
    final formValid = _form.currentState!.validate();
    setState(() => _photoMissing = _needsPhoto && _photo == null);
    if (!formValid || _photoMissing) return;
    setState(() {
      _saving = true;
      _error = null;
    });
    // Taken before the await: the sheet is closed afterwards.
    final router = GoRouter.of(context);
    final messenger = ScaffoldMessenger.of(context);
    final navigator = Navigator.of(context);
    try {
      // The photo goes to the profile first; if that fails nothing is booked.
      if (_needsPhoto) {
        final profile = await ref
            .read(touristProfileRepositoryProvider)
            .uploadPassportPhoto(_photo!.path);
        if (mounted) setState(() => _profile = profile);
      }
      final result = await ref
          .read(tripTemplatesRepositoryProvider)
          .book(
            widget.template.id,
            BookTemplateRequest(
              startDate: toApiDate(_start!),
              pax: int.parse(_pax.text),
              budgetUsd: double.parse(_budget.text),
              nationality: _nationality.text.trim(),
              passportNumber: _passport.text.trim(),
            ),
          );
      ref.invalidate(myTripsProvider);
      navigator.pop();
      router.go(Routes.trip(result.trip.id));
      messenger.showSnackBar(
        SnackBar(
          content: Text(
            result.planning.workflowStatus == 'FailedSafely'
                ? 'Trip saved, but planning could not start. Try again from the trip.'
                : 'Planning started',
          ),
        ),
      );
    } catch (error) {
      if (mounted) setState(() => _error = friendlyMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  /// The passport photo step: checking, already on file, or the Camera / Gallery picker.
  Widget _photoSection() {
    if (_profile == null) {
      return const Row(
        children: [
          SizedBox(
            width: 16,
            height: 16,
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
          SizedBox(width: 8),
          Expanded(child: Text('Checking your passport photo…')),
        ],
      );
    }
    if (!_needsPhoto) return const PassportPhotoOnFile();
    return PassportPhotoPicker(
      photo: _photo,
      missing: _photoMissing,
      onCamera: _saving ? null : () => _pickPhoto(ImageSource.camera),
      onGallery: _saving ? null : () => _pickPhoto(ImageSource.gallery),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final keyboard = MediaQuery.viewInsetsOf(context).bottom;
    final dates = _dates;
    return Padding(
      padding: EdgeInsets.only(bottom: keyboard),
      child: Form(
        key: _form,
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                'Book ${widget.template.name}',
                style: theme.textTheme.titleLarge,
              ),
              Text(
                '${widget.template.days} days · ${widget.template.cities.join(' → ')}',
                style: theme.textTheme.bodySmall,
              ),
              const SizedBox(height: 16),
              FormField<DateTime>(
                key: const ValueKey('start-date-field'),
                validator: (_) => _start == null
                    ? 'Choose a start date.'
                    : TripFormRules.dates(_dates, _today),
                builder: (field) => InputDecorator(
                  decoration: InputDecoration(
                    labelText: 'Start date',
                    errorText: field.errorText,
                  ),
                  child: InkWell(
                    onTap: () async {
                      final picked = await showDatePicker(
                        context: context,
                        firstDate: _today,
                        lastDate: _today.add(const Duration(days: 365)),
                        initialDate: _start ?? _today,
                      );
                      if (picked != null) {
                        setState(() => _start = picked);
                        field.didChange(picked);
                      }
                    },
                    child: Row(
                      children: [
                        const Icon(Icons.event),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            dates == null
                                ? 'Choose a start date'
                                : '${formatDate(toApiDate(dates.start))} – ${formatDate(toApiDate(dates.end))}',
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 12),
              // The budget follows the number of travellers (the from-price is for two).
              TravellersField(
                controller: _pax,
                onChanged: (pax) {
                  if (pax >= 1) _budget.text = _suggestedBudget(pax);
                },
              ),
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
              const SizedBox(height: 12),
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
              _photoSection(),
              if (_error != null) ...[
                const SizedBox(height: 12),
                Text(_error!, style: TextStyle(color: theme.colorScheme.error)),
              ],
              const SizedBox(height: 16),
              PrimaryButton(
                label: 'Book this trip',
                loading: _saving,
                // Wait for the profile, so we know whether to ask for the passport photo.
                onPressed: _profile == null ? null : _book,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
