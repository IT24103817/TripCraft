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
    { id: 'r2', hotelId: 'h1', name: 'Family Room', capacity: 4, ratePerNightLkr: 18000, totalRooms: 2 },
  ],
};

describe('Hotel form with room types', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('needs at least one room type', async () => {
    let posted = false;
    server.use(
      http.get(`${API}/api/hotels`, () => HttpResponse.json(paged([]))),
      http.post(`${API}/api/hotels`, () => {
        posted = true;
        return HttpResponse.json(KANDY_HILLS, { status: 201 });
      }),
    );
    const { user } = renderApp('/resources/hotels');

    await user.click((await screen.findAllByRole('button', { name: 'Add hotel' }))[0]!);
    const dialog = await screen.findByRole('dialog', { name: 'Add hotel' });
    await user.type(within(dialog).getByLabelText('Name'), 'Ella Rock View');
    await user.type(within(dialog).getByLabelText('City'), 'Ella');
    await user.click(within(dialog).getByRole('button', { name: 'Remove room type 1' }));
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await within(dialog).findByText('Add at least one room type.')).toBeInTheDocument();
    expect(posted).toBe(false);
  });

  it('refuses two room types with the same name and checks each row', async () => {
    server.use(http.get(`${API}/api/hotels`, () => HttpResponse.json(paged([]))));
    const { user } = renderApp('/resources/hotels');

    await user.click((await screen.findAllByRole('button', { name: 'Add hotel' }))[0]!);
    const dialog = await screen.findByRole('dialog', { name: 'Add hotel' });
    await user.type(within(dialog).getByLabelText('Room type 1 name'), 'Double');
    await user.click(within(dialog).getByRole('button', { name: 'Add room type' }));
    await user.type(within(dialog).getByLabelText('Room type 2 name'), 'double ');
    await user.clear(within(dialog).getByLabelText('Room type 2 sleeps'));
    await user.type(within(dialog).getByLabelText('Room type 2 sleeps'), '12');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await within(dialog).findByText('Each room type needs a different name.')).toBeInTheDocument();
    expect(within(dialog).getByLabelText('Room type 2 name')).toHaveAttribute('aria-invalid', 'true');
    expect(within(dialog).getByLabelText('Room type 2 sleeps')).toHaveAttribute('aria-invalid', 'true');
  });

  it('creates a hotel with its room types in one body', async () => {
    let body: unknown = null;
    server.use(
      http.get(`${API}/api/hotels`, () => HttpResponse.json(paged([]))),
      http.post(`${API}/api/hotels`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({ ...KANDY_HILLS, name: 'Ella Rock View' }, { status: 201 });
      }),
    );
    const { user } = renderApp('/resources/hotels');

    await user.click((await screen.findAllByRole('button', { name: 'Add hotel' }))[0]!);
    const dialog = await screen.findByRole('dialog', { name: 'Add hotel' });
    await user.type(within(dialog).getByLabelText('Name'), 'Ella Rock View');
    await user.type(within(dialog).getByLabelText('City'), 'Ella');
    await user.type(within(dialog).getByLabelText('Room type 1 name'), 'Deluxe Double');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Added Ella Rock View.')).toBeInTheDocument();
    expect(body).toEqual({
      name: 'Ella Rock View',
      city: 'Ella',
      starRating: 3,
      latitude: 7.2906,
      longitude: 80.6337,
      isActive: true,
      roomTypes: [{ name: 'Deluxe Double', capacity: 2, ratePerNightLkr: 12000, totalRooms: 5 }],
    });
  });

  it('keeps the ids of existing room types, adds new ones without an id and leaves removed ones out', async () => {
    let body: { roomTypes?: unknown } | null = null;
    server.use(
      http.get(`${API}/api/hotels`, () => HttpResponse.json(paged([KANDY_HILLS]))),
      http.put(`${API}/api/hotels/h1`, async ({ request }) => {
        body = (await request.json()) as { roomTypes?: unknown };
        return HttpResponse.json(KANDY_HILLS);
      }),
    );
    const { user } = renderApp('/resources/hotels');

    await user.click(await screen.findByRole('button', { name: 'Open Kandy Hills' }));
    const dialog = await screen.findByRole('dialog', { name: 'Edit hotel' });
    await user.click(within(dialog).getByRole('button', { name: 'Remove room type 2' }));
    await user.click(within(dialog).getByRole('button', { name: 'Add room type' }));
    await user.type(within(dialog).getByLabelText('Room type 2 name'), 'Triple');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Saved Kandy Hills.')).toBeInTheDocument();
    expect(body!.roomTypes).toEqual([
      { id: 'r1', name: 'Standard Double', capacity: 2, ratePerNightLkr: 12000, totalRooms: 5 },
      { name: 'Triple', capacity: 2, ratePerNightLkr: 12000, totalRooms: 5 },
    ]);
  });
});
