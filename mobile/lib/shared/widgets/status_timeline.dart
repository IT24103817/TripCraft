import 'package:flutter/material.dart';

import '../theme/app_theme.dart';
import '../utils/statuses.dart';

/// Vertical list of steps. Steps before [current] are done, [current] is highlighted.
/// A [current] not in [steps] (e.g. Rejected) is shown after the last step as the outcome.
class StatusTimeline extends StatelessWidget {
  const StatusTimeline({super.key, required this.steps, required this.current});

  final List<String> steps;
  final String current;

  /// Index of the highlighted step, used by tests and screen readers.
  static int currentIndex(List<String> steps, String current) {
    final index = steps.indexOf(current);
    return index >= 0 ? index : steps.length;
  }

  @override
  Widget build(BuildContext context) {
    final currentAt = currentIndex(steps, current);
    final shown = currentAt < steps.length ? steps : [...steps, current];
    return Column(
      children: [
        for (var i = 0; i < shown.length; i++)
          _StepRow(
            // Key like 'step-PendingReview-current', so tests and tools can find a step's state.
            key: ValueKey('step-${shown[i]}-${_stateOf(i, currentAt).name}'),
            label: statusLabel(shown[i]),
            state: _stateOf(i, currentAt),
            isLast: i == shown.length - 1,
          ),
      ],
    );
  }
}

_StepState _stateOf(int index, int currentAt) => index < currentAt
    ? _StepState.done
    : (index == currentAt ? _StepState.current : _StepState.todo);

enum _StepState { done, current, todo }

class _StepRow extends StatelessWidget {
  const _StepRow({
    super.key,
    required this.label,
    required this.state,
    required this.isLast,
  });

  final String label;
  final _StepState state;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    final color = switch (state) {
      _StepState.done => AppColors.success,
      _StepState.current => AppColors.brand,
      _StepState.todo => AppColors.line,
    };
    return Semantics(
      label:
          '$label, ${switch (state) {
            _StepState.done => 'done',
            _StepState.current => 'current step',
            _StepState.todo => 'not reached',
          }}',
      excludeSemantics: true,
      child: IntrinsicHeight(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Column(
              children: [
                Container(
                  key: state == _StepState.current
                      ? const ValueKey('timeline-current')
                      : null,
                  width: 24,
                  height: 24,
                  decoration: BoxDecoration(
                    color: state == _StepState.todo ? Colors.white : color,
                    shape: BoxShape.circle,
                    border: Border.all(color: color, width: 2),
                  ),
                  child: state == _StepState.done
                      ? const Icon(Icons.check, size: 16, color: Colors.white)
                      : null,
                ),
                if (!isLast)
                  Expanded(
                    child: Container(
                      width: 2,
                      color: color,
                      constraints: const BoxConstraints(minHeight: 16),
                    ),
                  ),
              ],
            ),
            const SizedBox(width: 12),
            Padding(
              padding: const EdgeInsets.only(top: 2, bottom: 16),
              child: Text(
                label,
                style: TextStyle(
                  fontWeight: state == _StepState.current
                      ? FontWeight.w700
                      : FontWeight.w400,
                  color: state == _StepState.todo
                      ? AppColors.muted
                      : AppColors.ink,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
