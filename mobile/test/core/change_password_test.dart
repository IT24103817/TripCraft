import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/app.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';

import '../helpers.dart';

Map<String, dynamic> guideJson({required bool mustChange}) => {
  'id': 'u-guide',
  'email': 'guide.new@tripcraft.test',
  'fullName': 'New Guide',
  'role': 'Guide',
  'isActive': true,
  'mustChangePassword': mustChange,
};

void main() {
  late MockApiClient api;
  late InMemorySessionStorage storage;

  /// Starts the real app with a guide who signed in with a temporary password.
  Future<void> startApp(WidgetTester tester) async {
    api = MockApiClient();
    when(() => api.get('/api/guides/me/schedule')).thenAnswer(
      (_) async => {'guideId': 'g1', 'guideName': 'New Guide', 'trips': []},
    );
    when(() => api.get('/api/notifications/mine'))
        .thenAnswer((_) async => {'unreadCount': 0, 'items': <Object>[]});
    storage = InMemorySessionStorage()
      ..token = 'jwt-guide'
      ..user = guideJson(mustChange: true);
    await tester.pumpWidget(
      ProviderScope(
        retry: (_, _) => null,
        overrides: [
          apiClientProvider.overrideWithValue(api),
          sessionStorageProvider.overrideWithValue(storage),
        ],
        child: const TripCraftApp(),
      ),
    );
    await tester.pumpAndSettle();
  }

  Future<void> fill(
    WidgetTester tester, {
    required String current,
    required String next,
    required String confirm,
  }) async {
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Current password'),
      current,
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'New password'),
      next,
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Confirm new password'),
      confirm,
    );
    await tester.tap(find.text('Change password'));
    await tester.pumpAndSettle();
  }

  testWidgets(
    'a user with mustChangePassword lands on Change password, not home',
    (tester) async {
      await startApp(tester);

      expect(find.text('Change your password'), findsOneWidget);
      expect(find.text('My schedule'), findsNothing);
      expect(find.byType(NavigationBar), findsNothing);
    },
  );

  testWidgets(
    'the new password follows the strong-password rules and must match',
    (tester) async {
      await startApp(tester);

      await fill(tester, current: 'Temp-1234', next: 'short', confirm: 'short');
      expect(
        find.text('Password must be at least 8 characters.'),
        findsOneWidget,
      );

      await fill(
        tester,
        current: 'Temp-1234',
        next: 'alllowercase1',
        confirm: 'alllowercase1',
      );
      expect(
        find.text('Password must contain an upper-case letter.'),
        findsOneWidget,
      );

      await fill(
        tester,
        current: 'Temp-1234',
        next: 'NewPassword1',
        confirm: 'NewPassword2',
      );
      expect(find.text('The passwords do not match.'), findsOneWidget);
      verifyNever(() => api.post(any(), body: any(named: 'body')));
    },
  );

  testWidgets('a wrong current password shows the API message', (tester) async {
    await startApp(tester);
    when(() => api.post('/api/auth/change-password', body: any(named: 'body')))
        .thenThrow(
          const UserFacingException(
            'The current password is not correct.',
            statusCode: 400,
          ),
        );

    await fill(
      tester,
      current: 'wrong',
      next: 'NewPassword1',
      confirm: 'NewPassword1',
    );

    expect(find.text('The current password is not correct.'), findsOneWidget);
    expect(find.text('Change your password'), findsOneWidget);
  });

  testWidgets('after a successful change the app continues to home', (
    tester,
  ) async {
    await startApp(tester);
    when(() => api.post('/api/auth/change-password', body: any(named: 'body')))
        .thenAnswer((_) async => guideJson(mustChange: false));

    await fill(
      tester,
      current: 'Temp-1234',
      next: 'NewPassword1',
      confirm: 'NewPassword1',
    );

    verify(
      () => api.post(
        '/api/auth/change-password',
        body: {'currentPassword': 'Temp-1234', 'newPassword': 'NewPassword1'},
      ),
    ).called(1);
    expect(find.text('My schedule'), findsOneWidget);
    // The stored user no longer needs a change, and the token is kept.
    expect(storage.user?['mustChangePassword'], isFalse);
    expect(storage.token, 'jwt-guide');
  });
}
