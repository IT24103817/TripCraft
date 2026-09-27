import { useState, type FormEvent } from 'react';
import { getErrorMessage, getErrorStatus, getFieldError } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { PageState } from '@/shared/components/PageState';
import { useToast } from '@/shared/components/Toast';
import { useAttractions, useUpdateItineraryDay } from './api';
import type { ItineraryDayDto } from './types';

/** Operator rule (PLAN.md): at most 3 stops a day. The API checks it again. */
const MAX_STOPS = 3;
const MAX_NOTES = 500;

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
  // Stops are visited in the order they are ticked; the current stops keep their order.
  const [selected, setSelected] = useState<string[]>(day.stops.map((s) => s.attractionId));
  const [notes, setNotes] = useState(day.notes ?? '');
  const [noneSelected, setNoneSelected] = useState(false);

  const full = selected.length >= MAX_STOPS;

  const toggle = (id: string) => {
    setNoneSelected(false);
    setSelected((current) =>
      current.includes(id) ? current.filter((x) => x !== id) : [...current, id].slice(0, MAX_STOPS),
    );
  };

  const submit = (event: FormEvent) => {
    event.preventDefault();
    if (selected.length === 0) {
      setNoneSelected(true);
      return;
    }
    save.mutate(
      { dayNumber: day.dayNumber, body: { attractionIds: selected, notes: notes.trim() || null } },
      {
        onSuccess: () => {
          toast.success(`Saved day ${day.dayNumber}.`);
          onClose();
        },
      },
    );
  };

  // 400: messages per field; 409 (trip not Confirmed, no itinerary) and others: one message.
  const attractionsError = noneSelected
    ? 'Pick at least one attraction.'
    : getFieldError(save.error, 'attractionIds');
  const notesError = getFieldError(save.error, 'notes');
  const formError =
    save.isError && !(getErrorStatus(save.error) === 400 && (attractionsError || notesError))
      ? getErrorMessage(save.error)
      : null;

  return (
    <Dialog open title={`Edit day ${day.dayNumber} — ${day.city}`} onClose={onClose}>
      <form noValidate className="space-y-4" onSubmit={submit}>
        {formError && (
          <p role="alert" className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
            {formError}
          </p>
        )}

        <fieldset aria-describedby="day-stops-hint">
          <legend className="text-sm font-medium text-slate-700">
            Attractions in {day.city} (up to {MAX_STOPS})
          </legend>
          <p id="day-stops-hint" className="mb-2 text-xs text-slate-500">
            Stops are visited in the order you tick them.
          </p>
          <PageState
            isLoading={attractions.isLoading}
            isError={attractions.isError}
            error={attractions.error}
            onRetry={() => attractions.refetch()}
            isEmpty={attractions.data?.items.length === 0}
            emptyTitle={`No attractions in ${day.city}`}
            emptyDescription="Add one on the Attractions page first."
          >
            <ul className="max-h-64 space-y-1 overflow-y-auto">
              {attractions.data?.items.map((a) => {
                const checked = selected.includes(a.id);
                return (
                  <li key={a.id}>
                    <label className="flex min-h-10 items-center gap-2 text-sm text-slate-700">
                      <input
                        type="checkbox"
                        className="h-4 w-4"
                        checked={checked}
                        disabled={!checked && full}
                        onChange={() => toggle(a.id)}
                      />
                      {a.name}
                      <span className="text-slate-500">({a.durationMinutes} min)</span>
                    </label>
                  </li>
                );
              })}
            </ul>
          </PageState>
          {full && (
            <p className="mt-2 text-xs text-slate-600">
              {MAX_STOPS} stops chosen — the most for one day. Untick one to choose another.
            </p>
          )}
          {attractionsError && (
            <p role="alert" className="mt-2 text-xs text-red-700">
              {attractionsError}
            </p>
          )}
        </fieldset>

        <div className="flex flex-col gap-1">
          <label htmlFor="day-notes" className="text-sm font-medium text-slate-700">
            Notes (optional)
          </label>
          <textarea
            id="day-notes"
            rows={3}
            className="input"
            maxLength={MAX_NOTES}
            value={notes}
            aria-invalid={notesError ? true : undefined}
            aria-describedby={notesError ? 'day-notes-hint day-notes-error' : 'day-notes-hint'}
            onChange={(e) => setNotes(e.target.value)}
          />
          <p id="day-notes-hint" className="text-xs text-slate-500">
            {notes.length}/{MAX_NOTES} characters
          </p>
          {notesError && (
            <p id="day-notes-error" role="alert" className="text-xs text-red-700">
              {notesError}
            </p>
          )}
        </div>

        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="btn-primary" disabled={save.isPending}>
            {save.isPending ? 'Saving…' : 'Save'}
          </button>
        </div>
      </form>
    </Dialog>
  );
}
