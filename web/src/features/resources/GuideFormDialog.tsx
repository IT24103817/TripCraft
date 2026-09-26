import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { ActiveCheckbox } from './ActiveCheckbox';
import { useSaveGuide } from './api';
import { guideSchema, type GuideForm } from './schemas';
import type { GuideDto } from './types';

interface Props {
  open: boolean;
  guide: GuideDto | null;
  onClose: () => void;
}

export function GuideFormDialog({ open, guide, onClose }: Props) {
  const toast = useToast();
  const save = useSaveGuide();
  const { register, handleSubmit, formState, reset } = useForm<GuideForm>({
    resolver: zodResolver(guideSchema),
  });

  useEffect(() => {
    if (open)
      reset(
        guide
          ? { ...guide, languages: guide.languages.join(', ') }
          : { name: '', phone: '', languages: 'en', dayRateLkr: 6000, maxPax: 8, isActive: true },
      );
  }, [open, guide, reset]);

  const submit = handleSubmit((values) =>
    save.mutate(
      {
        id: guide?.id,
        body: {
          ...values,
          languages: values.languages.split(',').map((code) => code.trim().toLowerCase()),
          userId: guide?.userId ?? null,
        },
      },
      {
        onSuccess: (saved) => {
          toast.success(guide ? `Saved ${saved.name}.` : `Added ${saved.name}.`);
          onClose();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    ),
  );

  const errors = formState.errors;
  return (
    <Dialog open={open} title={guide ? 'Edit guide' : 'Add guide'} onClose={onClose}>
      <form noValidate className="space-y-3" onSubmit={submit}>
        <FormField label="Name" registration={register('name')} error={errors.name?.message} />
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <FormField label="Phone" registration={register('phone')} error={errors.phone?.message} />
          <FormField
            label="Languages"
            registration={register('languages')}
            error={errors.languages?.message}
            hint="Two-letter codes, e.g. en, de"
          />
          <FormField
            label="Day rate (LKR)"
            type="number"
            registration={register('dayRateLkr')}
            error={errors.dayRateLkr?.message}
          />
          <FormField
            label="Max group size"
            type="number"
            registration={register('maxPax')}
            error={errors.maxPax?.message}
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
