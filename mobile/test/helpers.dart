import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_riverpod/misc.dart' show Override;
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/api_client.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';
import 'package:tripcraft_mobile/shared/utils/clock.dart';

class MockApiClient extends Mock implements ApiClient {}

/// The two phone sizes the layouts are checked at (logical pixels).
final phoneSizes = ValueVariant<Size>({
  const Size(360, 640),
  const Size(412, 915),
});

void usePhoneSize(WidgetTester tester, Size size) {
  tester.view.devicePixelRatio = 3;
  tester.view.physicalSize = size * 3;
  addTearDown(tester.view.reset);
}

/// Pumps [child] inside a ProviderScope where the API client is [api] and storage is in memory.
Future<void> pumpScreen(
  WidgetTester tester,
  Widget child, {
  required MockApiClient api,
  InMemorySessionStorage? storage,
  List<dynamic> overrides = const [],
}) async {
  await tester.pumpWidget(
    ProviderScope(
      retry: (_, _) => null,
      overrides: [
        apiClientProvider.overrideWithValue(api),
        sessionStorageProvider.overrideWithValue(
          storage ?? InMemorySessionStorage(),
        ),
        ...overrides.cast(),
      ],
      child: MaterialApp(theme: buildAppTheme(), home: child),
    ),
  );
}

/// Fixes "now" for screens that depend on the date or time (greeting, "today's trip", "day N of M").
Override fixedClock(DateTime now) => clockProvider.overrideWithValue(() => now);

/// The first vertical list on screen (a tab view's pages scroll sideways, so `Scrollable.first` is not enough).
Finder verticalScrollable() => find
    .byWidgetPredicate(
      (w) => w is Scrollable && w.axisDirection == AxisDirection.down,
    )
    .first;

Map<String, dynamic> pagedJson(List<Map<String, dynamic>> items) => {
  'items': items,
  'page': 1,
  'pageSize': 100,
  'total': items.length,
};

Map<String, dynamic> tripJson({
  String id = 'trip-1',
  String status = 'Submitted',
  String objective = '5 days in Kandy and Ella with the train',
  List<String> cities = const ['Kandy', 'Ella'],
}) => {
  'id': id,
  'touristId': 'tourist-1',
  'objective': objective,
  'startDate': '2026-10-10',
  'endDate': '2026-10-14',
  'pax': 4,
  'budgetUsd': 1500,
  'preferences': {'transport': 'train'},
  'status': status,
  'createdAt': '2026-09-26T08:00:00Z',
  'updatedAt': '2026-09-26T08:00:00Z',
  'cities': cities,
};

/// The cities GET /api/attractions/cities returns in the demo data.
const demoCities = [
  'Colombo',
  'Ella',
  'Galle',
  'Kandy',
  'Nuwara Eliya',
  'Sigiriya',
];

/// A GET /api/trip-requests/{id}/cancellation body.
Map<String, dynamic> cancellationJson({
  bool canCancel = true,
  String? closedReason,
  String operatorContact = 'operations@tripcraft.test',
}) => {
  'canCancel': canCancel,
  'cancelUntil': '2026-10-07',
  'cutoffDays': 3,
  'closedReason': closedReason,
  'operatorContact': operatorContact,
};
