import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../shared/widgets/app_text_field.dart';
import 'trip_form_rules.dart';

/// "Travellers" with − and + buttons either side (used by the new-trip form and "Book as is").
/// [onChanged] gets the new number after a button press or typing.
class TravellersField extends StatelessWidget {
  const TravellersField({super.key, required this.controller, this.onChanged});

  final TextEditingController controller;
  final ValueChanged<int>? onChanged;

  void _change(int delta) {
    final next = ((int.tryParse(controller.text) ?? 0) + delta).clamp(
      0,
      TripFormRules.maxPax,
    );
    controller.text = '$next';
    onChanged?.call(next);
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        IconButton.outlined(
          tooltip: 'One traveller fewer',
          onPressed: () => _change(-1),
          icon: const Icon(Icons.remove),
        ),
        const SizedBox(width: 8),
        Expanded(
          child: AppTextField(
            label: 'Travellers',
            controller: controller,
            keyboardType: TextInputType.number,
            inputFormatters: [FilteringTextInputFormatter.digitsOnly],
            validator: TripFormRules.pax,
            onChanged: (text) => onChanged?.call(int.tryParse(text) ?? 0),
          ),
        ),
        const SizedBox(width: 8),
        IconButton.outlined(
          tooltip: 'One traveller more',
          onPressed: () => _change(1),
          icon: const Icon(Icons.add),
        ),
      ],
    );
  }
}
