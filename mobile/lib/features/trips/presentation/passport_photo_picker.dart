import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';

import '../../../shared/theme/app_theme.dart';

/// The camera / gallery, as a provider so tests can swap in a fake.
final imagePickerProvider = Provider<ImagePicker>((ref) => ImagePicker());

/// Takes a passport photo from [source]. When the camera is not available (an emulator, or no camera
/// permission) it falls back to the gallery. Null when the tourist cancels.
Future<XFile?> pickPassportPhoto(ImagePicker picker, ImageSource source) async {
  try {
    return await picker.pickImage(
      source: source,
      maxWidth: 1600,
      imageQuality: 85,
    );
  } catch (_) {
    if (source != ImageSource.camera) rethrow;
    return picker.pickImage(
      source: ImageSource.gallery,
      maxWidth: 1600,
      imageQuality: 85,
    );
  }
}

/// "Passport photo": Camera and Gallery buttons, a thumbnail once a photo is chosen, and an error when
/// [missing] (the form was submitted without one). Used by the new-trip form and the "Book as is" sheet.
class PassportPhotoPicker extends StatelessWidget {
  const PassportPhotoPicker({
    super.key,
    required this.photo,
    required this.missing,
    required this.onCamera,
    required this.onGallery,
  });

  final XFile? photo;
  final bool missing;
  final VoidCallback? onCamera;
  final VoidCallback? onGallery;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Passport photo', style: theme.textTheme.titleSmall),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onCamera,
                icon: const Icon(Icons.photo_camera),
                label: const Text('Camera'),
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onGallery,
                icon: const Icon(Icons.photo_library),
                label: const Text('Gallery'),
              ),
            ),
          ],
        ),
        if (photo != null)
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Row(
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.circular(6),
                  child: Image.file(
                    File(photo!.path),
                    width: 56,
                    height: 56,
                    fit: BoxFit.cover,
                    // A file the app cannot draw still counts as chosen; show an icon instead.
                    errorBuilder: (_, _, _) =>
                        const Icon(Icons.image, size: 56),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    'Selected: ${photo!.name}',
                    semanticsLabel: 'Passport photo selected',
                  ),
                ),
              ],
            ),
          ),
        if (missing)
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Text(
              'Add a photo of your passport.',
              style: TextStyle(color: theme.colorScheme.error),
            ),
          ),
      ],
    );
  }
}

/// The small line shown instead of [PassportPhotoPicker] when the profile already has a passport photo.
class PassportPhotoOnFile extends StatelessWidget {
  const PassportPhotoOnFile({super.key});

  @override
  Widget build(BuildContext context) {
    return const Row(
      children: [
        Icon(Icons.badge_outlined, color: AppColors.success, size: 20),
        SizedBox(width: 8),
        Expanded(child: Text('Passport photo on file ✓')),
      ],
    );
  }
}
