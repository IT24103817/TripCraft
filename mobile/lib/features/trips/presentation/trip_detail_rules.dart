import 'package:flutter/widgets.dart';

/// From QuotationSent on, the tourist can open the quotation (it is sent automatically once the agents have
/// priced the trip). After a decline it stays visible so the tourist can see what they declined.
const quotationVisibleStatuses = {
  'QuotationSent',
  'ClientAccepted',
  'ClientDeclined',
  'Confirmed',
  'InProgress',
  'Completed',
};

/// Vouchers, a guide and a vehicle exist once the operator has confirmed the trip.
const voucherStatuses = {'Confirmed', 'InProgress', 'Completed'};

/// A finished or cancelled trip has nothing left to cancel.
const noCancelStatuses = {'Completed', 'Cancelled'};

/// The tourist starts planning a new request. Retrying after the agents failed (NeedsOperator) is the
/// operator's action, so the tourist never sees a Try again button.
const canStartPlanningStatuses = {'Submitted'};

/// Builds the Accept / Decline panel shown at QuotationSent. The router (the app's composition root) passes
/// it in, so the trips feature does not import the quotations feature. [onDecided] reloads the trip.
typedef DecisionPanelBuilder = Widget Function(
  String tripId,
  VoidCallback onDecided,
);
