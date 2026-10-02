import { useSearchParams } from 'react-router-dom';
import { PageHeader } from '@/shared/components/PageHeader';
import { toAttentionStatus } from './attentionApi';
import { AttentionQueue } from './AttentionQueue';

/**
 * Review queue (v1.1): quotations go to clients automatically, so this lists the trips that need the Operations
 * Manager — accepted (confirm), declined (replan or cancel) and needs operator (the agents could not finish).
 * The tab lives in the URL (?tab=ClientAccepted); an old tab value opens the first tab.
 */
export default function ApprovalsPage() {
  const [params, setParams] = useSearchParams();
  const tab = toAttentionStatus(params.get('tab'));

  return (
    <section className="space-y-4">
      <PageHeader
        title="Review queue"
        description="Trips that need you: accepted quotations to confirm, declined ones to replan, and trips the agents could not finish."
      />
      <AttentionQueue
        label="Trips that need you"
        selected={tab}
        onSelect={(next) => setParams({ tab: next }, { replace: true })}
      />
    </section>
  );
}
