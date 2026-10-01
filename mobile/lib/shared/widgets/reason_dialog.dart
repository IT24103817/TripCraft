import 'package:flutter/material.dart';

import '../utils/validators.dart';

/// Asks for a required reason (cancel a trip, decline a quotation, ask for a replacement guide).
/// Returns the trimmed reason, or null when the user backs out.
Future<String?> showReasonDialog(
  BuildContext context, {
  required String title,
  required String message,
  required String confirmLabel,
  String fieldLabel = 'Reason',
  String cancelLabel = 'Back',
}) => showDialog<String>(
  context: context,
  builder: (_) => _ReasonDialog(
    title: title,
    message: message,
    confirmLabel: confirmLabel,
    fieldLabel: fieldLabel,
    cancelLabel: cancelLabel,
  ),
);

class _ReasonDialog extends StatefulWidget {
  const _ReasonDialog({
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
  State<_ReasonDialog> createState() => _ReasonDialogState();
}

class _ReasonDialogState extends State<_ReasonDialog> {
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
    return AlertDialog(
      title: Text(widget.title),
      content: Form(
        key: _form,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(widget.message),
            const SizedBox(height: 12),
            TextFormField(
              controller: _reason,
              maxLines: 3,
              autofocus: true,
              validator: Validators.reason,
              decoration: InputDecoration(labelText: widget.fieldLabel),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(widget.cancelLabel),
        ),
        FilledButton(onPressed: _confirm, child: Text(widget.confirmLabel)),
      ],
    );
  }
}
