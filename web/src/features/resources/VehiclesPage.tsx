import { PlaceholderPage } from '@/shared/components/PlaceholderPage';

export default function VehiclesPage() {
  return (
    <PlaceholderPage
      title="Vehicles"
      component="Resource Management"
      owner="Student B"
      apis={['GET/POST /api/vehicles', 'PUT/DELETE /api/vehicles/{id}']}
    />
  );
}
