import 'package:flutter/material.dart';

/// One light theme with a small, consistent palette.
class AppColors {
  const AppColors._();

  static const primary = Color(0xFF3F51B5); // indigo
  static const success = Color(0xFF2E7D32);
  static const warning = Color(0xFFB26A00);
  static const danger = Color(0xFFC62828);
  static const info = Color(0xFF1565C0);
  static const purple = Color(0xFF6A1B9A);
  static const neutral = Color(0xFF546E7A);
  static const surface = Color(0xFFF6F7FB);
}

ThemeData buildAppTheme() {
  final scheme = ColorScheme.fromSeed(
    seedColor: AppColors.primary,
    surface: Colors.white,
  );
  return ThemeData(
    colorScheme: scheme,
    useMaterial3: true,
    scaffoldBackgroundColor: AppColors.surface,
    appBarTheme: const AppBarTheme(centerTitle: false),
    inputDecorationTheme: const InputDecorationTheme(
      border: OutlineInputBorder(),
      isDense: true,
    ),
    cardTheme: const CardThemeData(margin: EdgeInsets.zero, elevation: 0.5),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(minimumSize: const Size.fromHeight(48)),
    ),
  );
}
