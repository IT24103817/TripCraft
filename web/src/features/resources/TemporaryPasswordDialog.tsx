import { useState } from 'react';
import { Dialog } from '@/shared/components/Dialog';
import type { GuideAccountDto } from './types';

interface Props {
  /** The login and its temporary password, straight from the API answer (never stored). */
  account: GuideAccountDto;
  title: string;
  onClose: () => void;
}

/**
 * Shows a guide's one-time temporary password after "Add guide" or "Reset password". The API never shows it
 * again, so the manager copies it now and gives it to the guide, who must change it at the first login.
 */
export function TemporaryPasswordDialog({ account, title, onClose }: Props) {
  const [copyState, setCopyState] = useState<'idle' | 'copied' | 'failed'>('idle');

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(account.temporaryPassword);
      setCopyState('copied');
    } catch {
      setCopyState('failed');
    }
  };

  return (
    <Dialog open title={title} onClose={onClose}>
      <div className="space-y-4 text-sm text-slate-700">
        <p role="alert" className="rounded-md border border-amber-300 bg-amber-50 p-3 text-amber-900">
          This password is shown only once. Copy it now and give it to {account.guide.name}; they must change
          it at their first sign-in.
        </p>
        <dl className="space-y-2">
          <div>
            <dt className="text-slate-500">Login email</dt>
            <dd className="font-medium text-slate-900">{account.email}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Temporary password</dt>
            <dd>
              <code className="select-all rounded bg-slate-100 px-2 py-1 font-mono text-base text-slate-900">
                {account.temporaryPassword}
              </code>
            </dd>
          </div>
        </dl>
        <p role="status" className="text-xs text-slate-600">
          {copyState === 'copied' && 'Copied to the clipboard.'}
          {copyState === 'failed' && 'Copying failed: select the password and copy it by hand.'}
        </p>
        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={() => void copy()}>
            Copy password
          </button>
          <button type="button" className="btn-primary" onClick={onClose}>
            Done
          </button>
        </div>
      </div>
    </Dialog>
  );
}
