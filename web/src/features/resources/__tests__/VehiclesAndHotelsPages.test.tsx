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

  it("shows a hotel's room types in its details and opens the edit form from there", async () => {
    server.use(http.get(`${API}/api/hotels`, () => HttpResponse.json(paged([KANDY_HILLS]))));
    const { user } = renderApp('/resources/hotels');

    await user.click(await screen.findByRole('button', { name: 'Details of Kandy Hills' }));
    const details = await screen.findByRole('dialog', { name: 'Kandy Hills' });
    const rooms = within(details).getByRole('table', { name: 'Room types' });
    expect(within(rooms).getByRole('rowheader', { name: 'Standard Double' })).toBeInTheDocument();
    expect(rooms).toHaveTextContent('LKR 12,000.00');

    await user.click(within(details).getByRole('button', { name: 'Edit hotel' }));
    const form = await screen.findByRole('dialog', { name: 'Edit hotel' });
    expect(within(form).getByLabelText('Room type 1 name')).toHaveValue('Standard Double');
  });
});
