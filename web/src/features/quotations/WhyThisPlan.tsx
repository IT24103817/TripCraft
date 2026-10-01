import { usePlanExplanation } from './reviewApi';
import type { PlanExplanationItem } from './types';

/** A small picture per topic; the title next to it carries the meaning, so the icon is hidden from screen readers. */
const TOPIC_ICONS: Record<PlanExplanationItem['topic'], string> = {
  guide: '🧭',
  vehicle: '🚐',
  hotels: '🏨',
  driving: '🛣️',
  budget: '💰',
};

/**
 * "Why this plan": plain-language reasons for the guide, vehicle, hotels, driving times and the budget, so the
 * manager can check the agents' choices quickly. Hidden while the API has no proposal to explain (404).
 */
export function WhyThisPlan({ tripId }: { tripId: string }) {
  const explanation = usePlanExplanation(tripId);
  const items = explanation.data?.items ?? [];
  if (items.length === 0) return null;

  return (
    <section aria-labelledby="why-this-plan" className="card space-y-3">
      <h2 id="why-this-plan" className="font-semibold text-slate-900">
        Why this plan
      </h2>
      <ul aria-label="Reasons for this plan" className="grid gap-3 md:grid-cols-2">
        {items.map((item, index) => (
          <li key={index} className="flex gap-3 rounded-md border border-slate-200 p-3 text-sm">
            <span aria-hidden="true" className="text-xl leading-none">
              {TOPIC_ICONS[item.topic] ?? '•'}
            </span>
            <div>
              <h3 className="font-medium text-slate-900">{item.title}</h3>
              <p className="text-slate-700">{item.text}</p>
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
