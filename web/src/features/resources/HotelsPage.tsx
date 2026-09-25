import { PlaceholderPage } from '@/shared/components/PlaceholderPage';

export default function HotelsPage() {
  return (
    <PlaceholderPage
      title="Hotels and room types"
      component="Resource Management"
      owner="Student B"
      apis={['GET/POST /api/hotels', 'PUT/DELETE /api/hotels/{id}', 'GET/POST /api/hotels/{id}/room-types']}
    />
  );
}
