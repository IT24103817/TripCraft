import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { getErrorMessage } from '@/shared/api/errors';
import { ROLES, type Role } from '@/shared/api/types';
import { Button } from '@/shared/components/Button';
import { Dialog } from '@/shared/components/Dialog';
import { FormField } from '@/shared/components/FormField';
import { useToast } from '@/shared/components/Toast';
import { statusLabel } from '@/shared/statuses';
import { createUserSchema, type CreateUserForm } from './userSchema';
import { useCreateUser } from './usersApi';

export function CreateUserDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const toast = useToast();
  const create = useCreateUser();
  const { register, handleSubmit, formState, reset } = useForm<CreateUserForm>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { role: 'OperationsManager' },
  });

  const submit = handleSubmit((values) =>
    create.mutate(
      { ...values, role: values.role as Role },
      {
        onSuccess: (user) => {
          toast.success(`Created ${user.email}.`);
          reset();
          onClose();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    ),
  );

  return (
    <Dialog open={open} title="Create user" onClose={onClose}>
      <form noValidate className="space-y-3" onSubmit={submit}>
        <FormField
          label="Full name"
          registration={register('fullName')}
          error={formState.errors.fullName?.message}
        />
        <FormField
          label="Email"
          type="email"
          registration={register('email')}
          error={formState.errors.email?.message}
        />
        <FormField
          label="Temporary password"
          type="password"
          autoComplete="new-password"
          hint="At least 8 characters with upper-case, lower-case and a digit."
          registration={register('password')}
          error={formState.errors.password?.message}
        />
        <FormField
          label="Role"
          as="select"
          options={ROLES.map((r) => ({ value: r, label: statusLabel(r) }))}
          registration={register('role')}
          error={formState.errors.role?.message}
        />
        <div className="flex justify-end gap-2">
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={create.isPending} loadingText="Creating…">
            Create user
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
