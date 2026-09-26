import type { UseFormRegisterReturn } from 'react-hook-form';

/** "Active" checkbox for resource forms: inactive resources are never offered to the agents. */
export function ActiveCheckbox({ registration }: { registration: UseFormRegisterReturn }) {
  return (
    <label className="flex items-center gap-2 text-sm text-slate-700">
      <input type="checkbox" className="h-4 w-4" {...registration} />
      Active (offered to the planning agents)
    </label>
  );
}
