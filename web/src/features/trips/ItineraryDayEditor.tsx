import { DayStopsForm } from '@/shared/components/DayStopsForm';
import { Dialog } from '@/shared/components/Dialog';
import { useToast } from '@/shared/components/Toast';
import { useAttractions, useUpdateItineraryDay } from './api';
import type { ItineraryDayDto } from './types';

interface Props {
  tripId: string;
  day: ItineraryDayDto;
  onClose: () => void;
}

/**
 * Itinerary editor for one day of a Confirmed trip: pick 1–3 attractions of the day's city and edit the
 * notes. Rendered only while open, so the attraction list is fetched only when someone edits a day.
 */
export function ItineraryDayEditor({ tripId, day, onClose }: Props) {
  const toast = useToast();
  const save = useUpdateItineraryDay(tripId);
  const attractions = useAttractions({ city: day.city, sort: 'name', page: 1, pageSize: 100 });

  return (
    <Dialog open title={`Edit day ${day.dayNumber} — ${day.city}`} onClose={onClose}>
      <DayStopsForm
        city={day.city}
        options={{
          items: attractions.data?.items,
          isLoading: attractions.isLoading,
          isError: attractions.isError,
          error: attractions.error,
          refetch: attractions.refetch,
        }}
        initialIds={day.stops.map((s) => s.attractionId)}
        initialNotes={day.notes ?? ''}
        isPending={save.isPending}
        saveError={save.error}
        onCancel={onClose}
        onSave={(attractionIds, notes) =>
          save.mutate(
            { dayNumber: day.dayNumber, body: { attractionIds, notes } },
            {
              onSuccess: () => {
                toast.success(`Saved day ${day.dayNumber}.`);
                onClose();
              },
            },
          )
        }
      />
    </Dialog>
  );
}
