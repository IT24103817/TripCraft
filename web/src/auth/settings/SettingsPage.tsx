import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { FormField } from '@/shared/components/FormField';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { useToast } from '@/shared/components/Toast';
import { formatDateTime } from '@/shared/utils/format';
import { useSaveSettings, useSettings } from './settingsApi';
import { settingsSchema, type SettingsForm } from './settingsSchema';

/**
 * Admin: the operator settings — which LLM the agents use, the cancellation notice, the margin and deposit
 * percentages and the contact tourists are given. The API reads them on every request.
 */
export default function SettingsPage() {
  const settings = useSettings();
  const save = useSaveSettings();
  const toast = useToast();
  const { register, handleSubmit, formState, reset } = useForm<SettingsForm>({
    resolver: zodResolver(settingsSchema),
  });

  // Fill the form once the settings have loaded (and again after a save).
  useEffect(() => {
    if (settings.data) {
      reset({
        llmProvider: settings.data.llmProvider,
        cancellationCutoffDays: settings.data.cancellationCutoffDays,
        marginPct: settings.data.marginPct,
        depositPct: settings.data.depositPct,
        operatorContact: settings.data.operatorContact,
      });
    }
  }, [settings.data, reset]);

  const submit = handleSubmit((values) =>
    save.mutate(values, {
      onSuccess: () => toast.success('Settings saved.'),
      onError: (error) => toast.error(getErrorMessage(error)),
    }),
  );

  const errors = formState.errors;
  return (
    <section className="space-y-4">
      <PageHeader
        title="Settings"
        description="Operator settings used by the API and the agents. Changes apply to the next request."
      />
      <PageState
        isLoading={settings.isLoading}
        isError={settings.isError}
        error={settings.error}
        onRetry={() => settings.refetch()}
      >
        <form noValidate aria-label="Settings" className="card max-w-2xl space-y-4" onSubmit={submit}>
          <fieldset className="space-y-2">
            <legend className="text-sm font-medium text-slate-700">LLM provider for new agent runs</legend>
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="radio"
                value="ollama"
                className="h-4 w-4 accent-brand-700"
                {...register('llmProvider')}
              />
              Ollama (runs on our own server)
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="radio"
                value="groq"
                className="h-4 w-4 accent-brand-700"
                {...register('llmProvider')}
              />
              Groq (cloud API)
            </label>
            {errors.llmProvider && (
              <p role="alert" className="text-xs text-red-700">
                {errors.llmProvider.message}
              </p>
            )}
          </fieldset>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <FormField
              label="Cancellation notice (days)"
              type="number"
              registration={register('cancellationCutoffDays')}
              error={errors.cancellationCutoffDays?.message}
              hint="0–30 days before the start"
            />
            <FormField
              label="Margin (%)"
              type="number"
              step="0.01"
              registration={register('marginPct')}
              error={errors.marginPct?.message}
              hint="Saved as today's rate card margin"
            />
            <FormField
              label="Deposit (%)"
              type="number"
              step="0.01"
              registration={register('depositPct')}
              error={errors.depositPct?.message}
              hint="Asked for on new quotations"
            />
          </div>
          <FormField
            label="Operator contact"
            registration={register('operatorContact')}
            error={errors.operatorContact?.message}
            hint="Given to tourists who need help, e.g. after the cancellation cut-off"
          />
          <div className="flex flex-wrap items-center justify-between gap-2">
            <p className="text-xs text-slate-500">Last saved {formatDateTime(settings.data?.updatedAt)}</p>
            <button type="submit" className="btn-primary" disabled={save.isPending}>
              {save.isPending ? 'Saving…' : 'Save settings'}
            </button>
          </div>
        </form>
      </PageState>
    </section>
  );
}
