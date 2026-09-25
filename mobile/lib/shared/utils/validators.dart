/// Form validators that mirror the API's FluentValidation rules, so errors show before a round trip.
class Validators {
  const Validators._();

  static final _email = RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$');

  static String? required(String? value, String field) =>
      (value == null || value.trim().isEmpty) ? '$field is required.' : null;

  static String? email(String? value) {
    if (value == null || value.trim().isEmpty) return 'Email is required.';
    return _email.hasMatch(value.trim())
        ? null
        : 'Enter a valid email address.';
  }

  /// PasswordRules.StrongPassword: 8+ characters with upper-case, lower-case and a digit.
  static String? strongPassword(String? value) {
    if (value == null || value.isEmpty) return 'Password is required.';
    if (value.length < 8) return 'Password must be at least 8 characters.';
    if (!RegExp('[A-Z]').hasMatch(value)) {
      return 'Password must contain an upper-case letter.';
    }
    if (!RegExp('[a-z]').hasMatch(value)) {
      return 'Password must contain a lower-case letter.';
    }
    if (!RegExp('[0-9]').hasMatch(value)) {
      return 'Password must contain a digit.';
    }
    return null;
  }
}
