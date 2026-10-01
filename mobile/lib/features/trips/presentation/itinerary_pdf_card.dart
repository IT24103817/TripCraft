import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/section_card.dart';
import '../data/pdf_sharer.dart';
import '../data/trips_repository.dart';

/// The PDF exists once the operator has sent the quotation (the API answers 409 before that).
const itineraryPdfStatuses = {
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
};

/// "Download itinerary PDF": fetches the PDF with the tourist's token, then opens or shares it.
class ItineraryPdfCard extends ConsumerStatefulWidget {
  const ItineraryPdfCard({super.key, required this.tripId});

  final String tripId;

  @override
  ConsumerState<ItineraryPdfCard> createState() => _ItineraryPdfCardState();
}

class _ItineraryPdfCardState extends ConsumerState<ItineraryPdfCard> {
  bool _loading = false;

  Future<void> _download() async {
    final messenger = ScaffoldMessenger.of(context);
    setState(() => _loading = true);
    try {
      final bytes = await ref
          .read(tripsRepositoryProvider)
          .itineraryPdf(widget.tripId);
      await ref
          .read(pdfSharerProvider)
          .share(bytes, 'tripcraft-itinerary-${widget.tripId}.pdf');
    } catch (error) {
      // e.g. the API's 409 detail before a quotation was sent.
      messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Itinerary PDF',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text(
            'Your day-by-day plan and the quotation, with the deposit, in one file.',
          ),
          const SizedBox(height: 12),
          OutlinedButton.icon(
            onPressed: _loading ? null : _download,
            icon: _loading
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      semanticsLabel: 'Downloading',
                    ),
                  )
                : const Icon(Icons.picture_as_pdf_outlined),
            label: const Text('Download itinerary PDF'),
          ),
        ],
      ),
    );
  }
}
