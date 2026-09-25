import 'package:dio/dio.dart';

import '../../shared/utils/friendly_error.dart';

/// Every API failure reaches the UI as this: a sentence a tourist or guide can act on.
class UserFacingException implements Exception, FriendlyError {
  const UserFacingException(
    this.message, {
    this.statusCode,
    this.fieldErrors = const {},
  });

  @override
  final String message;
  final int? statusCode;

  /// Validation errors from a 400 ProblemDetails, keyed by field name.
  final Map<String, List<String>> fieldErrors;

  bool get isUnauthorized => statusCode == 401;

  @override
  String toString() => message;
}

/// Turns a Dio error (network problem or RFC 7807 ProblemDetails from the API) into a friendly message.
UserFacingException mapDioError(DioException error) {
  switch (error.type) {
    case DioExceptionType.connectionTimeout:
    case DioExceptionType.sendTimeout:
    case DioExceptionType.receiveTimeout:
      return const UserFacingException(
        'TripCraft is taking too long to answer. Please try again.',
      );
    case DioExceptionType.connectionError:
      return const UserFacingException(
        'Cannot reach TripCraft. Check your internet connection.',
      );
    default:
      break;
  }

  final status = error.response?.statusCode;
  final body = error.response?.data;
  final problem = body is Map ? body : const {};
  final detail = problem['detail'] is String
      ? problem['detail'] as String
      : null;
  final fieldErrors = <String, List<String>>{};
  if (problem['errors'] is Map) {
    (problem['errors'] as Map).forEach((key, value) {
      if (value is List) fieldErrors['$key'] = value.map((v) => '$v').toList();
    });
  }
  final firstFieldError = fieldErrors.values.expand((e) => e).firstOrNull;

  final message = switch (status) {
    400 => firstFieldError ?? detail ?? 'Please check the details you entered.',
    401 => 'Your session has expired. Please log in again.',
    403 => 'You are not allowed to do that.',
    404 => detail ?? 'We could not find that.',
    409 => detail ?? 'That cannot be done right now.',
    413 => 'The file is too large.',
    429 => 'Too many attempts. Wait a minute and try again.',
    final int s when s >= 500 =>
      'The TripCraft server had a problem. Please try again in a moment.',
    _ => 'Something went wrong. Please try again.',
  };
  return UserFacingException(
    message,
    statusCode: status,
    fieldErrors: fieldErrors,
  );
}
