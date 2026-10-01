import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';

import '../../../core/auth/profile_button.dart';
import '../data/resources_repository.dart';
import 'voucher_check_in.dart';
import 'voucher_lookup.dart';

/// Scans a voucher QR code with the camera (mobile_scanner asks for camera permission).
/// A tourist's trip voucher checks in the next stop; an older hotel QR looks the hotel up.
/// GPS check-in on the day screen stays as the alternative.
class QrScanScreen extends StatefulWidget {
  const QrScanScreen({super.key, this.tripId});

  /// Set when opened from a trip: the scanned voucher must belong to that trip.
  final String? tripId;

  @override
  State<QrScanScreen> createState() => _QrScanScreenState();
}

class _QrScanScreenState extends State<QrScanScreen> {
  final _controller = MobileScannerController(
    detectionSpeed: DetectionSpeed.noDuplicates,
  );
  String? _code;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onDetect(BarcodeCapture capture) {
    final value = capture.barcodes
        .map((b) => b.rawValue)
        .whereType<String>()
        .firstOrNull;
    if (value != null && value != _code) setState(() => _code = value);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Scan voucher'),
        actions: const [ProfileButton()],
      ),
      body: Column(
        children: [
          Expanded(
            child: MobileScanner(
              controller: _controller,
              onDetect: _onDetect,
              errorBuilder: (context, error) => Center(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Text(
                    error.errorCode == MobileScannerErrorCode.permissionDenied
                        ? 'Allow camera access in Settings to scan vouchers.'
                        : 'The camera is not available.',
                    textAlign: TextAlign.center,
                  ),
                ),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(16),
            child: ScanResultPanel(code: _code, tripId: widget.tripId),
          ),
        ],
      ),
    );
  }
}

/// What to do with the scanned code. Separate from the camera so tests can render it.
class ScanResultPanel extends StatelessWidget {
  const ScanResultPanel({super.key, required this.code, this.tripId});

  final String? code;
  final String? tripId;

  @override
  Widget build(BuildContext context) {
    final scanned = code;
    if (scanned == null) {
      return const Text('Point the camera at the tourist\'s trip voucher.');
    }
    return switch (kindOfScan(scanned)) {
      ScannedKind.tripVoucher => VoucherCheckIn(code: scanned, tripId: tripId),
      ScannedKind.hotel => VoucherLookup(code: scanned),
      ScannedKind.unknown => const Text(
        'This QR code is not a TripCraft voucher.',
      ),
    };
  }
}
