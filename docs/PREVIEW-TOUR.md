# Preview tour

A click-through of the finished system on `main`, on the local stack, with a freshly seeded database.
Follow the steps in order. Each step says where to go and what you should see.

| What | Where |
|------|-------|
| Landing page (public) | <http://localhost:5199/> |
| Staff login | <http://localhost:5199/login> |
| Swagger | <http://localhost:5080/swagger> |
| API health | <http://localhost:5080/health> (should show `"db":"ok"`) |
| Agent service health | <http://127.0.0.1:8001/health> |
| Mobile app | Android emulator, `flutter run --dart-define=API_URL=http://10.0.2.2:5080` |

**Accounts.** Every seeded account uses the password **`Passw0rd!`** at `@tripcraft.test`:
- Tourists: `tourist1`, `tourist2`, `tourist3`.
- Guides: `guide1` = Nimal Perera, `guide2` = Kumari Silva, `guide3` = Ruwan Fernando. Anjali Jayasinghe has no
  login.
- Operations Managers: `manager1`, `manager2`, `manager3`.
- Admins: `admin1`, `admin2`, `admin3`.

**Login limit.** Login allows 5 attempts per minute from one machine. If you see "Too many attempts", wait a minute.

**Seed data.** The database has:
- 21 attractions in 6 cities, 4 guides, 3 vehicles, 6 hotels (one per city);
- one completed sample trip (August, for the reports);
- one trip at **Pending approval**: tourist2, 9–12 Nov 2026, 2 people, USD 1,200, Kandy then Ella. The code proposed
  guide Nimal Perera and the quotation is USD 282.04.

## React: Operations Manager

**1. Landing page.** <http://localhost:5199/> (public, no login). You should see:
- the header with the TripCraft logo, *How it works*, *Who it's for*, *Get the app* and a **Staff login** button;
- the hero "Tell us the trip you want. We plan it, price it and book it for you." with the note "Tourist? Use the
  app — this website is for our operations team.";
- four numbered steps, three audience cards, and the dark *Get the app* band. With no `VITE_APK_URL` set, it says
  "The Android download link will appear here soon.";
- the footer with the module line.

**2. Staff login.** Click **Staff login** → `/login`. The two-panel sign-in page has the dark brand panel on the
left and the form on the right. Enter `manager1@tripcraft.test` / `Passw0rd!` → **Sign in**.

**3. Dashboard.** `/dashboard`.
- The KPI cards show **Pending approvals 1**, *Trips this month*, *Revenue this month* and *Active workflows 0*.
- **Latest workflows** lists the pending trip's workflow as *Pending approval*.
- The sidebar has Dashboard, Approvals, Trip requests, Attractions, Guides, Vehicles, Hotels, Availability, Agent
  workflows, Quotations and Reports. There is no Users entry; that is the Admin's.

**4. Approvals inbox and the pending review.** Sidebar **Approvals** → `/approvals`.
- The *Pending approval* tab has one row, with the objective "4 days for 2 people in Kandy and Ella…".
- Type `ella` in the new **search** box: the row stays. Type `sigiriya`: "no match" appears. Clear the search.
- Click the **Started** header to sort.
- Click **Open proposal …** → `/approvals/{workflowId}`. You should see:
  - the trip and budget (USD 1,200);
  - **Deterministic validation**: "All deterministic checks passed", with every rule ticked;
  - the **Itinerary**: day 1 Kandy (3 stops, road), day 2 Ella (train, 140 km), days 3–4 Ella;
  - **Proposed guide, vehicle and rooms** by name: Nimal Perera, a vehicle, and rooms per night;
  - **Quotation v1 (Pending)** with named lines in LKR and USD (LKR 93,150 = USD 282.04, with the rate and when it
    was fetched);
  - the **Approve**, **Reject** and **Request revision** buttons, and **Re-price with today's rates**.
- **Approve** → confirm **Approve**. Toast: "Approved. Trip is now confirmed; N holds created." You need this
  Confirmed trip for the itinerary editor in step 6.

**5. Workflow monitor.** Sidebar **Agent workflows** → `/workflows`.
- There is a search box and an **Objective** column, and **Status**, **Started** and **Finished** sort when you click
  their headers.
- Open the row → `/workflows/{id}`. You should see:
  - Status, Started, Elapsed, Agent time and Steps;
  - **Validation result**;
  - **Agent steps**: 1 Planner / Coordinator, 2 Itinerary Analysis, 3 Resource & Action, 4 Validation & Safety, each
    with its tool calls (e.g. `check_guide_availability · … ms`), duration and retries.
- On step 3, open **Summaries and validation result**. The output shows `guide_choice`: "Nimal Perera: cheapest of 4
  available 'en' guide(s), LKR 6,000/day". It also shows `model_guide_id` and `guide_overridden`.

**6. Itinerary editor.** Sidebar **Trip requests** → `/trips`.
- Open the 9–12 Nov trip → `/trips/{id}`. The status is **Confirmed**, and the itinerary card shows "Version 1 ·
  Agent".
- Each day has **Edit day**. Click it on day 2: the dialog "Edit day 2 — Ella" lists the Ella attractions as
  checkboxes, with the current stops ticked.
- Tick a fourth: you are stopped at 3 with a message. Choose e.g. *Ravana Falls* and *Nine Arches Bridge*, add a
  note, then **Save**.
- Toast. The card now shows **Version 2 · Manual** and the new stops.
- The edit is also on the trip's **History** card, and in the Admin audit log as *ItineraryDayEdited*.
- On a trip that is not Confirmed (e.g. the Completed August trip), there is no **Edit day** button.

**7. Guides, vehicles, hotels.** Each page has search, filters, header sort and paging.
- **Guides** `/resources/guides`: 4 guides.
  - Search `ru` → only Ruwan. **Language** = `de` → only Kumari.
  - Click **Day rate** twice to sort descending. Change the page size.
  - **Add guide** with phone `12` and language `english` shows field errors before anything is sent.
- **Vehicles** `/resources/vehicles`: 3 vehicles. Filter **Type** = Van, or **Seats at least** = 10. Sort by seats.
- **Hotels** `/resources/hotels`: 6 hotels, one per city (Colombo, Galle, Kandy, Ella, Nuwara Eliya, Sigiriya).
  - Filter **City** = Ella.
  - Click the room-types cell to open the room-types dialog.
- If a filter matches nothing, the empty state offers **Add guide / vehicle / hotel**.

**8. Availability calendar.** Sidebar **Availability** → `/availability`.
- **Find available resources**: Type = Guide, 9–12 Nov 2026, language `en`, pax 2 → **Search**. Nimal is **missing**,
  because he is now held for the approved trip.
- The **Hold calendar** below shows the approved trip's guide, vehicle and room holds (teal cells) with **Release**
  buttons.
- If a window has more holds than the calendar shows, a notice says so. It will not appear with this data.

**9. Quotations with the minimum-total filter.** Sidebar **Quotations** → `/quotations`.
- You see the seeded August quotation and the approved November one.
- Set **Min total (USD)** = 300: only the quotations of USD 300 or more remain. Set it to 100000: the empty state
  offers **Clear filters**.
- Search by objective, sort by clicking a header, change the page size.

**10. Reports.** Sidebar **Reports** → `/reports`, with a period filter at the top.
- **Trip requests by status**.
- **Revenue by month**: August from the seeded trip, November after step 4.
- **Guide and vehicle utilisation**: held days as a percentage.

**11. Admin: users and audit log.** Profile menu (top right) → sign out → sign in as `admin1@tripcraft.test`.
- The sidebar shows Dashboard, Agent workflows, Users and Audit log.
- **Users** `/admin/users`: search, filter by role and status, create a user, deactivate one.
- **Audit log** `/admin/audit-logs`: who, action, entity and before → after.
  - Filter **Entity** = TripRequest to see *TripRequestCreated … QuotationApproved*, and *ItineraryDayEdited* from
    step 6.
- Opening `/approvals` as Admin shows the 403 page, with a link back to the **dashboard**.
- Sign out, then sign back in as `manager1` for step 14.

## Emulator: Tourist, then Guide

**12. Register a tourist.** The app is at **Welcome back** (Sign in).
- Tap **New here? Create an account**.
- Enter a full name, a new email (e.g. `you@example.com`), a password with upper case, lower case and a digit (8+
  characters), and a nationality → **Create account**.
- You are signed in on **My trips**, which shows the empty state "No trip requests yet".

**13. Submit the section 6 demo request with a photo.** Tab **New trip**.
- **Objective**: *5 days for 4 people, 10–14 October, Kandy and Ella, prefer the hill-country train,
  English-speaking guide.*
- **Travel dates**: tap the field, then the pencil (**Switch to input**), type `10/10/2026` and `10/14/2026` → **OK**.
- **Travellers**: tap **+** twice → 4. **Budget** 1500.
- Chips: **Hill-country train** and **English-speaking guide**.
- **Nationality** and **Passport number** (e.g. `N1234567`).
- **Passport photo**: **Camera** (allow the camera; tap the shutter, then ✓/Done) or **Gallery**. You should see
  "Passport photo selected".
- Tap **Submit trip request**. You land on **Trip request**:
  - the **Progress** timeline is at *Planning*;
  - the Planning card says "Our AI agents are planning your trip. This page updates every 10 seconds.";
  - the itinerary placeholder says the plan will appear once drafted.

**14. Status timeline, then approve in React.**
- In React (`manager1`), **Agent workflows** shows the new workflow as *Planning*. Open it and watch the four steps
  appear; this takes about 1½–2½ minutes with the local model.
- When it reaches *Pending approval*, the phone shows the **Awaiting operator approval** card, and the timeline's
  *Pending approval* step is current.
- In React: **Approvals** → **Open proposal …** for the 4-person trip.
  - Check the checklist, the itinerary, the resources (guide **Nimal Perera**) and the quotation (under USD 1,500).
  - **Approve** → confirm. Toast: "Approved. Trip is now confirmed; 6 holds created."
- Note the **guide's name** on this page; you sign in as that guide in step 17.

**15. Refresh on the phone.** On the trip page, pull down. You should see:
- **Status: Confirmed**, with the timeline's *Confirmed* step current;
- **Planning — Completed**, with **View quotation**;
- the saved **Itinerary** with dates, transport, stops and the map;
- **History** from submitted to confirmed.

Within about 30 s a **"Trip confirmed"** notification appears, and the **Alerts** tab lists it.

**16. Accept the quotation.** Tap **View quotation**. You should see:
- named items: guide, vehicle, hotel rooms, entry tickets;
- the subtotal, the 15 % service margin, and the total in LKR and USD with the rate and its date.

"Your operator approved this price. Accept it to confirm." → **Accept quotation**. You get the snackbar "Quotation
accepted." and "You accepted this price on …".

**17. Sign in as the guide; vehicle card and GPS check-in.**
- Profile icon (top right) → **Log out**. Sign in as the guide named in step 14: Nimal → `guide1`, Kumari → `guide2`,
  Ruwan → `guide3`. The code picks the cheapest free English guide, so it is `guide1` unless Nimal is already held
  on those dates.
- **My schedule** shows the 10–14 Oct trip: 4 travellers, vehicle, and the days with hotel and stops. The **Confirmed**
  filter chip narrows the list.
- Tap **Day 1** → **Today's stops**. On top is the **Vehicle** card with registration, type and seats (e.g. CAB-1234,
  Van, 6).
- Put the emulator at a day-1 stop. Either use the emulator's **…** (Extended controls) → **Location**, or run
  `adb emu geo fix <longitude> <latitude>` in a terminal. Kandy stops:

  | Stop | Longitude | Latitude |
  |------|-----------|----------|
  | Temple of the Sacred Tooth Relic | 80.6413 | 7.2936 |
  | Royal Botanical Gardens, Peradeniya | 80.5966 | 7.2685 |
  | Kandy Lake | 80.6424 | 7.2926 |
  | Bahirawakanda Vihara Buddha Statue | 80.6303 | 7.2956 |

- On that stop, tap **Find my location** and allow location ("While using the app"). You should see "You are 0 m from
  …".
- **Check in** is only enabled within 500 m. Tap it: "Checked in … Trip is in progress."
- In React, the trip is now *In progress* and the Audit log has *StopCheckedIn*.
- The **Scan voucher** tab scans a hotel voucher QR (the hotel id) and shows the hotel.

## If something goes wrong

- **A workflow ends Failed safely.** Open it in the workflow monitor: the failing step and message are shown. The
  tourist can tap **Try again** on the trip page.
- **The pending seed trip (9–12 Nov) and the demo request (10–14 Oct) never compete** for the same guide or vehicle.
- **The emulator is slow or shows "isn't responding".** Tap **Wait**. The Mac is running the model, the API and the
  emulator at the same time.
