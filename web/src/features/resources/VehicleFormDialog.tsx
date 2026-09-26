import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { ActiveCheckbox } from './ActiveCheckbox';
import { useSaveVehicle } from './api';
import { VEHICLE_TYPES, vehicleSchema, type VehicleForm } from './schemas';
import type { VehicleDto } from './types';

interface Props {
  open: boolean;
  vehicle: VehicleDto | null;
  onClose: () => void;
}

export function VehicleFormDialog({ open, vehicle, onClose }: Props) {
  const toast = useToast();
  const save = useSaveVehicle();
  const { register, handleSubmit, formState, reset } = useForm<VehicleForm>({
    resolver: zodResolver(vehicleSchema),
  });

  useEffect(() => {
    if (open)
      reset(
        vehicle
          ? { ...vehicle, type: vehicle.type as VehicleForm['type'] }
          : { registrationNo: '', type: 'Van', seats: 6, ratePerKmLkr: 120, isActive: true },
      );
  }, [open, vehicle, reset]);

  const submit = handleSubmit((values) =>
    save.mutate(
      { id: vehicle?.id, body: values },
      {
        onSuccess: (saved) => {
          toast.success(vehicle ? `Saved ${saved.registrationNo}.` : `Added ${saved.registrationNo}.`);
          onClose();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    ),
  );

  const errors = formState.errors;
  return (
    <Dialog open={open} title={vehicle ? 'Edit vehicle' : 'Add vehicle'} onClose={onClose}>
      <form noValidate className="space-y-3" onSubmit={submit}>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <FormField
            label="Registration"
            registration={register('registrationNo')}
            error={errors.registrationNo?.message}
          />
          <FormField
            label="Type"
            as="select"
            options={VEHICLE_TYPES.map((t) => ({ value: t, label: t }))}
            registration={register('type')}
            error={errors.type?.message}
          />
          <FormField
            label="Seats"
            type="number"
            registration={register('seats')}
            error={errors.seats?.message}
          />
          <FormField
            label="Rate per km (LKR)"
            type="number"
            step="0.01"
            registration={register('ratePerKmLkr')}
            error={errors.ratePerKmLkr?.message}
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
