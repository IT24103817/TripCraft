import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { useToast } from '@/shared/components/Toast';
import { downloadVouchersPdf } from './api';

/** Downloads the printable vouchers (one page per voucher, with its QR code) of a confirmed trip. */
export function VouchersButton({ tripId }: { tripId: string }) {
  const toast = useToast();
  const [busy, setBusy] = useState(false);

  const download = async () => {
    setBusy(true);
    try {
      await downloadVouchersPdf(tripId);
    } catch (error) {
      toast.error(getErrorMessage(error, 'Could not download the vouchers.'));
    } finally {
      setBusy(false);
    }
  };

  return (
    <button type="button" className="btn-secondary" disabled={busy} onClick={() => void download()}>
      {busy ? 'Downloading…' : 'Download vouchers (PDF)'}
    </button>
  );
}
