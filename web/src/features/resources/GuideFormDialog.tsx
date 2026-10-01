import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { ActiveCheckbox } from './ActiveCheckbox';
import { useCreateGuide, useUpdateGuide } from './api';
import { editGuideSchema, newGuideSchema, type GuideAccountForm } from './schemas';
import type { GuideAccountDto, GuideDto } from './types';

interface Props {
  open: boolean;
  /** null = add a new guide (with a login email); otherwise edit this guide's details. */
  guide: GuideDto | null;
  onClose: () => void;
  /** After a create: the new login and its one-time temporary password. */
  onCreated: (account: GuideAccountDto) => void;
}

export function GuideFormDialog({ open, guide, onClose, onCreated }: Props) {
  const toast = useToast();
  const create = useCreateGuide();
  const update = useUpdateGuide();
  const { register, handleSubmit, formState, reset } = useForm<GuideAccountForm>({
    // Only a new guide needs a valid login email.
    resolver: zodResolver(guide ? editGuideSchema : newGuideSchema),
  });

  useEffect(() => {
    if (open)
      reset(
        guide
          ? { ...guide, languages: guide.languages.join(', '), email: '' }
          : {
              name: '',
              phone: '',
              languages: 'en',
              dayRateLkr: 6000,
              maxPax: 8,
              isActive: true,
              email: '',
            },
      );
  }, [open, guide, reset]);

  const submit = handleSubmit(({ email, ...values }) => {
    const details = {
      ...values,
      languages: values.languages.split(',').map((code) => code.trim().toLowerCase()),
    };
    const onError = (error: unknown) => toast.error(getErrorMessage(error));
    if (guide) {
      update.mutate(
        { id: guide.id, body: details },
        {
          onSuccess: (saved) => {
            toast.success(`Saved ${saved.name}.`);
            onClose();
          },
          onError,
        },
      );
    } else {
      create.mutate(
        { ...details, email },
        {
          onSuccess: (account) => {
            toast.success(`Added ${account.guide.name}.`);
            onClose();
            onCreated(account);
          },
          onError,
        },
      );
    }
  });

  const saving = create.isPending || update.isPending;
  const errors = formState.errors;
  return (
    <Dialog open={open} title={guide ? 'Edit guide' : 'Add guide'} onClose={onClose}>
      <form noValidate className="space-y-3" onSubmit={submit}>
        <FormField label="Name" registration={register('name')} error={errors.name?.message} />
        {!guide && (
          <FormField
            label="Login email"
            type="email"
            autoComplete="off"
            registration={register('email')}
            error={errors.email?.message}
            hint="The guide signs in to the mobile app with this email and a temporary password."
          />
        )}
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
          <button type="submit" className="btn-primary" disabled={saving}>
            {saving ? 'Saving…' : 'Save'}
          </button>
        </div>
      </form>
    </Dialog>
  );
}
