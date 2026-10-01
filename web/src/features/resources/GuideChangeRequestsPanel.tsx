import { useState } from 'react';
import { Link } from 'react-router-dom';
import { getErrorMessage } from '@/shared/api/errors';
import { PageState } from '@/shared/components/PageState';
import { useToast } from '@/shared/components/Toast';
import { formatDate } from '@/shared/utils/format';
import { useGuideChangeRequests, useResolveGuideChange } from './api';
import type { GuideChangeRequestDto } from './types';

/**
 * Dashboard panel: guides who asked to be replaced on a confirmed trip. The manager picks one of the free
 * candidates and swaps the guide; the API moves the holds and tells both guides and the tourist.
 */
export function GuideChangeRequestsPanel() {
  const requests = useGuideChangeRequests();
  return (
    <div className="card space-y-3">
      <h2 className="font-semibold text-slate-900">Guide change requests</h2>
      <PageState
        isLoading={requests.isLoading}
        isError={requests.isError}
        error={requests.error}
        onRetry={() => requests.refetch()}
        isEmpty={requests.data?.length === 0}
        emptyTitle="No guide change requests"
        emptyDescription="A guide asks for a replacement from the mobile app; the request appears here."
      >
        <ul aria-label="Guide change requests" className="divide-y divide-slate-200">
          {requests.data?.map((request) => (
            <GuideChangeRow key={request.id} request={request} />
          ))}
        </ul>
      </PageState>
    </div>
  );
}

function GuideChangeRow({ request }: { request: GuideChangeRequestDto }) {
  const toast = useToast();
  const resolve = useResolveGuideChange();
  const [replacementId, setReplacementId] = useState('');
  const selectId = `replacement-${request.id}`;

  const swap = () =>
    resolve.mutate(
      { id: request.id, replacementGuideId: replacementId },
      {
        onSuccess: (resolved) =>
          toast.success(
            `${resolved.replacementGuideName ?? 'The new guide'} replaces ${request.guideName} on this trip.`,
          ),
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    );

  return (
    <li className="space-y-2 py-3 text-sm">
      <p>
        <span className="font-medium text-slate-900">{request.guideName}</span> asked to be replaced on{' '}
        <Link to={`/trips/${request.tripRequestId}`} className="text-brand-700 hover:underline">
          {request.tripObjective}
        </Link>
      </p>
      <p className="text-slate-600">
        {formatDate(request.startDate)} – {formatDate(request.endDate)} · {request.pax} travellers · language{' '}
        {request.language}
      </p>
      <p className="text-slate-700">Reason: {request.reason}</p>
      {request.candidates.length === 0 ? (
        <p className="text-amber-800">
          No other guide is free for these dates, speaks {request.language} and takes {request.pax} people.
        </p>
      ) : (
        <div className="flex flex-wrap items-end gap-2">
          <label htmlFor={selectId} className="flex flex-col gap-1 text-slate-700">
            Replacement for {request.guideName}
            <select
              id={selectId}
              className="input"
              value={replacementId}
              onChange={(e) => setReplacementId(e.target.value)}
            >
              <option value="">Choose a guide</option>
              {request.candidates.map((guide) => (
                <option key={guide.id} value={guide.id}>
                  {guide.name} ({guide.languages.join(', ')}, up to {guide.maxPax})
                </option>
              ))}
            </select>
          </label>
          <button
            type="button"
            className="btn-primary"
            disabled={replacementId === '' || resolve.isPending}
            onClick={swap}
          >
            {resolve.isPending ? 'Swapping…' : 'Swap guide'}
          </button>
        </div>
      )}
    </li>
  );
}
