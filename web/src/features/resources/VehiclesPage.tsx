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
import { useDeleteVehicle, useVehicles } from './api';
import { DeleteButton } from './DeleteButton';
import { VEHICLE_TYPES } from './schemas';
import type { VehicleDto } from './types';
import { VehicleFormDialog } from './VehicleFormDialog';

/** Component B: vehicles CRUD with search, type and seat filters, sorting and paging. */
export default function VehiclesPage() {
  const list = useListParams({ sort: 'registrationNo' });
  const query = {
    type: list.get('type'),
    minSeats: list.get('minSeats'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const vehicles = useVehicles(query);
  const remove = useDeleteVehicle();
  const toast = useToast();
  const [editing, setEditing] = useState<VehicleDto | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [toDelete, setToDelete] = useState<VehicleDto | null>(null);

  const openForm = (vehicle: VehicleDto | null) => {
    setEditing(vehicle);
    setFormOpen(true);
  };

  const columns: Column<VehicleDto>[] = [
    {
      key: 'registrationNo',
      header: 'Registration',
      sortKey: 'registrationNo',
      render: (v) => v.registrationNo,
    },
    { key: 'type', header: 'Type', sortKey: 'type', render: (v) => v.type },
    { key: 'seats', header: 'Seats', sortKey: 'seats', render: (v) => v.seats },
    { key: 'rate', header: 'Rate per km', sortKey: 'ratePerKmLkr', render: (v) => formatLkr(v.ratePerKmLkr) },
    { key: 'active', header: 'Status', render: (v) => (v.isActive ? 'Active' : 'Inactive') },
    {
      key: 'delete',
      header: 'Delete',
      render: (v) => <DeleteButton label={v.registrationNo} onClick={() => setToDelete(v)} />,
    },
  ];

  return (
    <section className="space-y-4">
      <PageHeader
        title="Vehicles"
        description="The fleet the Resource agent chooses from; seats must fit the group."
        actions={
          <button type="button" className="btn-primary" onClick={() => openForm(null)}>
            Add vehicle
          </button>
        }
      />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search registration or type',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'type',
            label: 'Type',
            value: query.type,
            options: VEHICLE_TYPES.map((t) => ({ value: t, label: t })),
            onChange: (type) => list.set({ type }),
          },
          {
            name: 'minSeats',
            label: 'Seats at least',
            value: query.minSeats,
            options: ['3', '6', '10'].map((s) => ({ value: s, label: s })),
            onChange: (minSeats) => list.set({ minSeats }),
          },
        ]}
      />
      <PageState
        isLoading={vehicles.isLoading}
        isError={vehicles.isError}
        error={vehicles.error}
        onRetry={() => vehicles.refetch()}
        isEmpty={vehicles.data?.total === 0}
        emptyTitle="No vehicles found"
      >
        {vehicles.data && (
          <DataTable
            caption="Vehicles"
            columns={columns}
            rows={vehicles.data.items}
            getRowId={(v) => v.id}
            rowLabel={(v) => v.registrationNo}
            total={vehicles.data.total}
            page={vehicles.data.page}
            pageSize={vehicles.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
            onRowClick={openForm}
          />
        )}
      </PageState>
      <VehicleFormDialog open={formOpen} vehicle={editing} onClose={() => setFormOpen(false)} />
      <ConfirmDialog
        open={toDelete !== null}
        title="Delete vehicle"
        message={`Delete ${toDelete?.registrationNo ?? ''}? It will no longer be offered.`}
        confirmLabel="Delete"
        tone="danger"
        isPending={remove.isPending}
        onCancel={() => setToDelete(null)}
        onConfirm={() =>
          toDelete &&
          remove.mutate(toDelete.id, {
            onSuccess: () => {
              toast.success(`Deleted ${toDelete.registrationNo}.`);
              setToDelete(null);
            },
            onError: (error) => toast.error(getErrorMessage(error)),
          })
        }
      />
    </section>
  );
}
