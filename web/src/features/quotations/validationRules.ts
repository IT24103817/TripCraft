/**
 * The rules of the C# ProposalValidator (PLAN.md section 5), in display order. A rule is green when the
 * result has no violation with one of its codes. Keep the codes in sync with ProposalValidator.cs.
 */
export const VALIDATION_RULES: { label: string; codes: string[] }[] = [
  { label: 'Proposal JSON is complete', codes: ['SCHEMA_INCOMPLETE'] },
  { label: 'Every attraction exists', codes: ['UNKNOWN_ATTRACTION'] },
  { label: 'Every day has 1–3 stops', codes: ['DAY_STOPS'] },
  { label: 'Guide exists', codes: ['UNKNOWN_GUIDE'] },
  { label: 'Guide speaks the requested language', codes: ['GUIDE_LANGUAGE'] },
  { label: 'Guide is not already held on these dates', codes: ['GUIDE_HOLD_OVERLAP'] },
  { label: 'Vehicle exists', codes: ['UNKNOWN_VEHICLE'] },
  { label: 'Vehicle seats at least every traveller', codes: ['VEHICLE_SEATS'] },
  { label: 'Vehicle is not already held on these dates', codes: ['VEHICLE_HOLD_OVERLAP'] },
  { label: 'Hotels and room types exist', codes: ['UNKNOWN_HOTEL', 'UNKNOWN_ROOM_TYPE'] },
  { label: 'Rooms sleep every traveller every night', codes: ['ROOMS_BELOW_PAX'] },
  { label: 'Quotation total matches the server calculation (±1 LKR)', codes: ['QUOTATION_MISMATCH'] },
  { label: "Total is within the tourist's budget", codes: ['OVER_BUDGET'] },
];
