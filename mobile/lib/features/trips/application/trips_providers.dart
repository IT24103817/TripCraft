import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/config.dart';
import '../data/trip_models.dart';
import '../data/trips_repository.dart';

part 'trips_providers.g.dart';

@riverpod
Future<List<TripRequest>> myTrips(
  Ref ref, {
  String search = '',
  String status = '',
}) =>
    ref.watch(tripsRepositoryProvider).myTrips(search: search, status: status);

@riverpod
Future<TripRequest> tripDetail(Ref ref, String tripId) =>
    ref.watch(tripsRepositoryProvider).trip(tripId);

/// The trip's workflow, re-fetched every 10 s while the agents are Planning; stops once it leaves Planning.
/// Leaving the screen disposes the provider, which cancels the loop.
@riverpod
Stream<TripWorkflow?> tripWorkflow(Ref ref, String tripId) async* {
  final repository = ref.watch(tripsRepositoryProvider);
  while (true) {
    final workflow = await repository.workflow(tripId);
    yield workflow;
    if (workflow?.status != 'Planning') return;
    await Future<void>.delayed(AppConfig.workflowPollInterval);
    if (!ref.mounted) return;
  }
}

@riverpod
Future<CancellationInfo> cancellationInfo(Ref ref, String tripId) =>
    ref.watch(tripsRepositoryProvider).cancellationInfo(tripId);

/// The trip's vouchers, trip voucher first, then the hotel nights in date order.
@riverpod
Future<List<TripVoucher>> tripVouchers(Ref ref, String tripId) async =>
    sortVouchers(await ref.watch(tripsRepositoryProvider).vouchers(tripId));

/// The cities the trip form offers (GET /api/attractions/cities).
@riverpod
Future<List<String>> tripCities(Ref ref) =>
    ref.watch(tripsRepositoryProvider).cities();

/// Trip voucher first, then hotel-night vouchers by night ("yyyy-MM-dd" strings sort by date).
List<TripVoucher> sortVouchers(List<TripVoucher> vouchers) {
  final trip = vouchers.where((v) => v.type == 'Trip');
  final nights = vouchers.where((v) => v.type != 'Trip').toList()
    ..sort((a, b) => (a.night ?? '').compareTo(b.night ?? ''));
  return [...trip, ...nights];
}

@riverpod
Future<List<TripHistoryEntry>> tripHistory(Ref ref, String tripId) =>
    ref.watch(tripsRepositoryProvider).history(tripId);

@riverpod
Future<List<TripDay>> savedItinerary(Ref ref, String tripId) =>
    ref.watch(tripsRepositoryProvider).savedItinerary(tripId);

/// Coordinates of the stops for the map. [attractionIds] is a comma-separated list (a String keeps
/// the provider key stable; a List would be a new key on every build).
@riverpod
Future<List<Attraction>> stopLocations(Ref ref, String attractionIds) async {
  final repository = ref.watch(tripsRepositoryProvider);
  final ids = attractionIds.split(',').where((id) => id.isNotEmpty).toSet();
  return Future.wait(ids.map(repository.attraction));
}

/// Days from the agents' proposal when there is one, otherwise from the saved itinerary.
/// A FailedSafely workflow's days were never checked, so they are not shown to the tourist.
List<TripDay> proposalDays(TripWorkflow? workflow) => [
  if (workflow?.status != 'FailedSafely')
    for (final d
        in workflow?.finalOutcome?.proposal.days ?? const <ProposalDay>[])
      TripDay(
        day: d.day,
        date: d.date,
        city: d.city,
        transport: d.transport,
        weather: d.weather,
        stops: [
          for (final s in d.stops ?? const <ProposalStop>[])
            TripStop(attractionId: s.attractionId, name: s.name),
        ],
      ),
];
