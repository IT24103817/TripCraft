import { Dialog } from '@/shared/components/Dialog';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { RoomTypesTable } from './RoomTypesTable';
import type { HotelDto } from './types';

interface Props {
  hotel: HotelDto | null;
  onClose: () => void;
  onEdit: (hotel: HotelDto) => void;
}

/** One hotel at a glance: where it is, its stars and its room types. "Edit hotel" opens the form. */
export function HotelDetailsDialog({ hotel, onClose, onEdit }: Props) {
  return (
    <Dialog open={hotel !== null} title={hotel?.name ?? ''} onClose={onClose} size="wide">
      {hotel && (
        <div className="space-y-4 text-sm">
          <dl className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            <Item label="City" value={hotel.city} />
            <Item label="Stars" value={`${hotel.starRating} ★`} />
            <Item label="Location" value={`${hotel.latitude}, ${hotel.longitude}`} />
            <Item label="Status" value={<StatusBadge status={hotel.isActive ? 'Active' : 'Inactive'} />} />
          </dl>
          <h3 className="font-semibold text-slate-900">Room types</h3>
          <RoomTypesTable roomTypes={hotel.roomTypes} />
          <div className="flex justify-end gap-2">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Close
            </button>
            <button type="button" className="btn-primary" onClick={() => onEdit(hotel)}>
              Edit hotel
            </button>
          </div>
        </div>
      )}
    </Dialog>
  );
}

function Item({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <dt className="text-slate-500">{label}</dt>
      <dd className="font-medium text-slate-900">{value}</dd>
    </div>
  );
}
