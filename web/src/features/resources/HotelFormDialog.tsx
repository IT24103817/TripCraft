import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { ActiveCheckbox } from './ActiveCheckbox';
import { useSaveHotel } from './api';
import { RoomTypesFieldTable } from './RoomTypesFieldTable';
import { hotelSchema, NEW_ROOM_TYPE, type HotelForm } from './schemas';
import type { HotelDto, SaveHotelRequest } from './types';

interface Props {
  open: boolean;
  hotel: HotelDto | null;
  onClose: () => void;
}

/** The form's starting values: the hotel being edited, or a new hotel in Kandy with one room type. */
function initialValues(hotel: HotelDto | null): HotelForm {
  if (!hotel) {
    return {
      name: '',
      city: '',
      starRating: 3,
      latitude: 7.2906,
      longitude: 80.6337,
      isActive: true,
      roomTypes: [{ ...NEW_ROOM_TYPE }],
    };
  }
  return {
    name: hotel.name,
    city: hotel.city,
    starRating: hotel.starRating,
    latitude: hotel.latitude,
    longitude: hotel.longitude,
    isActive: hotel.isActive,
    roomTypes: hotel.roomTypes.map((room) => ({
      roomTypeId: room.id,
      name: room.name,
      capacity: room.capacity,
      ratePerNightLkr: room.ratePerNightLkr,
      totalRooms: room.totalRooms,
    })),
  };
}

/** The API body: an existing room type keeps its id (update), a new one has none (add). */
function toRequest(values: HotelForm): SaveHotelRequest {
  const { roomTypes, ...hotel } = values;
  return {
    ...hotel,
    roomTypes: roomTypes.map(({ roomTypeId, ...room }) => (roomTypeId ? { id: roomTypeId, ...room } : room)),
  };
}

/** Add or edit a hotel together with its room types (POST/PUT /api/hotels with roomTypes in the body). */
export function HotelFormDialog({ open, hotel, onClose }: Props) {
  const toast = useToast();
  const save = useSaveHotel();
  const { register, handleSubmit, formState, reset, control } = useForm<HotelForm>({
    resolver: zodResolver(hotelSchema),
  });
  const roomTypes = useFieldArray({ control, name: 'roomTypes' });

  useEffect(() => {
    if (open) reset(initialValues(hotel));
  }, [open, hotel, reset]);

  const submit = handleSubmit((values) =>
    save.mutate(
      { id: hotel?.id, body: toRequest(values) },
      {
        onSuccess: (saved) => {
          toast.success(hotel ? `Saved ${saved.name}.` : `Added ${saved.name}.`);
          onClose();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    ),
  );

  const errors = formState.errors;
  return (
    <Dialog open={open} title={hotel ? 'Edit hotel' : 'Add hotel'} onClose={onClose} size="wide">
      <form noValidate className="space-y-4" onSubmit={submit}>
        <FormField label="Name" registration={register('name')} error={errors.name?.message} />
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
          <FormField label="City" registration={register('city')} error={errors.city?.message} />
          <FormField
            label="Stars"
            type="number"
            registration={register('starRating')}
            error={errors.starRating?.message}
          />
          <div />
          <FormField
            label="Latitude"
            type="number"
            step="any"
            registration={register('latitude')}
            error={errors.latitude?.message}
          />
          <FormField
            label="Longitude"
            type="number"
            step="any"
            registration={register('longitude')}
            error={errors.longitude?.message}
          />
        </div>
        <ActiveCheckbox registration={register('isActive')} />
        <RoomTypesFieldTable register={register} fieldArray={roomTypes} errors={errors.roomTypes} />
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
