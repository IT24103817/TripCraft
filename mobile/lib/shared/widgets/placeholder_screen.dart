import 'package:flutter/material.dart';

import 'empty_state.dart';

/// For screens whose API is not merged yet: says exactly which endpoints are missing.
class PendingApiNotice extends StatelessWidget {
  const PendingApiNotice({
    super.key,
    required this.component,
    required this.endpoints,
  });

  final String component;
  final List<String> endpoints;

  @override
  Widget build(BuildContext context) {
    return EmptyState(
      icon: Icons.construction_outlined,
      title: 'Available when $component is merged',
      message: 'This needs: ${endpoints.join(', ')}',
    );
  }
}
