import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const KANDY_HILLS = {
  id: 'h1',
  name: 'Kandy Hills',
  city: 'Kandy',
  starRating: 4,
  latitude: 7.29,
  longitude: 80.63,
  isActive: true,
  roomTypes: [
    { id: 'r1', hotelId: 'h1', name: 'Standard Double', capacity: 2, ratePerNightLkr: 12000, totalRooms: 5 },
  ],
};

describe('Vehicles and hotels', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('lists vehicles and shows the error state when the API fails', async () => {
    server.use(
      http.get(`${API}/api/vehicles`, () =>
        HttpResponse.json(
          paged([
            {
              id: 'v1',
              registrationNo: 'CAB-1234',
              type: 'Van',
              seats: 6,
              ratePerKmLkr: 120,
              isActive: true,
            },
          ]),
        ),
      ),
    );
    renderApp('/resources/vehicles');
    expect(await screen.findByText('CAB-1234')).toBeInTheDocument();

    server.use(http.get(`${API}/api/vehicles`, () => HttpResponse.json({ title: 'Boom' }, { status: 500 })));
    renderApp('/resources/vehicles?search=x');
    expect(await screen.findByText('Could not load this page')).toBeInTheDocument();
  });

  it('adds a room type to a hotel', async () => {
    let added: unknown = null;
    server.use(
      http.get(`${API}/api/hotels`, () => HttpResponse.json(paged([KANDY_HILLS]))),
      http.post(`${API}/api/hotels/h1/room-types`, async ({ request }) => {
        added = await request.json();
        return HttpResponse.json({ id: 'r2', hotelId: 'h1', ...(added as object) }, { status: 201 });
      }),
    );
    const { user } = renderApp('/resources/hotels');

    await user.click(await screen.findByRole('button', { name: 'Room types of Kandy Hills' }));
    const dialog = await screen.findByRole('dialog', { name: 'Room types — Kandy Hills' });
    expect(within(dialog).getByRole('list', { name: 'Room types' })).toHaveTextContent('Standard Double');
    await user.type(within(dialog).getByLabelText('Room type name'), 'Family Room');
    await user.clear(within(dialog).getByLabelText('Sleeps'));
    await user.type(within(dialog).getByLabelText('Sleeps'), '4');
    await user.click(within(dialog).getByRole('button', { name: 'Add room type' }));

    expect(await screen.findByText('Added Family Room.')).toBeInTheDocument();
    expect(added).toMatchObject({ name: 'Family Room', capacity: 4 });
  });
});
