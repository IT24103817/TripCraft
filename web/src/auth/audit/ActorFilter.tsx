import { useEffect, useState } from 'react';
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue';

interface Props {
  value: string;
  onChange: (value: string) => void;
}

/** "Actor" text filter: part of a user's email, or "system". Sent once the admin stops typing. */
export function ActorFilter({ value, onChange }: Props) {
  const [text, setText] = useState(value);
  const debounced = useDebouncedValue(text, 300);

  useEffect(() => {
    if (debounced !== value) onChange(debounced);
  }, [debounced, value, onChange]);

  return (
    <label className="flex flex-col gap-1 text-sm text-slate-700">
      Actor
      <input
        type="text"
        className="input"
        value={text}
        placeholder="Email or system"
        onChange={(e) => setText(e.target.value)}
      />
    </label>
  );
}
