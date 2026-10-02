import '../../../core/router/routes.dart';
import '../data/notification_models.dart';

/// Where tapping a notification goes, or null to stay on the Alerts screen.
/// A tourist opens the trip the notification is about: "Your quotation is ready" (QuotationSent) and
/// "Your quote was updated, please review" (QuotationUpdated) land on the trip, where the Accept / Decline
/// panel is; so do ClientAccepted, ClientDeclined, NeedsOperator, TripConfirmed, TripCancelled and GuideChanged.
/// Guides see their trips on the schedule, so their notifications open nothing.
String? notificationRoute(AppNotification item, {required bool isTourist}) {
  final tripId = item.tripRequestId;
  if (!isTourist || tripId == null || tripId.isEmpty) return null;
  return Routes.trip(tripId);
}
