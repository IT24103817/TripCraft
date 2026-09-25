import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/core/api/api_client.dart';
import 'package:tripcraft_mobile/core/auth/auth_repository.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';

import '../helpers.dart';

/// PLAN.md section 11, Flutter: "secure storage read" — the JWT and user survive an app restart.
void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUp(() => FlutterSecureStorage.setMockInitialValues({}));

  test('writes the session to secure storage and reads it back', () async {
    final storage = SecureSessionStorage();

    await storage.writeSession(
      token: 'jwt-abc',
      user: {'id': 'u1', 'role': 'Tourist'},
    );

    expect(await storage.readToken(), 'jwt-abc');
    expect(await storage.readUser(), {'id': 'u1', 'role': 'Tourist'});
  });

  test(
    'restore signs the user back in from what a previous run stored',
    () async {
      FlutterSecureStorage.setMockInitialValues({
        'auth.token': 'jwt-from-last-run',
        'auth.user': '{"id":"u1","email":"tourist1@tripcraft.test","fullName":"Demo Tourist 1","role":"Tourist","isActive":true}',
      });
      final repository = AuthRepository(
        MockApiClient() as ApiClient,
        SecureSessionStorage(),
      );

      final user = await repository.restore();

      expect(user?.email, 'tourist1@tripcraft.test');
      expect(user?.role, 'Tourist');
    },
  );

  test('logout clears the token but keeps the nationality pre-fill', () async {
    final storage = SecureSessionStorage();
    await storage.writeSession(token: 'jwt', user: {'id': 'u1'});
    await storage.writeNationality('United Kingdom');

    await storage.clear();

    expect(await storage.readToken(), isNull);
    expect(await storage.readUser(), isNull);
    expect(await storage.readNationality(), 'United Kingdom');
  });
}
