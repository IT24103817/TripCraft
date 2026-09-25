import 'package:flutter/material.dart';

import '../../../core/auth/profile_button.dart';
import '../../../shared/widgets/placeholder_screen.dart';

/// The guide's schedule. Needs Resource Management (Student B) before it can list trips.
class ScheduleScreen extends StatelessWidget {
  const ScheduleScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('My schedule'),
        actions: const [ProfileButton()],
      ),
      body: const PendingApiNotice(
        component: 'Resource Management (Student B)',
        endpoints: ['GET /api/guides/{id}/schedule', 'POST /api/check-ins'],
      ),
    );
  }
}
