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
import { useAttractionFacets, useAttractions, useDeleteAttraction } from './api';
import { AttractionFormDialog } from './AttractionFormDialog';
import type { AttractionDto } from './types';

export default function AttractionsPage() {
  const list = useListParams({ sort: 'name' });
  const query = {
    city: list.get('city'),
    category: list.get('category'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const attractions = useAttractions(query);
  const facets = useAttractionFacets();
  const remove = useDeleteAttraction();
  const toast = useToast();
  const [editing, setEditing] = useState<AttractionDto | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [toDelete, setToDelete] = useState<AttractionDto | null>(null);

  const openForm = (attraction: AttractionDto | null) => {
    setEditing(attraction);
    setFormOpen(true);
  };

  const columns: Column<AttractionDto>[] = [
    { key: 'name', header: 'Name', sortKey: 'name', render: (a) => a.name },
    { key: 'city', header: 'City', sortKey: 'city', render: (a) => a.city },
    { key: 'category', header: 'Category', sortKey: 'category', render: (a) => a.category },
    {
      key: 'duration',
      header: 'Duration',
      sortKey: 'durationMinutes',
      render: (a) => `${a.durationMinutes} min`,
    },
    { key: 'fee', header: 'Entry fee', sortKey: 'entryFeeLkr', render: (a) => formatLkr(a.entryFeeLkr) },
    {
      key: 'delete',
      header: 'Delete',
      render: (a) => (
        <button
          type="button"
          className="text-red-700 hover:underline"
          aria-label={`Delete ${a.name}`}
          onClick={(event) => {
            event.stopPropagation();
            setToDelete(a);
          }}
        >
          Delete
        </button>
      ),
    },
  ];

  const toOptions = (values: string[] = []) => values.map((v) => ({ value: v, label: v }));

  return (
    <section className="space-y-4">
      <PageHeader
        title="Attractions"
        description="Places the Itinerary agent can choose from."
        actions={
          <button type="button" className="btn-primary" onClick={() => openForm(null)}>
            Add attraction
          </button>
        }
      />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search by name',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'city',
            label: 'City',
            value: query.city,
            options: toOptions(facets.data?.cities),
            onChange: (city) => list.set({ city }),
          },
          {
            name: 'category',
            label: 'Category',
            value: query.category,
            options: toOptions(facets.data?.categories),
            onChange: (category) => list.set({ category }),
          },
        ]}
      />
      <PageState
        isLoading={attractions.isLoading}
        isError={attractions.isError}
        error={attractions.error}
        onRetry={() => attractions.refetch()}
        isEmpty={attractions.data?.total === 0}
        emptyTitle="No attractions found"
        emptyAction={
          <button type="button" className="btn-primary" onClick={() => openForm(null)}>
            Add attraction
          </button>
        }
      >
        {attractions.data && (
          <DataTable
            caption="Attractions"
            columns={columns}
            rows={attractions.data.items}
            getRowId={(a) => a.id}
            rowLabel={(a) => a.name}
            total={attractions.data.total}
            page={attractions.data.page}
            pageSize={attractions.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
            onRowClick={openForm}
          />
        )}
      </PageState>
      <AttractionFormDialog open={formOpen} attraction={editing} onClose={() => setFormOpen(false)} />
      <ConfirmDialog
        open={toDelete !== null}
        title="Delete attraction"
        message={`Delete ${toDelete?.name ?? ''}? Existing itineraries keep it; new plans will not use it.`}
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
