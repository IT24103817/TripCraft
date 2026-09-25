import { PageHeader } from './PageHeader';

interface PlaceholderPageProps {
  title: string;
  component: string;
  owner: string;
  apis: string[];
}

/**
 * Shown for screens whose API is not merged yet. It says exactly which endpoints are missing
 * instead of showing invented data.
 */
export function PlaceholderPage({ title, component, owner, apis }: PlaceholderPageProps) {
  return (
    <section>
      <PageHeader title={title} />
      <div className="card space-y-2 border-dashed text-sm text-slate-700">
        <p className="font-medium text-slate-900">Available when {component} is merged</p>
        <p>
          This screen is owned by {owner}. It needs these API endpoints, which do not exist in the backend
          yet:
        </p>
        <ul className="list-inside list-disc font-mono text-xs">
          {apis.map((api) => (
            <li key={api}>{api}</li>
          ))}
        </ul>
      </div>
    </section>
  );
}
