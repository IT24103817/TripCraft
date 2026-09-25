import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'app.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(
    ProviderScope(
      // No automatic retries: every screen has its own Retry button and pull-to-refresh.
      retry: (retryCount, error) => null,
      child: const TripCraftApp(),
    ),
  );
}
