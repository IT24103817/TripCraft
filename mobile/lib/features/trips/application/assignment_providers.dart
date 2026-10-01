import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../data/assignment_models.dart';
import '../data/trip_assignment_repository.dart';

part 'assignment_providers.g.dart';

/// The guide and vehicle booked for a trip.
@riverpod
Future<TripAssignment> tripAssignment(Ref ref, String tripId) =>
    ref.watch(tripAssignmentRepositoryProvider).assignment(tripId);

/// The tourist's rating of the guide; null until they have rated.
@riverpod
Future<GuideRating?> guideRating(Ref ref, String tripId) =>
    ref.watch(tripAssignmentRepositoryProvider).guideRating(tripId);
