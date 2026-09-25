import 'package:flutter/material.dart';

import '../theme/app_theme.dart';

/// Trip request statuses from PLAN.md section 3 (TripRequestStatus in the API).
const tripStatuses = [
  'Submitted',
  'Planning',
  'PendingApproval',
  'Approved',
  'Rejected',
  'RevisionRequested',
  'Confirmed',
  'InProgress',
  'Completed',
  'Cancelled',
];

/// Colour for any trip, workflow or step status. Unknown statuses are neutral grey.
Color statusColor(String status) => switch (status) {
  'Submitted' || 'Cancelled' => AppColors.neutral,
  'Planning' || 'InProgress' => AppColors.info,
  'PendingApproval' => AppColors.warning,
  'RevisionRequested' => AppColors.purple,
  'Approved' || 'Confirmed' || 'Completed' || 'Succeeded' => AppColors.success,
  'Rejected' || 'FailedSafely' || 'Failed' => AppColors.danger,
  _ => AppColors.neutral,
};

/// "PendingApproval" -> "Pending approval".
String statusLabel(String status) {
  final spaced = status
      .replaceAllMapped(RegExp('([a-z])([A-Z])'), (m) => '${m[1]} ${m[2]}')
      .toLowerCase();
  return spaced.isEmpty
      ? spaced
      : spaced[0].toUpperCase() + spaced.substring(1);
}
