import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/reason_sheet.dart';
import '../data/quotation_models.dart';
import '../data/quotations_repository.dart';

// The tourist's decision on a quotation, used by the quotation screen and by the panel on the trip.
// Each returns true when the API accepted the decision; a snackbar says what happened either way.

/// POST /api/quotations/{id}/accept.
Future<bool> acceptQuotation(
  BuildContext context,
  WidgetRef ref,
  String quotationId,
) => _decide(
  context,
  () => ref.read(quotationsRepositoryProvider).accept(quotationId),
  'Quotation accepted. The operator will now confirm your trip.',
);

/// Asks for the reason in a bottom sheet (required), then POST /api/quotations/{id}/decline {reason}.
Future<bool> declineQuotation(
  BuildContext context,
  WidgetRef ref,
  String quotationId,
) async {
  final reason = await showReasonSheet(
    context,
    title: 'Decline this quotation?',
    message: 'Tell the operator what you would like changed. They will send you a new version.',
    confirmLabel: 'Decline',
  );
  if (reason == null || !context.mounted) return false;
  return _decide(
    context,
    () => ref.read(quotationsRepositoryProvider).decline(quotationId, reason),
    'Quotation declined. The operator will prepare a new version.',
  );
}

Future<bool> _decide(
  BuildContext context,
  Future<QuotationDecision> Function() call,
  String success,
) async {
  final messenger = ScaffoldMessenger.of(context);
  try {
    await call();
    messenger.showSnackBar(SnackBar(content: Text(success)));
    return true;
  } catch (error) {
    messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
    return false;
  }
}
