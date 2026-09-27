import type { TripRequestStatus, WorkflowStatus } from '@/shared/statuses';

// Taken from backend/src/TripCraft.Application/Workflows (WorkflowDtos.cs, WorkflowOutcome.cs,
// ProposalValidationResult.cs, AgentProposalRequest.cs) and Quotations/QuotationDecisionRequests.cs.
// The proposal inside finalOutcome keeps the agent's snake_case field names.

export interface WorkflowSummaryDto {
  id: string;
  tripRequestId: string;
  status: WorkflowStatus;
  currentStep: string | null;
  startedAt: string;
  finishedAt: string | null;
  errorSummary: string | null;
}

export interface WorkflowListQuery {
  status?: string;
  page: number;
  pageSize: number;
}

export type ViolationSeverity = 'Hard' | 'Soft';

export interface ProposalRuleViolation {
  code: string;
  message: string;
  severity: ViolationSeverity;
}

export interface ProposalValidationResult {
  isValid: boolean;
  violations: ProposalRuleViolation[];
  hasHard: boolean;
  hasSoft: boolean;
}

export interface ProposalStop {
  attraction_id: string;
  name: string;
  entry_fee_lkr: number;
}

export interface ProposalDay {
  day: number;
  date: string;
  city: string;
  stops: ProposalStop[] | null;
  transport: string;
  transfer_km: number;
  driving_minutes: number;
  weather: string | null;
}

export interface ProposalRoom {
  hotel_id: string;
  room_type_id: string;
  night: string;
}

export interface ProposalResources {
  guide_id: string | null;
  vehicle_id: string | null;
  rooms: ProposalRoom[] | null;
  gaps: string[] | null;
}

export interface ProposalQuotationLine {
  line_type: 'guide' | 'vehicle' | 'room' | 'entry' | string;
  description: string;
  qty: number;
  unit_lkr: number;
  amount_lkr: number;
}

export interface ProposalQuotation {
  lines: ProposalQuotationLine[] | null;
  subtotal_lkr: number;
  margin_pct: number;
  margin_lkr: number;
  total_lkr: number;
  fx_rate: number;
  fx_as_of: string;
  fx_stale: boolean;
  total_usd: number;
}

export interface StoredProposal {
  days: ProposalDay[] | null;
  resources: ProposalResources | null;
  quotation: ProposalQuotation | null;
  agentViolations: { code: string; message: string }[] | null;
  replans: number;
  quotationId: string | null;
}

export interface ResourceHold {
  type: 'Guide' | 'Vehicle' | 'Room';
  resourceId: string;
  tripRequestId: string;
  from: string;
  to: string;
  quantity: number;
}

export interface WorkflowDecision {
  decision: string;
  quotationId: string;
  decidedBy: string;
  decidedAt: string;
  comment: string | null;
  holds: ResourceHold[];
}

export interface WorkflowOutcome {
  proposal: StoredProposal;
  decision: WorkflowDecision | null;
}

export interface WorkflowDto {
  id: string;
  tripRequestId: string;
  status: WorkflowStatus;
  currentStep: string | null;
  plan: Record<string, unknown>;
  validationResult: ProposalValidationResult | null;
  finalOutcome: WorkflowOutcome | null;
  errorSummary: string | null;
  startedAt: string;
  finishedAt: string | null;
  elapsedMs: number | null;
  stepCount: number;
  totalStepDurationMs: number;
  /** Display names for the proposal's guide, vehicle and room-type ids. */
  resourceNames?: Record<string, string>;
}

export interface AgentToolCall {
  tool: string;
  args: Record<string, unknown> | null;
  ok: boolean;
  duration_ms: number;
  error: string | null;
}

/** inputSummary is stored as {summary, toolCalls} by WorkflowStepService. */
export interface AgentStepInputSummary {
  summary: Record<string, unknown> | null;
  toolCalls: AgentToolCall[] | null;
}

export interface AgentStepDto {
  id: string;
  stepNo: number;
  agentName: string;
  toolName: string | null;
  inputSummary: AgentStepInputSummary;
  outputSummary: Record<string, unknown>;
  validationResult: Record<string, unknown>;
  durationMs: number;
  retries: number;
  status: 'Succeeded' | 'Failed' | string;
  createdAt: string;
}

export interface QuotationDecisionResponse {
  quotationId: string;
  tripRequestId: string;
  workflowId: string;
  decision: string;
  tripStatus: string;
  workflowStatus: string;
  holdsCreated: number;
}

/** The fields of TripRequestDto this feature shows (same endpoint as the trips feature, no import). */
export interface TripSummary {
  id: string;
  objective: string;
  startDate: string;
  endDate: string;
  pax: number;
  budgetUsd: number;
  status: TripRequestStatus;
}

/** GET /api/quotations/{id} (camelCase, from the quotations table). */
export interface QuotationDto {
  id: string;
  tripRequestId: string;
  workflowId: string | null;
  version: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'RevisionRequested';
  subtotalLkr: number;
  marginPct: number;
  marginLkr: number;
  totalLkr: number;
  totalUsd: number;
  fxRate: number;
  fxAsOf: string;
  fxStale: boolean;
  acceptedAt: string | null;
  lines: { lineType: string; description: string; qty: number; unitLkr: number; amountLkr: number }[];
  decisions: { decision: string; comment: string | null; decidedAt: string }[];
  createdAt: string;
}

export interface RecalculationDto {
  quotation: QuotationDto;
  previousTotalLkr: number;
  previousTotalUsd: number;
  changed: boolean;
}

export interface RevenueMonthDto {
  month: string;
  quotations: number;
  totalLkr: number;
  totalUsd: number;
}

export interface UtilisationDto {
  resourceType: string;
  resourceId: string;
  name: string;
  heldDays: number;
  daysInRange: number;
  utilisationPct: number;
}

export interface StatusCountDto {
  status: string;
  count: number;
}
