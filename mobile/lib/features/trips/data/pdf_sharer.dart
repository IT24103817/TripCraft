import 'dart:io';

import 'package:path_provider/path_provider.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:share_plus/share_plus.dart';

part 'pdf_sharer.g.dart';

/// Hands a downloaded PDF to the phone. An interface so tests can check what would be shared.
abstract class PdfSharer {
  Future<void> share(List<int> bytes, String fileName);
}

/// Saves the PDF in the app's temporary folder, then opens the system share sheet, from which the
/// tourist can open it in a PDF viewer, save it to Files or send it on.
class SystemPdfSharer implements PdfSharer {
  @override
  Future<void> share(List<int> bytes, String fileName) async {
    final folder = await getTemporaryDirectory();
    final file = File('${folder.path}/$fileName');
    await file.writeAsBytes(bytes, flush: true);
    await SharePlus.instance.share(
      ShareParams(
        files: [XFile(file.path, mimeType: 'application/pdf')],
        title: 'Your TripCraft itinerary',
      ),
    );
  }
}

@riverpod
PdfSharer pdfSharer(Ref ref) => SystemPdfSharer();
