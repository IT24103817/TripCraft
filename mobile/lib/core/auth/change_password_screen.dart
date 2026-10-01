import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../shared/theme/app_theme.dart';
import '../../shared/utils/friendly_error.dart';
import '../../shared/utils/validators.dart';
import '../../shared/widgets/app_text_field.dart';
import '../../shared/widgets/primary_button.dart';
import 'auth_notifier.dart';

/// Shown after a login with mustChangePassword (a guide's temporary password from the operator).
/// There is no back button: the router keeps the user here until the password is changed or they log out.
class ChangePasswordScreen extends ConsumerStatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  ConsumerState<ChangePasswordScreen> createState() =>
      _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends ConsumerState<ChangePasswordScreen> {
  final _form = GlobalKey<FormState>();
  final _current = TextEditingController();
  final _newPassword = TextEditingController();
  final _confirm = TextEditingController();
  bool _saving = false;
  String? _error;

  @override
  void dispose() {
    for (final c in [_current, _newPassword, _confirm]) {
      c.dispose();
    }
    super.dispose();
  }

  String? _confirmMatches(String? value) =>
      value == _newPassword.text ? null : 'The passwords do not match.';

  Future<void> _submit() async {
    if (!_form.currentState!.validate()) return;
    setState(() {
      _saving = true;
      _error = null;
    });
    try {
      // On success mustChangePassword is false and the router goes home.
      await ref
          .read(authNotifierProvider.notifier)
          .changePassword(_current.text, _newPassword.text);
    } catch (error) {
      if (mounted) setState(() => _error = friendlyMessage(error));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: false,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Change your password'),
          automaticallyImplyLeading: false,
          actions: [
            TextButton(
              onPressed: _saving
                  ? null
                  : () => ref.read(authNotifierProvider.notifier).logout(),
              child: const Text('Log out'),
            ),
          ],
        ),
        body: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Form(
              key: _form,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'Your account has a temporary password. Choose your own password to continue.',
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
                    label: 'Current password',
                    controller: _current,
                    obscureText: true,
                    autofillHints: const [AutofillHints.password],
                    validator: (v) =>
                        Validators.required(v, 'Current password'),
                  ),
                  const SizedBox(height: 12),
                  AppTextField(
                    label: 'New password',
                    controller: _newPassword,
                    obscureText: true,
                    hint: '8+ characters, upper-case, lower-case and a digit',
                    autofillHints: const [AutofillHints.newPassword],
                    validator: Validators.strongPassword,
                  ),
                  const SizedBox(height: 12),
                  AppTextField(
                    label: 'Confirm new password',
                    controller: _confirm,
                    obscureText: true,
                    validator: _confirmMatches,
                  ),
                  const SizedBox(height: 20),
                  PrimaryButton(
                    label: 'Change password',
                    loading: _saving,
                    onPressed: _submit,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
