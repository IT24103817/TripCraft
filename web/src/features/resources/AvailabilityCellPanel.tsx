import { useState } from 'react';
import { Link } from 'react-router-dom';
import { getErrorMessage } from '@/shared/api/errors';
import { Dialog } from '@/shared/components/Dialog';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useToast } from '@/shared/components/Toast';
import { formatDate } from '@/shared/utils/format';
import { useCreateHold, useHold, useReleaseHold, useUpdateHold } from './api';
import { CELL_LOOK, cellSummary, freeText } from './availabilityCells';
import { formatDayMonth } from './availabilityDates';
import { BlockForm } from './BlockForm';
import type { AvailabilityCellDto, AvailabilityRowDto, UpdateHoldRequest } from './types';

export interface CellSelection {
  row: AvailabilityRowDto;
  cell: AvailabilityCellDto;
}

interface Props {
  selection: CellSelection;
  onClose: () => void;
}

/**
 * The side panel for one grid cell. A free cell offers a new manual block; a manual block can be edited or
 * released; a trip's hold shows who it is for, with a link to the trip.
 */
export function AvailabilityCellPanel({ selection, onClose }: Props) {
  const { row, cell } = selection;
  return (
    <Dialog open placement="side" title={`${row.name}, ${formatDayMonth(cell.date)}`} onClose={onClose}>
      <div className="space-y-4 text-sm">
        <p className="flex flex-wrap items-center gap-2 text-slate-700">
          <StatusBadge status={cell.state} label={CELL_LOOK[cell.state].label} />
          {row.detail}
        </p>
        {cell.state === 'Free' && <CreateBlock selection={selection} onDone={onClose} />}
        {cell.state === 'Blocked' && cell.holdId && (
          <EditBlock holdId={cell.holdId} selection={selection} onDone={onClose} />
        )}
        {(cell.state === 'Held' || cell.state === 'Confirmed') && <TripHoldDetails selection={selection} />}
        <button type="button" className="btn-secondary w-full" onClick={onClose}>
          Close
        </button>
      </div>
    </Dialog>
  );
}

function TripHoldDetails({ selection }: { selection: CellSelection }) {
  const { row, cell } = selection;
  const free = freeText(row, cell);
  return (
    <div className="space-y-3">
      <p className="text-slate-900">{cellSummary(cell)}</p>
      <dl className="grid grid-cols-2 gap-2">
        <dt className="text-slate-500">Tourist</dt>
        <dd className="text-slate-900">{cell.touristName ?? '—'}</dd>
        <dt className="text-slate-500">Trip status</dt>
        <dd>{cell.tripStatus ? <StatusBadge status={cell.tripStatus} /> : '—'}</dd>
        <dt className="text-slate-500">Held</dt>
        <dd className="text-slate-900">{cell.heldQuantity}</dd>
        {free && (
          <>
            <dt className="text-slate-500">Free</dt>
            <dd className="text-slate-900">{free}</dd>
          </>
        )}
      </dl>
      {cell.tripRequestId && (
        <Link to={`/trips/${cell.tripRequestId}`} className="btn-primary w-full">
          Open trip
        </Link>
      )}
    </div>
  );
}

function CreateBlock({ selection, onDone }: { selection: CellSelection; onDone: () => void }) {
  const { row, cell } = selection;
  const toast = useToast();
  const create = useCreateHold();
  const isRoom = row.resourceType === 'Room';

  const save = (block: UpdateHoldRequest) =>
    create.mutate(
      { resourceType: row.resourceType, resourceId: row.resourceId, ...block },
      {
        onSuccess: () => {
          toast.success(`Blocked ${row.name}.`);
          onDone();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    );

  return (
    <section aria-label="New block" className="space-y-2">
      <h3 className="font-semibold text-slate-900">Block this {isRoom ? 'room type' : 'resource'}</h3>
      <p className="text-slate-600">A block keeps it out of new plans, e.g. for leave or maintenance.</p>
      <BlockForm
        mode="create"
        initial={{ reason: 'Leave', details: '', fromDate: cell.date, toDate: cell.date, quantity: 1 }}
        maxQuantity={isRoom ? Math.max(cell.freeQuantity, 1) : 1}
        showQuantity={isRoom}
        isPending={create.isPending}
        submitLabel="Block"
        onSubmit={save}
      />
    </section>
  );
}

function EditBlock({
  holdId,
  selection,
  onDone,
}: {
  holdId: string;
  selection: CellSelection;
  onDone: () => void;
}) {
  const { row } = selection;
  const toast = useToast();
  const hold = useHold(holdId);
  const update = useUpdateHold();
  const release = useReleaseHold();
  const [confirmRelease, setConfirmRelease] = useState(false);
  const isRoom = row.resourceType === 'Room';

  const save = (block: UpdateHoldRequest) =>
    update.mutate(
      { id: holdId, body: block },
      {
        onSuccess: () => {
          toast.success(`Saved the block on ${row.name}.`);
          onDone();
        },
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    );

  const releaseBlock = () =>
    release.mutate(holdId, {
      onSuccess: () => {
        toast.success(`Released ${row.name}.`);
        onDone();
      },
      onError: (error) => toast.error(getErrorMessage(error)),
    });

  return (
    <PageState
      isLoading={hold.isLoading}
      isError={hold.isError}
      error={hold.error}
      onRetry={() => hold.refetch()}
    >
      {hold.data && (
        <section aria-label="Manual block" className="space-y-3">
          <p className="text-slate-700">
            Blocked {formatDate(hold.data.fromDate)} – {formatDate(hold.data.toDate)}
            {isRoom && ` · ${hold.data.quantity} rooms`}
          </p>
          <BlockForm
            mode="edit"
            initial={{
              reason: 'Other',
              details: hold.data.note ?? '',
              fromDate: hold.data.fromDate,
              toDate: hold.data.toDate,
              quantity: hold.data.quantity,
            }}
            maxQuantity={isRoom ? row.capacity : 1}
            showQuantity={isRoom}
            isPending={update.isPending}
            submitLabel="Save block"
            onSubmit={save}
          />
          {confirmRelease ? (
            <div
              role="group"
              aria-label="Confirm release"
              className="space-y-2 rounded-md border border-red-200 bg-red-50 p-3"
            >
              <p className="text-red-800">Release this block? {row.name} becomes free on these days.</p>
              <div className="flex gap-2">
                <button
                  type="button"
                  className="btn-danger"
                  disabled={release.isPending}
                  onClick={releaseBlock}
                >
                  Yes, release
                </button>
                <button type="button" className="btn-secondary" onClick={() => setConfirmRelease(false)}>
                  Keep it
                </button>
              </div>
            </div>
          ) : (
            <button type="button" className="btn-danger w-full" onClick={() => setConfirmRelease(true)}>
              Release block
            </button>
          )}
        </section>
      )}
    </PageState>
  );
}
