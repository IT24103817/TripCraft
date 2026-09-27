import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../shared/utils/friendly_error.dart';
import '../../shared/utils/validators.dart';
import '../../shared/theme/app_theme.dart';
import '../../shared/widgets/app_text_field.dart';
import '../../shared/widgets/brand_mark.dart';
import '../../shared/widgets/primary_button.dart';
import '../api/user_facing_exception.dart';
import '../router/routes.dart';
import 'auth_notifier.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key, this.registeredEmail});

  /// Pre-fills the email after a successful registration.
  final String? registeredEmail;

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _form = GlobalKey<FormState>();
  late final _email = TextEditingController(text: widget.registeredEmail);
  final _password = TextEditingController();
  bool _loading = false;
  String? _error;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_form.currentState!.validate()) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      // On success the auth state changes and the router redirects by role.
      await ref
          .read(authNotifierProvider.notifier)
          .login(_email.text, _password.text);
    } catch (error) {
      // A 401 from /api/auth/login means wrong credentials.
      final wrongCredentials =
          error is UserFacingException && error.isUnauthorized;
      setState(
        () => _error = wrongCredentials
            ? 'Invalid email or password.'
            : friendlyMessage(error),
      );
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Form(
                key: _form,
                child: AutofillGroup(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const Align(
                        alignment: Alignment.centerLeft,
                        child: BrandMark(),
                      ),
                      const SizedBox(height: 32),
                      Text(
                        'Welcome back',
                        style: Theme.of(context).textTheme.headlineSmall,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Sign in to plan your Sri Lanka trip.',
                        style: Theme.of(context).textTheme.bodyLarge
                            ?.copyWith(color: AppColors.muted),
                      ),
                      const SizedBox(height: 24),
                      if (_error != null) ...[
                        Semantics(
                          liveRegion: true,
                          child: Text(
                            _error!,
                            style: TextStyle(
                              color: Theme.of(context).colorScheme.error,
                            ),
                          ),
                        ),
                        const SizedBox(height: 12),
                      ],
                      AppTextField(
                        label: 'Email',
                        controller: _email,
                        keyboardType: TextInputType.emailAddress,
                        autofillHints: const [AutofillHints.email],
                        textInputAction: TextInputAction.next,
                        validator: Validators.email,
                      ),
                      const SizedBox(height: 12),
                      AppTextField(
                        label: 'Password',
                        controller: _password,
                        obscureText: true,
                        autofillHints: const [AutofillHints.password],
                        validator: (v) => Validators.required(v, 'Password'),
                      ),
                      const SizedBox(height: 20),
                      PrimaryButton(
                        label: 'Sign in',
                        loading: _loading,
                        onPressed: _submit,
                      ),
                      TextButton(
                        onPressed: _loading
                            ? null
                            : () => context.go(Routes.register),
                        child: const Text('New here? Create an account'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
