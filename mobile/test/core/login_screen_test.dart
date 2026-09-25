import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/core/auth/login_screen.dart';

import '../helpers.dart';

void main() {
  testWidgets('shows validation errors and does not call the API', (
    tester,
  ) async {
    final size = phoneSizes.currentValue!;
    usePhoneSize(tester, size);
    final api = MockApiClient();
    await pumpScreen(tester, const LoginScreen(), api: api);

    await tester.tap(find.text('Sign in'));
    await tester.pump();

    expect(find.text('Email is required.'), findsOneWidget);
    expect(find.text('Password is required.'), findsOneWidget);

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Email'),
      'not-an-email',
    );
    await tester.tap(find.text('Sign in'));
    await tester.pump();

    expect(find.text('Enter a valid email address.'), findsOneWidget);
    verifyNever(() => api.post(any(), body: any(named: 'body')));
  }, variant: phoneSizes);

  testWidgets('shows an error when the credentials are wrong', (tester) async {
    final api = MockApiClient();
    when(() => api.post('/api/auth/login', body: any(named: 'body'))).thenThrow(
      const UserFacingException(
        'Your session has expired. Please log in again.',
        statusCode: 401,
      ),
    );
    await pumpScreen(tester, const LoginScreen(), api: api);

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Email'),
      'tourist1@tripcraft.test',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Password'),
      'wrong',
    );
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();

    expect(find.text('Invalid email or password.'), findsOneWidget);
  });
}
