import 'package:flutter/widgets.dart';

/// From QuotationSent on, the tourist can open the quotation (before that the operator is still reviewing it).
const quotationVisibleStatuses = {
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
};

/// Vouchers, a guide and a vehicle exist once the operator has confirmed the trip.
const voucherStatuses = {'Confirmed', 'InProgress', 'Completed'};

/// A finished or cancelled trip has nothing left to cancel.
const noCancelStatuses = {'Completed', 'Cancelled'};

/// Only the tourist may (re)start planning: a new request, or after planning failed safely.
const canStartPlanningStatuses = {'Submitted', 'FailedSafely'};

/// Builds the Accept / Decline panel shown at QuotationSent. The router (the app's composition root) passes
/// it in, so the trips feature does not import the quotations feature. [onDecided] reloads the trip.
typedef DecisionPanelBuilder = Widget Function(
  String tripId,
  VoidCallback onDecided,
);
