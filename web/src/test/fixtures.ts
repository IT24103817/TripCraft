import type { PagedResult } from '@/shared/api/types';

export function paged<T>(items: T[], total = items.length, page = 1, pageSize = 20): PagedResult<T> {
  return { items, total, page, pageSize };
}

export function trip(overrides: Record<string, unknown> = {}) {
  return {
    id: '11111111-1111-1111-1111-111111111111',
    touristId: '22222222-2222-2222-2222-222222222222',
    objective: '5 days for 4 people in Kandy and Ella with the hill-country train',
    startDate: '2026-10-10',
    endDate: '2026-10-14',
    pax: 4,
    budgetUsd: 1500,
    preferences: { language: 'en', transport: 'train' },
    status: 'Submitted',
    createdAt: '2026-09-26T08:00:00Z',
    updatedAt: '2026-09-26T08:00:00Z',
    ...overrides,
  };
}

export const WORKFLOW_ID = '33333333-3333-3333-3333-333333333333';
export const QUOTATION_ID = '44444444-4444-4444-4444-444444444444';

/** A PendingApproval workflow as GET /api/workflows/{id} returns it (the PLAN.md section 6 demo). */
export function pendingWorkflow(overrides: Record<string, unknown> = {}) {
  return {
    id: WORKFLOW_ID,
    tripRequestId: trip().id,
    status: 'PendingApproval',
    currentStep: 'awaiting-manager',
    plan: {
      plan: [{ step: 1, agent: 'itinerary', task: 'Pick attractions' }],
      constraints: { cities: ['Kandy', 'Ella'] },
    },
    validationResult: { isValid: true, violations: [], hasHard: false, hasSoft: false },
    finalOutcome: {
      proposal: {
        days: [
          {
            day: 1,
            date: '2026-10-10',
            city: 'Kandy',
            stops: [{ attraction_id: 'a1', name: 'Temple of the Tooth', entry_fee_lkr: 2000 }],
            transport: 'road',
            transfer_km: 0,
            driving_minutes: 0,
            weather: 'light rain',
          },
          {
            day: 2,
            date: '2026-10-11',
            city: 'Ella',
            stops: [{ attraction_id: 'a2', name: 'Nine Arches Bridge', entry_fee_lkr: 0 }],
            transport: 'train',
            transfer_km: 140,
            driving_minutes: 0,
            weather: null,
          },
        ],
        resources: {
          guide_id: 'guide-1',
          vehicle_id: 'vehicle-1',
          rooms: [
            { hotel_id: 'hotel-1', room_type_id: 'room-std', night: '2026-10-10' },
            { hotel_id: 'hotel-1', room_type_id: 'room-std', night: '2026-10-10' },
          ],
          gaps: [],
        },
        quotation: {
          lines: [
            { line_type: 'guide', description: 'Guide', qty: 5, unit_lkr: 6000, amount_lkr: 30000 },
            { line_type: 'room', description: 'Standard double', qty: 8, unit_lkr: 12000, amount_lkr: 96000 },
          ],
          subtotal_lkr: 162800,
          margin_pct: 15,
          margin_lkr: 24420,
          total_lkr: 187220,
          fx_rate: 300,
          fx_as_of: '2026-10-01T00:00:00Z',
          fx_stale: false,
          total_usd: 624.07,
        },
        agentViolations: [],
        replans: 0,
        quotationId: QUOTATION_ID,
      },
      decision: null,
    },
    errorSummary: null,
    startedAt: '2026-09-26T08:00:00Z',
    finishedAt: null,
    elapsedMs: 42000,
    stepCount: 4,
    totalStepDurationMs: 38000,
    resourceNames: {
      'guide-1': 'Nimal Perera',
      'vehicle-1': 'Van CAB-1234',
      'room-std': 'Kandy Hills — Standard Double',
    },
    ...overrides,
  };
}

export function step(stepNo: number, agentName: string, overrides: Record<string, unknown> = {}) {
  return {
    id: `step-${stepNo}`,
    stepNo,
    agentName,
    toolName: 'get_attractions,get_distance',
    inputSummary: {
      summary: { cities: ['Kandy', 'Ella'] },
      toolCalls: [
        { tool: 'get_attractions', args: { city: 'Kandy' }, ok: true, duration_ms: 120, error: null },
        {
          tool: 'get_distance',
          args: { from_city: 'Kandy', to_city: 'Ella' },
          ok: true,
          duration_ms: 80,
          error: null,
        },
      ],
    },
    outputSummary: { days: 5 },
    validationResult: { ok: true },
    durationMs: 2300,
    retries: 0,
    status: 'Succeeded',
    createdAt: '2026-09-26T08:00:10Z',
    ...overrides,
  };
}

/** One row of GET /api/workflows (WorkflowSummaryDto). */
export function workflowSummary(overrides: Record<string, unknown> = {}) {
  return {
    id: WORKFLOW_ID,
    tripRequestId: trip().id,
    status: 'PendingApproval',
    currentStep: 'awaiting-manager',
    startedAt: '2026-09-26T08:00:00Z',
    finishedAt: '2026-09-26T08:01:00Z',
    errorSummary: null,
    objective: trip().objective,
    ...overrides,
  };
}

/** An attraction as GET /api/attractions returns it. */
export function attraction(id: string, name: string, city = 'Kandy') {
  return {
    id,
    name,
    city,
    category: 'Culture',
    durationMinutes: 90,
    entryFeeLkr: 2000,
    latitude: 7.29,
    longitude: 80.64,
  };
}

/** GET /api/trip-requests/{id}/itinerary: two days, Kandy then Ella. */
export function itinerary(overrides: Record<string, unknown> = {}) {
  return {
    id: '55555555-5555-5555-5555-555555555555',
    tripRequestId: trip().id,
    version: 1,
    generatedBy: 'Agent',
    days: [
      {
        dayNumber: 1,
        city: 'Kandy',
        hotelId: null,
        notes: 'Arrive and settle in',
        stops: [
          {
            sequence: 1,
            arrivalTime: null,
            attractionId: 'a1',
            attractionName: 'Temple of the Tooth',
            durationMinutes: 90,
          },
        ],
      },
      {
        dayNumber: 2,
        city: 'Ella',
        hotelId: null,
        notes: null,
        stops: [
          {
            sequence: 1,
            arrivalTime: null,
            attractionId: 'e1',
            attractionName: 'Nine Arches Bridge',
            durationMinutes: 60,
          },
        ],
      },
    ],
    ...overrides,
  };
}
