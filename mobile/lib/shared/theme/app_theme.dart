import 'package:flutter/material.dart';

/// Hallmark design tokens (.claude/skills/hallmark/SKILL.md), the same values as web/tailwind.config.ts.
/// Screens use these names or the theme, never raw colours.
class AppColors {
  const AppColors._();

  static const brand = Color(0xFF0F766E); // brand-700, primary
  static const brandDeep = Color(0xFF042F2E); // brand-950
  static const brandSoft = Color(0xFFCCFBF1); // brand-100
  static const accent = Color(0xFFD97706); // saffron: highlights and map pins
  static const ink = Color(0xFF0F172A); // primary text
  static const muted = Color(0xFF475569); // secondary text
  static const border = Color(0xFFE2E8F0);
  static const line = Color(
    0xFFCBD5E1,
  ); // slate-300: input outlines, timeline steps not reached
  static const canvas = Color(0xFFF8FAFC); // page background

  // Status tones (same meaning as web/src/shared/statuses.ts).
  static const success = Color(0xFF15803D);
  static const warning = Color(0xFFB45309);
  static const danger = Color(0xFFB91C1C);
  static const info = Color(0xFF1D4ED8);
  static const purple = Color(0xFF7E22CE);
  static const neutral = Color(0xFF475569);

  /// Kept for existing widgets: the primary colour is the brand colour.
  static const primary = brand;

  /// Kept for existing widgets: the page background.
  static const surface = canvas;
}

/// Radii and spacing of the design system.
class AppRadius {
  const AppRadius._();

  static const control = 10.0; // buttons, inputs
  static const card = 16.0; // cards, dialogs
  static const pill = 999.0; // chips, status badges
}

ThemeData buildAppTheme() {
  final scheme = ColorScheme.fromSeed(
    seedColor: AppColors.brand,
    primary: AppColors.brand,
    secondary: AppColors.accent,
    error: AppColors.danger,
    surface: Colors.white,
  );
  final base = ThemeData(colorScheme: scheme, useMaterial3: true);
  final text = base.textTheme.apply(
    bodyColor: AppColors.ink,
    displayColor: AppColors.ink,
  );
  const controlShape = RoundedRectangleBorder(
    borderRadius: BorderRadius.all(Radius.circular(AppRadius.control)),
  );

  return base.copyWith(
    scaffoldBackgroundColor: AppColors.canvas,
    textTheme: text.copyWith(
      headlineSmall: text.headlineSmall?.copyWith(fontWeight: FontWeight.w700),
      titleLarge: text.titleLarge?.copyWith(fontWeight: FontWeight.w600),
      titleMedium: text.titleMedium?.copyWith(fontWeight: FontWeight.w600),
      bodySmall: text.bodySmall?.copyWith(color: AppColors.muted),
    ),
    appBarTheme: AppBarTheme(
      centerTitle: false,
      backgroundColor: AppColors.canvas,
      foregroundColor: AppColors.ink,
      surfaceTintColor: Colors.transparent,
      titleTextStyle: text.titleLarge?.copyWith(
        fontWeight: FontWeight.w700,
        color: AppColors.ink,
      ),
    ),
    cardTheme: const CardThemeData(
      margin: EdgeInsets.zero,
      elevation: 0,
      color: Colors.white,
      surfaceTintColor: Colors.transparent,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(AppRadius.card)),
        side: BorderSide(color: AppColors.border),
      ),
    ),
    inputDecorationTheme: const InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      isDense: true,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(AppRadius.control)),
        borderSide: BorderSide(color: AppColors.border),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(AppRadius.control)),
        borderSide: BorderSide(color: AppColors.line),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(AppRadius.control)),
        borderSide: BorderSide(color: AppColors.brand, width: 2),
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        minimumSize: const Size.fromHeight(48),
        shape: controlShape,
        textStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        minimumSize: const Size(48, 48),
        shape: controlShape,
        side: const BorderSide(color: AppColors.line),
        textStyle: const TextStyle(fontWeight: FontWeight.w600),
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(
        foregroundColor: AppColors.brand,
        minimumSize: const Size(48, 48),
        textStyle: const TextStyle(fontWeight: FontWeight.w600),
      ),
    ),
    chipTheme: base.chipTheme.copyWith(
      shape: const StadiumBorder(side: BorderSide(color: AppColors.border)),
      selectedColor: AppColors.brandSoft,
      side: const BorderSide(color: AppColors.border),
    ),
    navigationBarTheme: const NavigationBarThemeData(
      backgroundColor: Colors.white,
      indicatorColor: AppColors.brandSoft,
      surfaceTintColor: Colors.transparent,
    ),
    snackBarTheme: const SnackBarThemeData(
      behavior: SnackBarBehavior.floating,
      backgroundColor: AppColors.ink,
    ),
    listTileTheme: const ListTileThemeData(
      contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(AppRadius.card)),
      ),
      iconColor: AppColors.muted,
    ),
    dividerTheme: const DividerThemeData(color: AppColors.border),
  );
}
