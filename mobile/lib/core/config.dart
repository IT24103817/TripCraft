/// Build-time settings. Pass them with --dart-define, e.g.
///   flutter run --dart-define=API_URL=https://tripcraft-api.onrender.com
/// The default reaches the API on the host machine from the Android emulator (10.0.2.2 = host localhost).
class AppConfig {
  const AppConfig._();

  static const String apiUrl = String.fromEnvironment(
    'API_URL',
    defaultValue: 'http://10.0.2.2:5080',
  );

  /// Trip detail refreshes the workflow this often while the agents are planning.
  static const Duration workflowPollInterval = Duration(seconds: 10);

  /// While the app is open, GET /api/notifications/mine is polled this often.
  static const Duration notificationPollInterval = Duration(seconds: 30);
}
