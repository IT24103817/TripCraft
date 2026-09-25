import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/app.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/auth/auth_models.dart';
import 'package:tripcraft_mobile/core/router/auth_redirect.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';

import '../helpers.dart';

AsyncValue<AppUser?> signedIn(String role) => AsyncData(
  AppUser(
    id: 'u1',
    email: 'a@b.lk',
    fullName: 'Demo',
    role: role,
    isActive: true,
  ),
);

void main() {
  group('authRedirect', () {
    test(
      'signed out: every protected page goes to /login, public pages stay',
      () {
        expect(authRedirect(const AsyncData(null), '/trips'), '/login');
        expect(authRedirect(const AsyncData(null), '/schedule'), '/login');
        expect(authRedirect(const AsyncData(null), '/login'), isNull);
        expect(authRedirect(const AsyncData(null), '/register'), isNull);
      },
    );

    test('while the saved session is read, show the splash', () {
      expect(authRedirect(const AsyncLoading(), '/trips'), '/splash');
      expect(authRedirect(const AsyncLoading(), '/splash'), isNull);
    });

    test('each role lands on its own home', () {
      expect(authRedirect(signedIn('Tourist'), '/login'), '/trips');
      expect(authRedirect(signedIn('Guide'), '/login'), '/schedule');
      expect(
        authRedirect(signedIn('OperationsManager'), '/login'),
        '/not-supported',
      );
      expect(authRedirect(signedIn('Admin'), '/trips'), '/not-supported');
    });

    test('roles cannot open each other\'s areas', () {
      expect(authRedirect(signedIn('Tourist'), '/schedule'), '/trips');
      expect(authRedirect(signedIn('Guide'), '/trips/abc'), '/schedule');
      expect(authRedirect(signedIn('Tourist'), '/trips/abc'), isNull);
    });
  });

  testWidgets('the real app shows the login screen when nobody is signed in', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          apiClientProvider.overrideWithValue(MockApiClient()),
          sessionStorageProvider.overrideWithValue(InMemorySessionStorage()),
        ],
        child: const TripCraftApp(),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Sign in to plan your Sri Lanka trip.'), findsOneWidget);
  });
}
