import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable, type Column } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { useToast } from '@/shared/components/Toast';
import { useListParams } from '@/shared/hooks/useListParams';
import { useDeleteHotel, useHotels } from './api';
import { DeleteButton } from './DeleteButton';
import { HotelFormDialog } from './HotelFormDialog';
import { RoomTypesDialog } from './RoomTypesDialog';
import type { HotelDto } from './types';

const CITIES = ['Colombo', 'Kandy', 'Ella', 'Galle'];

/** Component B: hotels CRUD with room types, search, city and star filters, sorting and paging. */
export default function HotelsPage() {
  const list = useListParams({ sort: 'name' });
  const query = {
    city: list.get('city'),
    minStars: list.get('minStars'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const hotels = useHotels(query);
  const remove = useDeleteHotel();
  const toast = useToast();
  const [editing, setEditing] = useState<HotelDto | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [rooms, setRooms] = useState<HotelDto | null>(null);
  const [toDelete, setToDelete] = useState<HotelDto | null>(null);

  const openForm = (hotel: HotelDto | null) => {
    setEditing(hotel);
    setFormOpen(true);
  };

  const columns: Column<HotelDto>[] = [
    { key: 'name', header: 'Name', sortKey: 'name', render: (h) => h.name },
    { key: 'city', header: 'City', sortKey: 'city', render: (h) => h.city },
    { key: 'stars', header: 'Stars', sortKey: 'starRating', render: (h) => '★'.repeat(h.starRating) },
    {
      key: 'rooms',
      header: 'Room types',
      render: (h) => (
        <button
          type="button"
          className="text-brand-700 hover:underline"
          aria-label={`Room types of ${h.name}`}
          onClick={(event) => {
            event.stopPropagation();
            setRooms(h);
          }}
        >
          {h.roomTypes.length} types · {h.roomTypes.reduce((n, r) => n + r.totalRooms, 0)} rooms
        </button>
      ),
    },
    { key: 'active', header: 'Status', render: (h) => (h.isActive ? 'Active' : 'Inactive') },
    {
      key: 'delete',
      header: 'Delete',
      render: (h) => <DeleteButton label={h.name} onClick={() => setToDelete(h)} />,
    },
  ];

  return (
    <section className="space-y-4">
      <PageHeader
        title="Hotels"
        description="Hotels and room types; availability is checked per room type and night."
        actions={
          <button type="button" className="btn-primary" onClick={() => openForm(null)}>
            Add hotel
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
            options: CITIES.map((c) => ({ value: c, label: c })),
            onChange: (city) => list.set({ city }),
          },
          {
            name: 'minStars',
            label: 'Stars at least',
            value: query.minStars,
            options: ['3', '4', '5'].map((s) => ({ value: s, label: s })),
            onChange: (minStars) => list.set({ minStars }),
          },
        ]}
      />
      <PageState
        isLoading={hotels.isLoading}
        isError={hotels.isError}
        error={hotels.error}
        onRetry={() => hotels.refetch()}
        isEmpty={hotels.data?.total === 0}
        emptyTitle="No hotels found"
      >
        {hotels.data && (
          <DataTable
            caption="Hotels"
            columns={columns}
            rows={hotels.data.items}
            getRowId={(h) => h.id}
            rowLabel={(h) => h.name}
            total={hotels.data.total}
            page={hotels.data.page}
            pageSize={hotels.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
            onRowClick={openForm}
          />
        )}
      </PageState>
      <HotelFormDialog open={formOpen} hotel={editing} onClose={() => setFormOpen(false)} />
      <RoomTypesDialog hotel={rooms} onClose={() => setRooms(null)} />
      <ConfirmDialog
        open={toDelete !== null}
        title="Delete hotel"
        message={`Delete ${toDelete?.name ?? ''}? Refused while rooms are held for an upcoming trip.`}
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
