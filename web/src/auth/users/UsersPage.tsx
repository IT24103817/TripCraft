import { useMemo, useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { ROLES } from '@/shared/api/types';
import { Button } from '@/shared/components/Button';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable, type Column } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useToast } from '@/shared/components/Toast';
import { useListParams } from '@/shared/hooks/useListParams';
import { statusLabel } from '@/shared/statuses';
import type { UserDto } from '../types';
import { CreateUserDialog } from './CreateUserDialog';
import { useDeactivateUser, useUsers } from './usersApi';

const SORTERS: Record<string, (a: UserDto, b: UserDto) => number> = {
  fullName: (a, b) => a.fullName.localeCompare(b.fullName),
  email: (a, b) => a.email.localeCompare(b.email),
  role: (a, b) => a.role.localeCompare(b.role),
};

/** Admin only (PLAN.md section 2). The API returns every user, so filtering and paging happen here. */
export default function UsersPage() {
  const list = useListParams({ sort: 'fullName' });
  const users = useUsers();
  const deactivate = useDeactivateUser();
  const toast = useToast();
  const [creating, setCreating] = useState(false);
  const [toDeactivate, setToDeactivate] = useState<UserDto | null>(null);
  const role = list.get('role');
  const active = list.get('active');

  const filtered = useMemo(() => {
    const term = list.search.toLowerCase();
    const rows = (users.data ?? []).filter(
      (u) =>
        (!term || u.fullName.toLowerCase().includes(term) || u.email.toLowerCase().includes(term)) &&
        (!role || u.role === role) &&
        (!active || String(u.isActive) === active),
    );
    const key = list.sort.replace('-', '');
    const sorter = SORTERS[key];
    if (sorter) rows.sort((a, b) => (list.sort.startsWith('-') ? -sorter(a, b) : sorter(a, b)));
    return rows;
  }, [users.data, list.search, list.sort, role, active]);

  const pageRows = filtered.slice((list.page - 1) * list.pageSize, list.page * list.pageSize);

  const columns: Column<UserDto>[] = [
    { key: 'fullName', header: 'Name', sortKey: 'fullName', render: (u) => u.fullName },
    { key: 'email', header: 'Email', sortKey: 'email', render: (u) => u.email },
    { key: 'role', header: 'Role', sortKey: 'role', render: (u) => statusLabel(u.role) },
    {
      key: 'status',
      header: 'Status',
      render: (u) => <StatusBadge status={u.isActive ? 'Active' : 'Inactive'} />,
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (u) =>
        u.isActive ? (
          <button
            type="button"
            className="text-red-700 hover:underline"
            onClick={() => setToDeactivate(u)}
            aria-label={`Deactivate ${u.email}`}
          >
            Deactivate
          </button>
        ) : null,
    },
  ];

  return (
    <section className="space-y-4">
      <PageHeader
        title="Users"
        description="Create staff accounts and deactivate users."
        actions={<Button onClick={() => setCreating(true)}>Create user</Button>}
      />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Name or email',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'role',
            label: 'Role',
            value: role,
            options: ROLES.map((r) => ({ value: r, label: statusLabel(r) })),
            onChange: (v) => list.set({ role: v }),
          },
          {
            name: 'active',
            label: 'Status',
            value: active,
            options: [
              { value: 'true', label: 'Active' },
              { value: 'false', label: 'Inactive' },
            ],
            onChange: (v) => list.set({ active: v }),
          },
        ]}
      />
      <PageState
        isLoading={users.isLoading}
        isError={users.isError}
        error={users.error}
        onRetry={() => users.refetch()}
        isEmpty={filtered.length === 0}
        emptyTitle="No users match these filters"
      >
        <DataTable
          caption="Users"
          columns={columns}
          rows={pageRows}
          getRowId={(u) => u.id}
          total={filtered.length}
          page={list.page}
          pageSize={list.pageSize}
          sort={list.sort}
          onSortChange={(sort) => list.set({ sort })}
          onPageChange={(page) => list.set({ page })}
          onPageSizeChange={(pageSize) => list.set({ pageSize })}
        />
      </PageState>
      <CreateUserDialog open={creating} onClose={() => setCreating(false)} />
      <ConfirmDialog
        open={toDeactivate !== null}
        title="Deactivate user"
        message={`${toDeactivate?.email ?? ''} will no longer be able to sign in.`}
        confirmLabel="Deactivate"
        tone="danger"
        isPending={deactivate.isPending}
        onCancel={() => setToDeactivate(null)}
        onConfirm={() =>
          toDeactivate &&
          deactivate.mutate(toDeactivate.id, {
            onSuccess: () => {
              toast.success(`${toDeactivate.email} was deactivated.`);
              setToDeactivate(null);
            },
            onError: (error) => toast.error(getErrorMessage(error)),
          })
        }
      />
    </section>
  );
}
