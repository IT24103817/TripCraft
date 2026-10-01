import 'package:flutter/material.dart';

import '../theme/app_theme.dart';

/// Trip request statuses of the v1.1 lifecycle (TripRequestStatus in the API, docs/API-V11.md).
const tripStatuses = [
  'Submitted',
  'Planning',
  'PendingReview',
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
  'RevisionRequested',
  'FailedSafely',
  'Cancelled',
];

/// Colour for any trip, workflow or step status. Unknown statuses are neutral grey.
/// PendingApproval, Approved and Rejected are agent workflow statuses (no longer trip statuses).
Color statusColor(String status) => switch (status) {
  'Submitted' || 'Cancelled' => AppColors.neutral,
  'Planning' || 'InProgress' => AppColors.info,
  'PendingReview' || 'PendingApproval' => AppColors.warning,
  'QuotationSent' => AppColors.accent,
  'RevisionRequested' => AppColors.purple,
  'ClientAccepted' ||
  'Approved' ||
  'Confirmed' ||
  'Completed' ||
  'Succeeded' => AppColors.success,
  'Rejected' || 'FailedSafely' || 'Failed' => AppColors.danger,
  _ => AppColors.neutral,
};

/// Labels that read better than the spaced-out status name.
const _customLabels = {
  'ClientAccepted': 'Accepted',
  'FailedSafely': 'Planning failed',
};

/// "PendingReview" -> "Pending review"; "ClientAccepted" -> "Accepted".
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
