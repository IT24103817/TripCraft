import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { ActiveCheckbox } from './ActiveCheckbox';
import { useSaveHotel } from './api';
import { hotelSchema, type HotelForm } from './schemas';
import type { HotelDto } from './types';

interface Props {
  open: boolean;
  hotel: HotelDto | null;
  onClose: () => void;
}

export function HotelFormDialog({ open, hotel, onClose }: Props) {
  const toast = useToast();
  const save = useSaveHotel();
  const { register, handleSubmit, formState, reset } = useForm<HotelForm>({
    resolver: zodResolver(hotelSchema),
  });

  useEffect(() => {
    if (open)
      reset(
        hotel ?? { name: '', city: '', starRating: 3, latitude: 7.2906, longitude: 80.6337, isActive: true },
      );
  }, [open, hotel, reset]);

  const submit = handleSubmit((values) =>
    save.mutate(
      { id: hotel?.id, body: values },
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
    <Dialog open={open} title={hotel ? 'Edit hotel' : 'Add hotel'} onClose={onClose}>
      <form noValidate className="space-y-3" onSubmit={submit}>
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
