import type { ProposalValidationResult } from './types';
import { VALIDATION_RULES } from './validationRules';

const KNOWN_CODES = new Set(VALIDATION_RULES.flatMap((r) => r.codes));

/** Set by the server when the agents failed before a proposal existed, so no rule was run. */
const AGENT_FAILED = 'AGENT_FAILED';

export function ValidationChecklist({ result }: { result: ProposalValidationResult }) {
  const extra = result.violations.filter((v) => !KNOWN_CODES.has(v.code));
  const notChecked = result.violations.some((v) => v.code === AGENT_FAILED);
  return (
    <div>
      <p className="mb-2 text-sm font-medium">
        {result.isValid ? (
          <span className="text-green-700">All deterministic checks passed.</span>
        ) : (
          <span className="text-red-700">
            {result.violations.length} check{result.violations.length === 1 ? '' : 's'} failed
            {result.hasHard
              ? ' (hard: cannot be approved)'
              : ' (soft: needs a revision or a manager decision)'}
            .
            {notChecked && ' The agents stopped before a proposal existed, so no rule was checked.'}
          </span>
        )}
      </p>
      <ul aria-label="Validation checklist" className="space-y-1 text-sm">
        {VALIDATION_RULES.map((rule) => {
          const failures = result.violations.filter((v) => rule.codes.includes(v.code));
          const passed = failures.length === 0;
          if (notChecked) {
            return (
              <li key={rule.label} className="flex gap-2 text-slate-500">
                <span aria-hidden="true">–</span>
                <span>
                  {rule.label}
                  <span className="sr-only"> — not checked</span>
                </span>
              </li>
            );
          }
          return (
            <li key={rule.label} className="flex gap-2">
              <span aria-hidden="true" className={passed ? 'text-green-600' : 'text-red-600'}>
                {passed ? '✔' : '✖'}
              </span>
              <span>
                <span className={passed ? 'text-slate-700' : 'font-medium text-red-800'}>{rule.label}</span>
                <span className="sr-only">{passed ? ' — passed' : ' — failed'}</span>
                {failures.map((f, i) => (
                  <span key={i} className="block text-xs text-red-700">
                    {f.severity}: {f.message}
                  </span>
                ))}
              </span>
            </li>
          );
        })}
        {extra.map((v, i) => (
          <li key={`extra-${i}`} className="flex gap-2">
            <span aria-hidden="true" className="text-red-600">
              ✖
            </span>
            <span className="font-medium text-red-800">
              {v.code}: {v.message}
              <span className="sr-only"> — failed</span>
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
