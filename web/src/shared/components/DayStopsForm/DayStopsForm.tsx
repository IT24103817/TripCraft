import { useState, type FormEvent } from 'react';
import { getErrorMessage, getErrorStatus, getFieldError } from '../../api/errors';
import { cn } from '../../utils/cn';
import { Button } from '../Button';
import { PageState } from '../PageState';
import type { DayStopsFormProps } from './DayStopsForm.types';

/** Operator rule (PLAN.md): at most 3 stops a day. The API checks it again. */
const MAX_STOPS = 3;
const MAX_NOTES = 500;

/**
 * The form of one itinerary day: tick 1–3 attractions of the day's city (visited in the order ticked) and edit
 * the notes. Used by the saved-itinerary editor (Confirmed trips) and the proposal editor (trips in review).
 */
export const DayStopsForm = ({
  city,
  options,
  initialIds,
  initialNotes,
  showNotes = true,
  isPending,
  saveError,
  onSave,
  onCancel,
  className,
}: DayStopsFormProps) => {
  const [selected, setSelected] = useState<string[]>(initialIds);
  const [notes, setNotes] = useState(initialNotes);
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
    onSave(selected, notes.trim() || null);
  };

  // 400: messages per field; 409 (wrong status, no itinerary) and others: one message.
  const attractionsError = noneSelected
    ? 'Pick at least one attraction.'
    : getFieldError(saveError, 'attractionIds');
  const notesError = getFieldError(saveError, 'notes');
  const formError =
    saveError && !(getErrorStatus(saveError) === 400 && (attractionsError || notesError))
      ? getErrorMessage(saveError)
      : null;

  return (
    <form noValidate className={cn('space-y-4', className)} onSubmit={submit}>
      {formError && (
        <p role="alert" className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {formError}
        </p>
      )}

      <fieldset aria-describedby="day-stops-hint">
        <legend className="text-sm font-medium text-slate-700">
          Attractions in {city} (up to {MAX_STOPS})
        </legend>
        <p id="day-stops-hint" className="mb-2 text-xs text-slate-500">
          Stops are visited in the order you tick them.
        </p>
        <PageState
          isLoading={options.isLoading}
          isError={options.isError}
          error={options.error}
          onRetry={() => options.refetch()}
          isEmpty={options.items?.length === 0}
          emptyTitle={`No attractions in ${city}`}
          emptyDescription="Add one on the Attractions page first."
        >
          <ul className="max-h-64 space-y-1 overflow-y-auto">
            {options.items?.map((a) => {
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
          <p role="alert" className="mt-2 text-xs font-medium text-red-700">
            {attractionsError}
          </p>
        )}
      </fieldset>

      {showNotes && (
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
            <p id="day-notes-error" role="alert" className="text-xs font-medium text-red-700">
              {notesError}
            </p>
          )}
        </div>
      )}

      <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
        <Button variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isPending} loadingText="Saving…">
          Save
        </Button>
      </div>
    </form>
  );
};
