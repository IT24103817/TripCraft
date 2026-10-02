import { PAGE_SIZES } from '../../hooks/useListParams';
import { cn } from '../../utils/cn';
import { Button } from '../Button';
import { Card } from '../Card';
import type { DataTableProps } from './DataTable.types';

/** Server-driven table: sorting and paging only change parameters; the API does the work. */
export const DataTable = <T,>(props: DataTableProps<T>) => {
  const { columns, rows, sort = '', page, pageSize, total } = props;
  const pageCount = Math.max(1, Math.ceil(total / pageSize));

  const toggleSort = (key: string) => props.onSortChange?.(sort === key ? `-${key}` : key);

  return (
    <Card className={cn('overflow-hidden p-0', props.className)}>
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-200 text-sm tabular-nums">
          <caption className="sr-only">{props.caption}</caption>
          <thead className="bg-slate-50">
            <tr>
              {columns.map((column) => {
                const direction =
                  sort === column.sortKey
                    ? 'ascending'
                    : sort === `-${column.sortKey}`
                      ? 'descending'
                      : 'none';
                return (
                  <th
                    key={column.key}
                    scope="col"
                    aria-sort={column.sortKey ? direction : undefined}
                    className="whitespace-nowrap px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500"
                  >
                    {column.sortKey && props.onSortChange ? (
                      <button
                        type="button"
                        className={cn(
                          'inline-flex items-center gap-1 rounded hover:text-brand-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
                          direction !== 'none' && 'text-brand-700',
                        )}
                        onClick={() => toggleSort(column.sortKey!)}
                        aria-label={`Sort by ${column.header}`}
                      >
                        {column.header}
                        <span aria-hidden="true">
                          {direction === 'ascending' ? '▲' : direction === 'descending' ? '▼' : '↕'}
                        </span>
                      </button>
                    ) : (
                      column.header
                    )}
                  </th>
                );
              })}
              {props.onRowClick && (
                <th scope="col" className="px-4 py-2">
                  <span className="sr-only">Actions</span>
                </th>
              )}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {rows.map((row) => (
              // Mouse users can click anywhere on the row; keyboard users use the Open button in the last cell.
              <tr
                key={props.getRowId(row)}
                onClick={props.onRowClick ? () => props.onRowClick?.(row) : undefined}
                className={
                  props.onRowClick ? 'cursor-pointer transition-colors hover:bg-brand-50/60' : undefined
                }
              >
                {columns.map((column) => (
                  <td
                    key={column.key}
                    className={column.className ?? 'whitespace-nowrap px-4 py-3 text-slate-700'}
                  >
                    {column.render(row)}
                  </td>
                ))}
                {props.onRowClick && (
                  <td className="px-4 py-2 text-right">
                    <button
                      type="button"
                      className="rounded font-semibold text-brand-700 hover:underline focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500"
                      aria-label={`Open ${props.rowLabel?.(row) ?? 'row'}`}
                      onClick={(event) => {
                        event.stopPropagation();
                        props.onRowClick?.(row);
                      }}
                    >
                      Open <span aria-hidden="true">→</span>
                    </button>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <nav
        aria-label="Pagination"
        className="flex flex-wrap items-center justify-between gap-2 border-t border-slate-200 bg-slate-50/60 px-4 py-2 text-sm"
      >
        <span className="text-slate-600">
          {total === 0
            ? 'No results'
            : `${(page - 1) * pageSize + 1}–${Math.min(page * pageSize, total)} of ${total}`}
        </span>
        <div className="flex items-center gap-2">
          {props.onPageSizeChange && (
            <label className="flex items-center gap-1 text-slate-600">
              Rows
              <select
                className="rounded-md border border-slate-300 bg-surface px-2 py-1"
                value={pageSize}
                onChange={(e) => props.onPageSizeChange?.(Number(e.target.value))}
              >
                {PAGE_SIZES.map((size) => (
                  <option key={size} value={size}>
                    {size}
                  </option>
                ))}
              </select>
            </label>
          )}
          <Button
            variant="secondary"
            size="sm"
            disabled={page <= 1}
            onClick={() => props.onPageChange(page - 1)}
          >
            Previous
          </Button>
          <span aria-current="page">
            Page {page} of {pageCount}
          </span>
          <Button
            variant="secondary"
            size="sm"
            disabled={page >= pageCount}
            onClick={() => props.onPageChange(page + 1)}
          >
            Next
          </Button>
        </div>
      </nav>
    </Card>
  );
};
