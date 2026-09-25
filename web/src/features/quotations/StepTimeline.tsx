import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatDuration } from '@/shared/utils/format';
import type { AgentStepDto } from './types';

const AGENT_LABELS: Record<string, string> = {
  planner: 'Planner / Coordinator',
  itinerary: 'Itinerary Analysis',
  resources: 'Resource & Action',
  validation: 'Validation & Safety',
};

function Json({ value }: { value: unknown }) {
  return (
    <pre className="overflow-x-auto rounded bg-slate-50 p-2 text-xs text-slate-800">
      {JSON.stringify(value, null, 2)}
    </pre>
  );
}

/** One entry per agent step, in step order. Summaries only — prompts are never stored. */
export function StepTimeline({ steps }: { steps: AgentStepDto[] }) {
  return (
    <ol aria-label="Agent steps" className="relative space-y-4 border-l-2 border-slate-200 pl-5">
      {steps.map((step) => (
        <li key={step.id} className="relative">
          <span
            aria-hidden="true"
            className="absolute -left-[29px] top-1 h-3 w-3 rounded-full border-2 border-white bg-indigo-500"
          />
          <div className="card space-y-2">
            <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-sm">
              <span className="font-semibold text-slate-900">
                {step.stepNo}. {AGENT_LABELS[step.agentName] ?? step.agentName}
              </span>
              <StatusBadge status={step.status} />
              <span className="text-slate-600">{formatDuration(step.durationMs)}</span>
              <span className="text-slate-600">
                {step.retries} {step.retries === 1 ? 'retry' : 'retries'}
              </span>
            </div>
            {(step.inputSummary.toolCalls ?? []).length > 0 && (
              <ul aria-label={`Tool calls of step ${step.stepNo}`} className="flex flex-wrap gap-1 text-xs">
                {step.inputSummary.toolCalls?.map((call, i) => (
                  <li
                    key={i}
                    className={`rounded px-2 py-0.5 ${call.ok ? 'bg-slate-100 text-slate-700' : 'bg-red-100 text-red-800'}`}
                    title={call.error ?? undefined}
                  >
                    {call.tool} · {formatDuration(call.duration_ms)}
                    {!call.ok && ' · failed'}
                  </li>
                ))}
              </ul>
            )}
            <details className="text-sm">
              <summary className="cursor-pointer text-indigo-700">Summaries and validation result</summary>
              <div className="mt-2 grid gap-2 md:grid-cols-3">
                <div>
                  <p className="text-xs font-medium text-slate-500">Input</p>
                  <Json value={step.inputSummary.summary} />
                </div>
                <div>
                  <p className="text-xs font-medium text-slate-500">Output</p>
                  <Json value={step.outputSummary} />
                </div>
                <div>
                  <p className="text-xs font-medium text-slate-500">Validation</p>
                  <Json value={step.validationResult} />
                </div>
              </div>
            </details>
          </div>
        </li>
      ))}
    </ol>
  );
}
