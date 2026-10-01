import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/reason_dialog.dart';
import '../data/resources_repository.dart';

/// "Request a change": the guide asks the operator to replace them on a confirmed trip (with a required reason).
/// A 409 (trip not Confirmed, or a request is already open) is shown as the API's message.
class RequestReplacementButton extends ConsumerWidget {
  const RequestReplacementButton({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return TextButton.icon(
      icon: const Icon(Icons.swap_horiz),
      label: const Text('Request a change'),
      onPressed: () async {
        final messenger = ScaffoldMessenger.of(context);
        final reason = await showReasonDialog(
          context,
          title: 'Request a replacement guide?',
          message: 'Tell the operator why you cannot guide this trip. They will choose another guide.',
          confirmLabel: 'Send request',
        );
        if (reason == null) return;
        try {
          await ref
              .read(resourcesRepositoryProvider)
              .requestReplacement(tripId, reason);
          messenger.showSnackBar(
            const SnackBar(
              content: Text(
                'Request sent. The operator will choose a replacement guide.',
              ),
            ),
          );
        } catch (error) {
          messenger.showSnackBar(
            SnackBar(content: Text(friendlyMessage(error))),
          );
        }
      },
    );
  }
}
