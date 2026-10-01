import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { FormField } from '@/shared/components/FormField';
import { BLOCK_REASONS, blockNote, blockSchema, type BlockFormValues } from './schemas';
import type { UpdateHoldRequest } from './types';

interface Props {
  /** "create" asks for a reason (leave, maintenance, other); "edit" changes an existing block's note. */
  mode: 'create' | 'edit';
  initial: BlockFormValues;
  /** Room types: the most rooms that can be blocked. Guides and vehicles: 1, and no quantity field. */
  maxQuantity: number;
  showQuantity: boolean;
  isPending: boolean;
  submitLabel: string;
  onSubmit: (block: UpdateHoldRequest) => void;
}

/** The manual block form of the availability side panel: reason or note, first and last day, rooms. */
export function BlockForm({
  mode,
  initial,
  maxQuantity,
  showQuantity,
  isPending,
  submitLabel,
  onSubmit,
}: Props) {
  const { register, handleSubmit, formState } = useForm<BlockFormValues>({
    resolver: zodResolver(blockSchema(maxQuantity)),
    defaultValues: initial,
  });

  const submit = handleSubmit((values) =>
    onSubmit({
      fromDate: values.fromDate,
      toDate: values.toDate,
      quantity: values.quantity,
      note: blockNote(values),
    }),
  );

  const errors = formState.errors;
  return (
    <form
      noValidate
      aria-label={mode === 'create' ? 'Block' : 'Edit block'}
      className="space-y-3"
      onSubmit={submit}
    >
      {mode === 'create' && (
        <FormField
          label="Reason"
          as="select"
          options={BLOCK_REASONS.map((reason) => ({ value: reason, label: reason }))}
          registration={register('reason')}
          error={errors.reason?.message}
        />
      )}
      <FormField
        label={mode === 'create' ? 'Note' : 'Note (required)'}
        registration={register('details')}
        error={errors.details?.message}
        hint={mode === 'create' ? 'Required for Other, e.g. "Annual leave" or "Brake service".' : undefined}
      />
      <div className="grid grid-cols-2 gap-3">
        <FormField
          label="From"
          type="date"
          registration={register('fromDate')}
          error={errors.fromDate?.message}
        />
        <FormField label="To" type="date" registration={register('toDate')} error={errors.toDate?.message} />
      </div>
      {showQuantity && (
        <FormField
          label="Rooms"
          type="number"
          registration={register('quantity')}
          error={errors.quantity?.message}
          hint={`1–${maxQuantity}`}
        />
      )}
      <button type="submit" className="btn-primary w-full" disabled={isPending}>
        {isPending ? 'Saving…' : submitLabel}
      </button>
    </form>
  );
}
