import type { FieldErrors, UseFieldArrayReturn, UseFormRegister } from 'react-hook-form';
import { NEW_ROOM_TYPE, type HotelForm } from './schemas';

interface Props {
  register: UseFormRegister<HotelForm>;
  fieldArray: UseFieldArrayReturn<HotelForm, 'roomTypes'>;
  errors: FieldErrors<HotelForm>['roomTypes'];
}

const COLUMNS = [
  { field: 'name', label: 'name', type: 'text' },
  { field: 'capacity', label: 'sleeps', type: 'number' },
  { field: 'ratePerNightLkr', label: 'rate per night (LKR)', type: 'number' },
  { field: 'totalRooms', label: 'total rooms', type: 'number' },
] as const;

/** The editable room types of the hotel form: one row per room type, with Add and Remove. */
export function RoomTypesFieldTable({ register, fieldArray, errors }: Props) {
  const { fields, append, remove } = fieldArray;
  // A list-level error ("Add at least one room type.") sits on root or on the list itself.
  const listError = errors?.root?.message ?? errors?.message;

  return (
    <fieldset className="space-y-2">
      <legend className="text-sm font-semibold text-slate-900">Room types</legend>
      <div className="overflow-x-auto">
        <table className="min-w-full text-sm">
          <caption className="sr-only">Room types of this hotel</caption>
          <thead className="text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
            <tr>
              <th scope="col" className="py-1 pr-2">
                Name
              </th>
              <th scope="col" className="py-1 pr-2">
                Sleeps
              </th>
              <th scope="col" className="py-1 pr-2">
                Rate / night (LKR)
              </th>
              <th scope="col" className="py-1 pr-2">
                Rooms
              </th>
              <th scope="col" className="py-1">
                <span className="sr-only">Remove</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {fields.map((field, index) => (
              <tr key={field.id} className="align-top">
                {COLUMNS.map((column) => {
                  const error = errors?.[index]?.[column.field]?.message;
                  const errorId = `room-${index}-${column.field}-error`;
                  return (
                    <td key={column.field} className="py-1 pr-2">
                      <input
                        type={column.type}
                        className="input min-w-24"
                        aria-label={`Room type ${index + 1} ${column.label}`}
                        aria-invalid={error ? true : undefined}
                        aria-describedby={error ? errorId : undefined}
                        {...register(`roomTypes.${index}.${column.field}`)}
                      />
                      {error && (
                        <p id={errorId} role="alert" className="mt-1 text-xs text-red-700">
                          {error}
                        </p>
                      )}
                    </td>
                  );
                })}
                <td className="py-1">
                  <button
                    type="button"
                    className="btn-secondary"
                    aria-label={`Remove room type ${index + 1}`}
                    onClick={() => remove(index)}
                  >
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {listError && (
        <p role="alert" className="text-xs text-red-700">
          {listError}
        </p>
      )}
      <button type="button" className="btn-secondary" onClick={() => append({ ...NEW_ROOM_TYPE })}>
        Add room type
      </button>
    </fieldset>
  );
}
