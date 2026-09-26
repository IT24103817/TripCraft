import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable, type Column } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { useToast } from '@/shared/components/Toast';
import { useListParams } from '@/shared/hooks/useListParams';
import { formatLkr } from '@/shared/utils/format';
import { useDeleteGuide, useGuides } from './api';
import { DeleteButton } from './DeleteButton';
import { GuideFormDialog } from './GuideFormDialog';
import type { GuideDto } from './types';

const LANGUAGES = ['en', 'de', 'fr', 'ja', 'zh', 'si'];

/** Component B: guides CRUD with search, language and status filters, sorting and paging. */
export default function GuidesPage() {
  const list = useListParams({ sort: 'name' });
  const query = {
    language: list.get('language'),
    isActive: list.get('isActive'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const guides = useGuides(query);
  const remove = useDeleteGuide();
  const toast = useToast();
  const [editing, setEditing] = useState<GuideDto | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [toDelete, setToDelete] = useState<GuideDto | null>(null);

  const openForm = (guide: GuideDto | null) => {
    setEditing(guide);
    setFormOpen(true);
  };

  const columns: Column<GuideDto>[] = [
    { key: 'name', header: 'Name', sortKey: 'name', render: (g) => g.name },
    { key: 'languages', header: 'Languages', render: (g) => g.languages.join(', ') },
    { key: 'maxPax', header: 'Max group', sortKey: 'maxPax', render: (g) => g.maxPax },
    { key: 'rate', header: 'Day rate', sortKey: 'dayRateLkr', render: (g) => formatLkr(g.dayRateLkr) },
    { key: 'active', header: 'Status', render: (g) => (g.isActive ? 'Active' : 'Inactive') },
    { key: 'login', header: 'App login', render: (g) => (g.userId ? 'Linked' : '—') },
    {
      key: 'delete',
      header: 'Delete',
      render: (g) => <DeleteButton label={g.name} onClick={() => setToDelete(g)} />,
    },
  ];

  return (
    <section className="space-y-4">
      <PageHeader
        title="Guides"
        description="Guides the Resource agent can propose; holds stop double-booking."
        actions={
          <button type="button" className="btn-primary" onClick={() => openForm(null)}>
            Add guide
          </button>
        }
      />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search name or phone',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'language',
            label: 'Language',
            value: query.language,
            options: LANGUAGES.map((l) => ({ value: l, label: l })),
            onChange: (language) => list.set({ language }),
          },
          {
            name: 'isActive',
            label: 'Status',
            value: query.isActive,
            options: [
              { value: 'true', label: 'Active' },
              { value: 'false', label: 'Inactive' },
            ],
            onChange: (isActive) => list.set({ isActive }),
          },
        ]}
      />
      <PageState
        isLoading={guides.isLoading}
        isError={guides.isError}
        error={guides.error}
        onRetry={() => guides.refetch()}
        isEmpty={guides.data?.total === 0}
        emptyTitle="No guides found"
      >
        {guides.data && (
          <DataTable
            caption="Guides"
            columns={columns}
            rows={guides.data.items}
            getRowId={(g) => g.id}
            rowLabel={(g) => g.name}
            total={guides.data.total}
            page={guides.data.page}
            pageSize={guides.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
            onRowClick={openForm}
          />
        )}
      </PageState>
      <GuideFormDialog open={formOpen} guide={editing} onClose={() => setFormOpen(false)} />
      <ConfirmDialog
        open={toDelete !== null}
        title="Delete guide"
        message={`Delete ${toDelete?.name ?? ''}? Past trips keep the guide; new plans will not use them.`}
        confirmLabel="Delete"
        tone="danger"
        isPending={remove.isPending}
        onCancel={() => setToDelete(null)}
        onConfirm={() =>
          toDelete &&
          remove.mutate(toDelete.id, {
            onSuccess: () => {
              toast.success(`Deleted ${toDelete.name}.`);
              setToDelete(null);
            },
            onError: (error) => toast.error(getErrorMessage(error)),
          })
        }
      />
    </section>
  );
}
