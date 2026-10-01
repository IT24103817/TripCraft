import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { NOTIFICATIONS_POLL_MS } from '@/shared/notifications/notificationsApi';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const TRIP_ID = '11111111-1111-1111-1111-111111111111';

function notification(id: string, overrides: Record<string, unknown> = {}) {
  return {
    id,
    type: 'ClientAccepted',
    title: 'Client accepted a quotation',
    body: 'Version 2 (USD 624.07) was accepted. Confirm the trip to book it.',
    tripRequestId: TRIP_ID,
    isRead: false,
    createdAt: '2026-10-01T08:00:00Z',
    ...overrides,
  };
}

describe('Notification bell', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('polls every 30 seconds', () => {
    expect(NOTIFICATIONS_POLL_MS).toBe(30_000);
  });

  it('shows the unread count, marks a clicked notification read and opens its trip', async () => {
    let readId = '';
    server.use(
      http.get(`${API}/api/notifications/mine`, () =>
        HttpResponse.json({
          unreadCount: 2,
          items: [
            notification('n1'),
            notification('n2', { type: 'ClientDeclined', title: 'Client declined a quotation' }),
            notification('n3', { title: 'Old news', isRead: true, tripRequestId: null }),
          ],
        }),
      ),
      http.post(`${API}/api/notifications/:id/read`, ({ params }) => {
        readId = String(params.id);
        return new HttpResponse(null, { status: 204 });
      }),
      http.get(`${API}/api/trip-requests/${TRIP_ID}`, () =>
        HttpResponse.json({ title: 'Not found', status: 404 }, { status: 404 }),
      ),
      http.get(`${API}/api/trip-requests/${TRIP_ID}/itinerary`, () =>
        HttpResponse.json({ title: 'Not found', status: 404 }, { status: 404 }),
      ),
      http.get(`${API}/api/trip-requests/${TRIP_ID}/history`, () => HttpResponse.json([])),
    );
    const { user, location } = renderApp('/dashboard');

    const bell = await screen.findByRole('button', { name: 'Notifications, 2 unread' });
    await user.click(bell);
    const list = screen.getByRole('list', { name: 'Notifications' });
    expect(within(list).getAllByRole('listitem')).toHaveLength(3);

    await user.click(within(list).getByRole('button', { name: /Client accepted a quotation/ }));

    await waitFor(() => expect(readId).toBe('n1'));
    await waitFor(() => expect(location()).toBe(`/trips/${TRIP_ID}`));
    expect(screen.queryByRole('list', { name: 'Notifications' })).not.toBeInTheDocument();
  });

  it('marks all notifications read', async () => {
    let unread = 1;
    let readAll = false;
    server.use(
      http.get(`${API}/api/notifications/mine`, () =>
        HttpResponse.json({ unreadCount: unread, items: [notification('n1', { isRead: unread === 0 })] }),
      ),
      http.post(`${API}/api/notifications/read-all`, () => {
        readAll = true;
        unread = 0;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const { user } = renderApp('/dashboard');

    await user.click(await screen.findByRole('button', { name: 'Notifications, 1 unread' }));
    await user.click(screen.getByRole('button', { name: 'Mark all read' }));

    expect(await screen.findByRole('button', { name: 'Notifications, 0 unread' })).toBeInTheDocument();
    expect(readAll).toBe(true);
  });

  it('says when there are no notifications', async () => {
    const { user } = renderApp('/dashboard');

    await user.click(await screen.findByRole('button', { name: 'Notifications, 0 unread' }));
    expect(screen.getByText('No notifications yet.')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Mark all read' })).toBeDisabled();
  });
});
