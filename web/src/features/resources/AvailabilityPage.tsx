import { PlaceholderPage } from '@/shared/components/PlaceholderPage';

export default function AvailabilityPage() {
  return (
    <PlaceholderPage
      title="Availability calendar"
      component="Resource Management"
      owner="Student B"
      apis={['GET /api/availability?type=&from=&to=&language=', 'resource holds per resource and date range']}
    />
  );
}
