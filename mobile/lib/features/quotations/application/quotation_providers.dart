import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../data/quotation_models.dart';
import '../data/quotations_repository.dart';

part 'quotation_providers.g.dart';

@riverpod
Future<QuotationView> quotationView(Ref ref, String tripId) =>
    ref.watch(quotationsRepositoryProvider).quotationFor(tripId);
