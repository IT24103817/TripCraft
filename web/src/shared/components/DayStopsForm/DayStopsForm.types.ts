/** One attraction that can be ticked as a stop of the day. */
export interface StopOption {
  id: string;
  name: string;
  durationMinutes: number;
}

/** The attractions of the day's city, as the caller's query hook returns them. */
export interface StopOptionsQuery {
  items: StopOption[] | undefined;
  isLoading: boolean;
  isError: boolean;
  error: unknown;
  refetch: () => unknown;
}

export interface DayStopsFormProps {
  city: string;
  /** The attractions of the city: the caller loads them (each feature has its own API hooks). */
  options: StopOptionsQuery;
  initialIds: string[];
  initialNotes: string;
  /** False when the day has no notes to edit (a proposal under review). */
  showNotes?: boolean;
  /** Save request in flight, and its error (400 field messages or a 409 message). */
  isPending: boolean;
  saveError: unknown;
  onSave: (attractionIds: string[], notes: string | null) => void;
  onCancel: () => void;
  className?: string;
}
