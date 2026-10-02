export interface StatusBadgeProps {
  /** Any status from the PLAN.md workflows, e.g. "PendingApproval". Shown as "Pending approval". */
  status: string;
  /**
   * Replaces the generated text when one status name means something else on a page (e.g. a quotation that is
   * "Approved" was sent to the client). The tone still comes from `status`.
   */
  label?: string;
  className?: string;
}
