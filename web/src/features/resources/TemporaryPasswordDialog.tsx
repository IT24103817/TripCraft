import { useState } from 'react';
import { Dialog } from '@/shared/components/Dialog';
import { guideShareMessage } from './guideMessage';
import type { GuideAccountDto } from './types';

interface Props {
  /** The login and its temporary password, straight from the API answer (never stored). */
  account: GuideAccountDto;
  title: string;
  onClose: () => void;
}

/** What was copied last, so the status line says the right thing. */
type CopyState = 'idle' | 'password' | 'message' | 'failed';

const COPY_STATUS: Record<CopyState, string> = {
  idle: '',
  password: 'Copied to the clipboard.',
  message: 'Message copied to the clipboard.',
  failed: 'Copying failed: select the text and copy it by hand.',
};

/**
 * Shows a guide's one-time temporary password after "Add guide" or "Reset password". The API never shows it
 * again, so the manager copies it now — on its own, or inside a ready-made message — and gives it to the guide,
 * who must change it at the first login.
 */
export function TemporaryPasswordDialog({ account, title, onClose }: Props) {
  const [copyState, setCopyState] = useState<CopyState>('idle');
  const message = guideShareMessage(account.email, account.temporaryPassword);

  const copy = async (text: string, what: 'password' | 'message') => {
    try {
      await navigator.clipboard.writeText(text);
      setCopyState(what);
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
        <section aria-labelledby="share-with-guide" className="space-y-2">
          <h3 id="share-with-guide" className="font-semibold text-slate-900">
            Share with guide
          </h3>
          <p className="select-all rounded-md border border-slate-200 bg-slate-50 p-3 text-slate-900">
            {message}
          </p>
          <button type="button" className="btn-secondary" onClick={() => void copy(message, 'message')}>
            Copy message
          </button>
        </section>
        <p role="status" className="text-xs text-slate-600">
          {COPY_STATUS[copyState]}
        </p>
        <div className="flex justify-end gap-2">
          <button
            type="button"
            className="btn-secondary"
            onClick={() => void copy(account.temporaryPassword, 'password')}
          >
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
