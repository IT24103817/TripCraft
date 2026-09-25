import { PlaceholderPage } from '@/shared/components/PlaceholderPage';

export default function GuidesPage() {
  return (
    <PlaceholderPage
      title="Guides"
      component="Resource Management"
      owner="Student B"
      apis={['GET/POST /api/guides', 'PUT/DELETE /api/guides/{id}', 'GET /api/guides/{id}/schedule']}
    />
  );
}
