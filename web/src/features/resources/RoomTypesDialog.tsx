import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { formatLkr } from '@/shared/utils/format';
import { useDeleteRoomType, useSaveRoomType } from './api';
import { roomTypeSchema, type RoomTypeForm } from './schemas';
import type { HotelDto } from './types';

/** The room types of one hotel: list, add and delete (409 when rooms of that type are held). */
export function RoomTypesDialog({ hotel, onClose }: { hotel: HotelDto | null; onClose: () => void }) {
  const toast = useToast();
  const save = useSaveRoomType(hotel?.id ?? '');
  const remove = useDeleteRoomType(hotel?.id ?? '');
  const { register, handleSubmit, formState, reset } = useForm<RoomTypeForm>({
    resolver: zodResolver(roomTypeSchema),
  });

  useEffect(() => {
    reset({ name: '', capacity: 2, ratePerNightLkr: 12000, totalRooms: 5 });
  }, [hotel, reset]);

  const add = handleSubmit((values) =>
    save.mutate(
      { body: values },
      {
        onSuccess: (saved) => {
          toast.success(`Added ${saved.name}.`);
          onClose();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    ),
  );

  const errors = formState.errors;
  return (
    <Dialog open={hotel !== null} title={`Room types — ${hotel?.name ?? ''}`} onClose={onClose}>
      <ul aria-label="Room types" className="mb-4 space-y-1 text-sm">
        {hotel?.roomTypes.length === 0 && <li className="text-slate-500">No room types yet.</li>}
        {hotel?.roomTypes.map((r) => (
          <li
            key={r.id}
            className="flex items-center justify-between rounded border border-slate-200 px-3 py-2"
          >
            <span>
              {r.name} · sleeps {r.capacity} · {r.totalRooms} rooms · {formatLkr(r.ratePerNightLkr)}/night
            </span>
            <button
              type="button"
              className="text-red-700 hover:underline"
              aria-label={`Delete ${r.name}`}
              onClick={() =>
                remove.mutate(r.id, {
                  onSuccess: () => {
                    toast.success(`Deleted ${r.name}.`);
                    onClose();
                  },
                  onError: (error) => toast.error(getErrorMessage(error)),
                })
              }
            >
              Delete
            </button>
          </li>
        ))}
      </ul>
      <form noValidate className="space-y-3" onSubmit={add}>
        <h3 className="text-sm font-semibold text-slate-900">Add a room type</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <FormField label="Room type name" registration={register('name')} error={errors.name?.message} />
          <FormField
            label="Sleeps"
            type="number"
            registration={register('capacity')}
            error={errors.capacity?.message}
          />
          <FormField
            label="Rate per night (LKR)"
            type="number"
            registration={register('ratePerNightLkr')}
            error={errors.ratePerNightLkr?.message}
          />
          <FormField
            label="Rooms"
            type="number"
            registration={register('totalRooms')}
            error={errors.totalRooms?.message}
          />
        </div>
        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={onClose}>
            Close
          </button>
          <button type="submit" className="btn-primary" disabled={save.isPending}>
            Add room type
          </button>
        </div>
      </form>
    </Dialog>
  );
}
