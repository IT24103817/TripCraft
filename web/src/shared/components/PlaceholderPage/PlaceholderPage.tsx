import { Card } from '../Card';
import { PageHeader } from '../PageHeader';
import type { PlaceholderPageProps } from './PlaceholderPage.types';

/**
 * Shown for screens whose API is not merged yet. It says exactly which endpoints are missing
 * instead of showing invented data.
 */
export const PlaceholderPage = ({ title, component, owner, apis }: PlaceholderPageProps) => (
  <section>
    <PageHeader title={title} />
    <Card className="space-y-2 border-dashed text-sm text-slate-700">
      <p className="font-medium text-slate-900">Available when {component} is merged</p>
      <p>
        This screen is owned by {owner}. It needs these API endpoints, which do not exist in the backend yet:
      </p>
      <ul className="list-inside list-disc font-mono text-xs">
        {apis.map((api) => (
          <li key={api}>{api}</li>
        ))}
      </ul>
    </Card>
  </section>
);
