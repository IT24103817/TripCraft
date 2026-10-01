/// Waits for a reload started by pull-to-refresh. A failure is not rethrown: the screen already shows it
/// with a Retry button, so the refresh spinner simply stops.
Future<void> waitForReload(Future<Object?> reload) async {
  try {
    await reload;
  } catch (_) {
    // Shown by the screen's error state.
  }
}
