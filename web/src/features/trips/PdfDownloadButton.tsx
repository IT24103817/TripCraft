import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { useToast } from '@/shared/components/Toast';
import { downloadPdf } from './api';

interface Props {
  /** API path of the PDF, e.g. /api/trips/{id}/vouchers.pdf. */
  path: string;
  fileName: string;
  label: string;
  /** Shown in a toast when the API refuses (e.g. 409: no quotation has been sent yet). */
  errorMessage: string;
}

/** Downloads a PDF that needs the bearer token (vouchers, itinerary) and says so while it is busy. */
export function PdfDownloadButton({ path, fileName, label, errorMessage }: Props) {
  const toast = useToast();
  const [busy, setBusy] = useState(false);

  const download = async () => {
    setBusy(true);
    try {
      await downloadPdf(path, fileName);
    } catch (error) {
      toast.error(getErrorMessage(error, errorMessage));
    } finally {
      setBusy(false);
    }
  };

  return (
    <button type="button" className="btn-secondary" disabled={busy} onClick={() => void download()}>
      {busy ? 'Downloading…' : label}
    </button>
  );
}
