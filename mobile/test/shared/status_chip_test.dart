import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';
import 'package:tripcraft_mobile/shared/utils/statuses.dart';
import 'package:tripcraft_mobile/shared/widgets/status_chip.dart';

void main() {
  test('every v1.1 trip status has the colour from the lifecycle', () {
    expect(statusColor('Submitted'), AppColors.neutral);
    expect(statusColor('Planning'), AppColors.info);
    expect(statusColor('QuotationSent'), AppColors.accent);
    expect(statusColor('ClientAccepted'), AppColors.success);
    expect(statusColor('Confirmed'), AppColors.success);
    expect(statusColor('InProgress'), AppColors.info);
    expect(statusColor('Completed'), AppColors.success);
    expect(statusColor('ClientDeclined'), AppColors.danger);
    expect(statusColor('NeedsOperator'), AppColors.warning);
    expect(statusColor('Cancelled'), AppColors.neutral);
    expect(statusColor('SomethingNew'), AppColors.neutral);
  });

  test('workflow statuses keep their colours', () {
    expect(statusColor('PendingApproval'), AppColors.warning);
    expect(statusColor('Approved'), AppColors.success);
    expect(statusColor('Rejected'), AppColors.danger);
    expect(
      statusColor('RevisionRequested'),
      AppColors.neutral,
      reason: 'retired in v1.1, so it is an unknown status',
    );
    expect(statusColor('FailedSafely'), AppColors.danger);
  });

  test(
    'the trip statuses are the new lifecycle, without an operator review',
    () {
      expect(tripStatuses, [
        'Submitted',
        'Planning',
        'QuotationSent',
        'ClientAccepted',
        'Confirmed',
        'InProgress',
        'Completed',
        'ClientDeclined',
        'NeedsOperator',
        'Cancelled',
      ]);
      for (final gone in [
        'PendingReview',
        'RevisionRequested',
        'FailedSafely',
        'PendingApproval',
        'Approved',
        'Rejected',
      ]) {
        expect(tripStatuses, isNot(contains(gone)), reason: gone);
      }
    },
  );

  test('labels are readable', () {
    expect(statusLabel('QuotationSent'), 'Quotation sent');
    expect(statusLabel('ClientAccepted'), 'Accepted');
    expect(statusLabel('ClientDeclined'), 'Declined');
    expect(statusLabel('NeedsOperator'), 'Needs operator');
    expect(statusLabel('InProgress'), 'In progress');
    expect(statusLabel('Confirmed'), 'Confirmed');
    // Workflow status, still shown on the workflow chip.
    expect(statusLabel('FailedSafely'), 'Planning failed');
  });

  testWidgets('StatusChip shows the label in the status colour', (
    tester,
  ) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(body: StatusChip(status: 'NeedsOperator')),
      ),
    );

    final text = tester.widget<Text>(find.text('Needs operator'));
    expect(text.style!.color, AppColors.warning);
    expect(find.bySemanticsLabel('Status: Needs operator'), findsOneWidget);
  });
}
