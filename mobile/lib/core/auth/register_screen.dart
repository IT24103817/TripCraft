import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../shared/utils/friendly_error.dart';
import '../../shared/utils/validators.dart';
import '../../shared/theme/app_theme.dart';
import '../../shared/widgets/app_text_field.dart';
import '../../shared/widgets/primary_button.dart';
import '../router/routes.dart';
import 'auth_notifier.dart';
import 'auth_repository.dart';

/// Tourist self-registration (the API always creates a Tourist). Signs in straight after.
class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _form = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _nationality = TextEditingController();
  bool _loading = false;
  String? _error;

  @override
  void dispose() {
    for (final c in [_name, _email, _password, _nationality]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_form.currentState!.validate()) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      await ref
          .read(authRepositoryProvider)
          .register(
            fullName: _name.text,
            email: _email.text,
            password: _password.text,
            nationality: _nationality.text,
          );
      await ref
          .read(authNotifierProvider.notifier)
          .login(_email.text, _password.text);
    } catch (error) {
      setState(() => _error = friendlyMessage(error));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Create account')),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Form(
            key: _form,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  'Create a tourist account to request a custom trip.',
                  style: Theme.of(context).textTheme.bodyLarge
                      ?.copyWith(color: AppColors.muted),
                ),
                const SizedBox(height: 20),
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
                  label: 'Full name',
                  controller: _name,
                  autofillHints: const [AutofillHints.name],
                  validator: (v) => Validators.required(v, 'Full name'),
                ),
                const SizedBox(height: 12),
                AppTextField(
                  label: 'Email',
                  controller: _email,
                  keyboardType: TextInputType.emailAddress,
                  autofillHints: const [AutofillHints.email],
                  validator: Validators.email,
                ),
                const SizedBox(height: 12),
                AppTextField(
                  label: 'Password',
                  controller: _password,
                  obscureText: true,
                  hint: '8+ characters, upper-case, lower-case and a digit',
                  autofillHints: const [AutofillHints.newPassword],
                  validator: Validators.strongPassword,
                ),
                const SizedBox(height: 12),
                AppTextField(
                  label: 'Nationality',
                  controller: _nationality,
                  hint: 'e.g. United Kingdom',
                  validator: (v) => Validators.required(v, 'Nationality'),
                ),
                const SizedBox(height: 20),
                PrimaryButton(
                  label: 'Create account',
                  loading: _loading,
                  onPressed: _submit,
                ),
                TextButton(
                  onPressed: _loading ? null : () => context.go(Routes.login),
                  child: const Text('Already have an account? Sign in'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
