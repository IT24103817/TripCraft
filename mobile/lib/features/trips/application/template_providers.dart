import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../data/template_models.dart';
import '../data/trip_templates_repository.dart';

part 'template_providers.g.dart';

/// The mood packages for the home screen.
@riverpod
Future<List<TripTemplate>> tripTemplates(Ref ref) =>
    ref.watch(tripTemplatesRepositoryProvider).templates();

/// One package with its itinerary, for the package screen.
@riverpod
Future<TripTemplate> tripTemplate(Ref ref, String templateId) =>
    ref.watch(tripTemplatesRepositoryProvider).template(templateId);
