import 'package:intl/intl.dart';

final _lkr = NumberFormat.currency(
  locale: 'en_US',
  symbol: 'LKR ',
  decimalDigits: 2,
);
final _usd = NumberFormat.currency(
  locale: 'en_US',
  symbol: 'USD ',
  decimalDigits: 2,
);
final _date = DateFormat('d MMM yyyy');
final _dateTime = DateFormat('d MMM yyyy, HH:mm');
final _apiDate = DateFormat('yyyy-MM-dd');

String formatLkr(num amount) => _lkr.format(amount);
String formatUsd(num amount) => _usd.format(amount);

/// "2026-10-10" or an ISO date-time -> "10 Oct 2026".
String formatDate(String? value) {
  final parsed = value == null ? null : DateTime.tryParse(value);
  return parsed == null ? '—' : _date.format(parsed.toLocal());
}

String formatDateTime(String? value) {
  final parsed = value == null ? null : DateTime.tryParse(value);
  return parsed == null ? '—' : _dateTime.format(parsed.toLocal());
}

/// DateTime -> "yyyy-MM-dd" for DateOnly API fields.
String toApiDate(DateTime date) => _apiDate.format(date);
