import 'package:flutter/material.dart';

import '../theme/app_theme.dart';

/// Trip request statuses of the v1.1 lifecycle (TripRequestStatus in the API, docs/API-V11.md):
/// the main path first, then the side states. Quotations go straight to the tourist (no operator review).
const tripStatuses = [
  'Submitted',
  'Planning',
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
  'ClientDeclined',
  'NeedsOperator',
  'Cancelled',
];

/// Colour for any trip, workflow or step status. Unknown statuses are neutral grey.
/// PendingApproval, Approved, Rejected and FailedSafely are agent workflow statuses (not trip statuses), so they
/// keep their colours for the workflow chip. RevisionRequested was retired in v1.1.
Color statusColor(String status) => switch (status) {
  'Submitted' || 'Cancelled' => AppColors.neutral,
  'Planning' || 'InProgress' => AppColors.info,
  'NeedsOperator' || 'PendingApproval' => AppColors.warning,
  'QuotationSent' => AppColors.accent,
  'ClientAccepted' ||
  'Approved' ||
  'Confirmed' ||
  'Completed' ||
  'Succeeded' => AppColors.success,
  'ClientDeclined' ||
  'Rejected' ||
  'FailedSafely' ||
  'Failed' => AppColors.danger,
  _ => AppColors.neutral,
};

/// Labels that read better than the spaced-out status name.
const _customLabels = {
  'ClientAccepted': 'Accepted',
  'ClientDeclined': 'Declined',
  'FailedSafely': 'Planning failed',
};

/// "QuotationSent" -> "Quotation sent"; "ClientAccepted" -> "Accepted"; "NeedsOperator" -> "Needs operator".
String statusLabel(String status) {
  final custom = _customLabels[status];
  if (custom != null) return custom;
  final spaced = status
      .replaceAllMapped(RegExp('([a-z])([A-Z])'), (m) => '${m[1]} ${m[2]}')
      .toLowerCase();
  return spaced.isEmpty
      ? spaced
      : spaced[0].toUpperCase() + spaced.substring(1);
}
