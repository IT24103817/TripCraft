import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';
import 'package:tripcraft_mobile/shared/utils/statuses.dart';
import 'package:tripcraft_mobile/shared/widgets/status_chip.dart';

void main() {
  test('every v1.1 trip status has the colour from the lifecycle', () {
    expect(statusColor('Submitted'), AppColors.neutral);
    expect(statusColor('Planning'), AppColors.info);
    expect(statusColor('PendingReview'), AppColors.warning);
    expect(statusColor('QuotationSent'), AppColors.accent);
    expect(statusColor('ClientAccepted'), AppColors.success);
    expect(statusColor('Confirmed'), AppColors.success);
    expect(statusColor('InProgress'), AppColors.info);
    expect(statusColor('Completed'), AppColors.success);
    expect(statusColor('RevisionRequested'), AppColors.purple);
    expect(statusColor('FailedSafely'), AppColors.danger);
    expect(statusColor('Cancelled'), AppColors.neutral);
    expect(statusColor('SomethingNew'), AppColors.neutral);
  });

  test('workflow statuses keep their colours', () {
    expect(statusColor('PendingApproval'), AppColors.warning);
    expect(statusColor('Approved'), AppColors.success);
    expect(statusColor('Rejected'), AppColors.danger);
  });

  test(
    'the old PendingApproval / Approved / Rejected are not trip statuses',
    () {
      expect(tripStatuses, isNot(contains('PendingApproval')));
      expect(tripStatuses, isNot(contains('Approved')));
      expect(tripStatuses, isNot(contains('Rejected')));
      expect(tripStatuses, hasLength(11));
    },
  );

  test('labels are readable', () {
    expect(statusLabel('PendingReview'), 'Pending review');
    expect(statusLabel('QuotationSent'), 'Quotation sent');
    expect(statusLabel('ClientAccepted'), 'Accepted');
    expect(statusLabel('InProgress'), 'In progress');
    expect(statusLabel('RevisionRequested'), 'Revision requested');
    expect(statusLabel('FailedSafely'), 'Planning failed');
    expect(statusLabel('Confirmed'), 'Confirmed');
  });

  testWidgets('StatusChip shows the label in the status colour', (
    tester,
  ) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(body: StatusChip(status: 'PendingReview')),
      ),
    );

    final text = tester.widget<Text>(find.text('Pending review'));
    expect(text.style!.color, AppColors.warning);
    expect(find.bySemanticsLabel('Status: Pending review'), findsOneWidget);
  });
}
