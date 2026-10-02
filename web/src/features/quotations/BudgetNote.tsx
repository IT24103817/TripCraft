import { budgetNoteText } from './reviewRules';
import type { QuotationDto } from './types';

/**
 * The warning-tone sentence of a version sent as the best available price ("Best price we can offer — USD 132.00
 * above your budget"). Nothing is drawn for a version within budget.
 */
export function BudgetNote({ quotation }: { quotation: QuotationDto }) {
  const text = budgetNoteText(quotation);
  if (!text) return null;
  return (
    <p className="rounded-md border border-amber-300 bg-amber-50 p-3 text-sm font-medium text-amber-900">
      {text}
    </p>
  );
}
