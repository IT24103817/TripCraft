import { DayStopsForm } from '@/shared/components/DayStopsForm';
import { Dialog } from '@/shared/components/Dialog';
import { useToast } from '@/shared/components/Toast';
import { useCityAttractions, useEditProposalDay } from './reviewApi';
import type { ProposalDay } from './types';

interface Props {
  tripId: string;
  day: ProposalDay;
  onClose: () => void;
}

/**
 * "Edit directly" for one day of the proposal under review: 1–3 attractions of the day's city
 * (PUT /api/trip-requests/{id}/proposal/days/{n}). The proposal must then be re-priced before it is sent.
 * A proposal day has no notes, so the notes box is hidden.
 */
export function ProposalDayEditor({ tripId, day, onClose }: Props) {
  const toast = useToast();
  const save = useEditProposalDay(tripId);
  const attractions = useCityAttractions(day.city);

  return (
    <Dialog open title={`Edit day ${day.day} — ${day.city}`} onClose={onClose}>
      <DayStopsForm
        city={day.city}
        options={{
          items: attractions.data,
          isLoading: attractions.isLoading,
          isError: attractions.isError,
          error: attractions.error,
          refetch: attractions.refetch,
        }}
        initialIds={(day.stops ?? []).map((s) => s.attraction_id)}
        initialNotes=""
        showNotes={false}
        isPending={save.isPending}
        saveError={save.error}
        onCancel={onClose}
        onSave={(attractionIds) =>
          save.mutate(
            { dayNumber: day.day, attractionIds },
            {
              onSuccess: () => {
                toast.success(`Saved day ${day.day}. Re-price before sending it to the client.`);
                onClose();
              },
            },
          )
        }
      />
    </Dialog>
  );
}
