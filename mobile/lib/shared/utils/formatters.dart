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
final _usdWhole = NumberFormat.currency(
  locale: 'en_US',
  symbol: 'USD ',
  decimalDigits: 0,
);
final _date = DateFormat('d MMM yyyy');
final _shortDate = DateFormat('d MMM');
final _dateTime = DateFormat('d MMM yyyy, HH:mm');
final _apiDate = DateFormat('yyyy-MM-dd');

String formatLkr(num amount) => _lkr.format(amount);
String formatUsd(num amount) => _usd.format(amount);

/// "USD 1,240" (rounded to whole dollars) for prices on cards.
String formatUsdWhole(num amount) => _usdWhole.format(amount);

/// "2026-10-10" or an ISO date-time -> "10 Oct 2026".
String formatDate(String? value) {
  final parsed = value == null ? null : DateTime.tryParse(value);
  return parsed == null ? '—' : _date.format(parsed.toLocal());
}

/// "2026-10-10" -> "10 Oct" (for short lines such as "starts 10 Oct").
String formatShortDate(String? value) {
  final parsed = value == null ? null : DateTime.tryParse(value);
  return parsed == null ? '—' : _shortDate.format(parsed.toLocal());
}

String formatDateTime(String? value) {
  final parsed = value == null ? null : DateTime.tryParse(value);
  return parsed == null ? '—' : _dateTime.format(parsed.toLocal());
}

/// DateTime -> "yyyy-MM-dd" for DateOnly API fields.
String toApiDate(DateTime date) => _apiDate.format(date);
