/// Implemented by errors whose message is safe and useful to show to the user.
abstract interface class FriendlyError {
  String get message;
}

/// The text to show for any error: the friendly message when there is one, a generic sentence otherwise.
String friendlyMessage(Object error) => error is FriendlyError
    ? error.message
    : 'Something unexpected happened. Please try again.';
