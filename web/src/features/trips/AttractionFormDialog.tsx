import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { Button } from '@/shared/components/Button';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { useSaveAttraction } from './api';
import { attractionSchema, type AttractionForm } from './attractionSchema';
import { MapPreview } from './MapPreview';
import type { AttractionDto } from './types';

interface Props {
  open: boolean;
  attraction: AttractionDto | null;
  onClose: () => void;
}

export function AttractionFormDialog({ open, attraction, onClose }: Props) {
  const toast = useToast();
  const save = useSaveAttraction();
  const { register, handleSubmit, formState, reset, watch } = useForm<AttractionForm>({
    resolver: zodResolver(attractionSchema),
  });

  useEffect(() => {
    if (open)
      reset(
        attraction ?? {
          name: '',
          city: '',
          category: '',
          durationMinutes: 60,
          entryFeeLkr: 0,
          latitude: 7.2906,
          longitude: 80.6337,
        },
      );
  }, [open, attraction, reset]);

  const submit = handleSubmit((values) =>
    save.mutate(
      { id: attraction?.id, body: values },
      {
        onSuccess: (saved) => {
          toast.success(attraction ? `Saved ${saved.name}.` : `Added ${saved.name}.`);
          onClose();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    ),
  );

  const errors = formState.errors;
  return (
    <Dialog open={open} title={attraction ? 'Edit attraction' : 'Add attraction'} onClose={onClose}>
      <form noValidate className="space-y-3" onSubmit={submit}>
        <FormField label="Name" registration={register('name')} error={errors.name?.message} />
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <FormField label="City" registration={register('city')} error={errors.city?.message} />
          <FormField label="Category" registration={register('category')} error={errors.category?.message} />
          <FormField
            label="Duration (minutes)"
            type="number"
            registration={register('durationMinutes')}
            error={errors.durationMinutes?.message}
          />
          <FormField
            label="Entry fee (LKR)"
            type="number"
            step="0.01"
            registration={register('entryFeeLkr')}
            error={errors.entryFeeLkr?.message}
          />
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
        <MapPreview
          latitude={Number(watch('latitude'))}
          longitude={Number(watch('longitude'))}
          label={watch('name') || 'the attraction'}
        />
        <div className="flex justify-end gap-2">
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={save.isPending} loadingText="Saving…">
            Save
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
