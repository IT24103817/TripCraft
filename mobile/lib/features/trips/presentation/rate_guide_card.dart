import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/app_text_field.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/primary_button.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/assignment_providers.dart';
import '../application/trips_providers.dart';
import '../data/assignment_models.dart';
import '../data/trip_assignment_repository.dart';

/// "Rate your guide" after a completed trip: 1–5 stars and an optional comment, once.
/// After rating, the card shows the rating instead of the form.
class RateGuideCard extends ConsumerWidget {
  const RateGuideCard({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SectionCard(
      title: 'Rate your guide',
      child: AsyncView<GuideRating?>(
        value: ref.watch(guideRatingProvider(tripId)),
        onRetry: () => ref.invalidate(guideRatingProvider(tripId)),
        data: (rating) => rating == null
            ? _RatingForm(tripId: tripId)
            : _RatingSummary(rating: rating),
      ),
    );
  }
}

class _RatingSummary extends StatelessWidget {
  const _RatingSummary({required this.rating});

  final GuideRating rating;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        StarsRow(stars: rating.stars),
        const SizedBox(height: 4),
        Text(
          'You rated ${rating.guideName} ${rating.stars} out of 5. Thank you!',
        ),
        if (rating.comment != null) ...[
          const SizedBox(height: 4),
          Text(
            '“${rating.comment}”',
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ],
      ],
    );
  }
}

class _RatingForm extends ConsumerStatefulWidget {
  const _RatingForm({required this.tripId});

  final String tripId;

  @override
  ConsumerState<_RatingForm> createState() => _RatingFormState();
}

class _RatingFormState extends ConsumerState<_RatingForm> {
  final _comment = TextEditingController();
  int _stars = 0;
  bool _saving = false;

  @override
  void dispose() {
    _comment.dispose();
    super.dispose();
  }

  Future<void> _send() async {
    final messenger = ScaffoldMessenger.of(context);
    setState(() => _saving = true);
    try {
      await ref
          .read(tripAssignmentRepositoryProvider)
          .rateGuide(widget.tripId, stars: _stars, comment: _comment.text);
      messenger.showSnackBar(
        const SnackBar(content: Text('Thank you for rating your guide.')),
      );
      // The card now shows the rating, and the trip card says "Completed".
      ref.invalidate(guideRatingProvider(widget.tripId));
      ref.invalidate(myTripsProvider);
    } catch (error) {
      messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const Text('How was your guide? Tap the stars.'),
        StarsRow(
          stars: _stars,
          onSelected: (stars) => setState(() => _stars = stars),
        ),
        const SizedBox(height: 8),
        AppTextField(
          label: 'Comment (optional)',
          controller: _comment,
          maxLines: 3,
          validator: (v) => (v ?? '').length > 500
              ? 'Please keep it under 500 characters.'
              : null,
        ),
        const SizedBox(height: 12),
        PrimaryButton(
          label: 'Send rating',
          loading: _saving,
          onPressed: _stars == 0 ? null : _send,
        ),
      ],
    );
  }
}

/// Five stars, the first [stars] filled. With [onSelected] each star is a 48 dp button.
class StarsRow extends StatelessWidget {
  const StarsRow({super.key, required this.stars, this.onSelected});

  final int stars;
  final ValueChanged<int>? onSelected;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        for (var i = 1; i <= 5; i++)
          onSelected == null
              ? Icon(
                  i <= stars ? Icons.star : Icons.star_border,
                  color: AppColors.accent,
                  semanticLabel: i == 1 ? '$stars out of 5 stars' : null,
                )
              : IconButton(
                  tooltip: '$i star${i == 1 ? '' : 's'}',
                  onPressed: () => onSelected!(i),
                  icon: Icon(
                    i <= stars ? Icons.star : Icons.star_border,
                    color: AppColors.accent,
                  ),
                ),
      ],
    );
  }
}
