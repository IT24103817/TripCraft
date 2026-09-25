import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';
import 'package:tripcraft_mobile/shared/utils/statuses.dart';
import 'package:tripcraft_mobile/shared/widgets/status_chip.dart';

void main() {
  test('every trip status has the colour from the status workflow', () {
    expect(statusColor('Submitted'), AppColors.neutral);
    expect(statusColor('Planning'), AppColors.info);
    expect(statusColor('PendingApproval'), AppColors.warning);
    expect(statusColor('RevisionRequested'), AppColors.purple);
    expect(statusColor('Confirmed'), AppColors.success);
    expect(statusColor('Approved'), AppColors.success);
    expect(statusColor('Completed'), AppColors.success);
    expect(statusColor('Rejected'), AppColors.danger);
    expect(statusColor('FailedSafely'), AppColors.danger);
    expect(statusColor('InProgress'), AppColors.info);
    expect(statusColor('Cancelled'), AppColors.neutral);
    expect(statusColor('SomethingNew'), AppColors.neutral);
  });

  test('labels are readable', () {
    expect(statusLabel('PendingApproval'), 'Pending approval');
    expect(statusLabel('RevisionRequested'), 'Revision requested');
    expect(statusLabel('Confirmed'), 'Confirmed');
  });

  testWidgets('StatusChip shows the label in the status colour', (
    tester,
  ) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(body: StatusChip(status: 'PendingApproval')),
      ),
    );

    final text = tester.widget<Text>(find.text('Pending approval'));
    expect(text.style!.color, AppColors.warning);
    expect(find.bySemanticsLabel('Status: Pending approval'), findsOneWidget);
  });
}
