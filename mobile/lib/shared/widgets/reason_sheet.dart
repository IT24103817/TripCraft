import 'package:flutter/material.dart';

import '../utils/validators.dart';

/// A bottom sheet that asks for a required reason (e.g. declining a quotation).
/// Returns the trimmed reason, or null when the user backs out.
Future<String?> showReasonSheet(
  BuildContext context, {
  required String title,
  required String message,
  required String confirmLabel,
  String fieldLabel = 'Reason',
  String cancelLabel = 'Back',
}) => showModalBottomSheet<String>(
  context: context,
  // Lets the sheet grow above the keyboard.
  isScrollControlled: true,
  showDragHandle: true,
  builder: (_) => _ReasonSheet(
    title: title,
    message: message,
    confirmLabel: confirmLabel,
    fieldLabel: fieldLabel,
    cancelLabel: cancelLabel,
  ),
);

class _ReasonSheet extends StatefulWidget {
  const _ReasonSheet({
    required this.title,
    required this.message,
    required this.confirmLabel,
    required this.fieldLabel,
    required this.cancelLabel,
  });

  final String title;
  final String message;
  final String confirmLabel;
  final String fieldLabel;
  final String cancelLabel;

  @override
  State<_ReasonSheet> createState() => _ReasonSheetState();
}

class _ReasonSheetState extends State<_ReasonSheet> {
  final _form = GlobalKey<FormState>();
  final _reason = TextEditingController();

  @override
  void dispose() {
    _reason.dispose();
    super.dispose();
  }

  void _confirm() {
    if (!_form.currentState!.validate()) return;
    Navigator.of(context).pop(_reason.text.trim());
  }

  @override
  Widget build(BuildContext context) {
    // The keyboard's height, so the field stays visible while typing.
    final keyboard = MediaQuery.viewInsetsOf(context).bottom;
    return Padding(
      padding: EdgeInsets.fromLTRB(16, 0, 16, 16 + keyboard),
      child: Form(
        key: _form,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(widget.title, style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 8),
            Text(widget.message),
            const SizedBox(height: 12),
            TextFormField(
              controller: _reason,
              maxLines: 3,
              autofocus: true,
              validator: Validators.reason,
              decoration: InputDecoration(labelText: widget.fieldLabel),
            ),
            const SizedBox(height: 16),
            FilledButton(onPressed: _confirm, child: Text(widget.confirmLabel)),
            const SizedBox(height: 8),
            OutlinedButton(
              onPressed: () => Navigator.of(context).pop(),
              child: Text(widget.cancelLabel),
            ),
          ],
        ),
      ),
    );
  }
}
