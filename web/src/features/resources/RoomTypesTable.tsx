import { formatLkr } from '@/shared/utils/format';
import type { RoomTypeDto } from './types';

/** The room types of a hotel, read-only (hotel details). */
export function RoomTypesTable({ roomTypes }: { roomTypes: RoomTypeDto[] }) {
  if (roomTypes.length === 0) return <p className="text-sm text-slate-600">No room types yet.</p>;
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-slate-200 text-sm tabular-nums">
        <caption className="sr-only">Room types</caption>
        <thead className="bg-slate-50 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
          <tr>
            <th scope="col" className="px-3 py-2">
              Name
            </th>
            <th scope="col" className="px-3 py-2 text-right">
              Sleeps
            </th>
            <th scope="col" className="px-3 py-2 text-right">
              Rate / night
            </th>
            <th scope="col" className="px-3 py-2 text-right">
              Rooms
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {roomTypes.map((room) => (
            <tr key={room.id}>
              <th scope="row" className="px-3 py-2 text-left font-medium text-slate-900">
                {room.name}
              </th>
              <td className="px-3 py-2 text-right">{room.capacity}</td>
              <td className="px-3 py-2 text-right">{formatLkr(room.ratePerNightLkr)}</td>
              <td className="px-3 py-2 text-right">{room.totalRooms}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
