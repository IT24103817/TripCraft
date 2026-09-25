import type { ReactNode } from 'react';
import { PAGE_SIZES } from '../hooks/useListParams';

export interface Column<T> {
  key: string;
  header: string;
  render: (row: T) => ReactNode;
  /** API sort field (e.g. "startDate"). Only columns with a sortKey get a sort button. */
  sortKey?: string;
  className?: string;
}

interface DataTableProps<T> {
  caption: string;
  columns: Column<T>[];
  rows: T[];
  getRowId: (row: T) => string;
  total: number;
  page: number;
  pageSize: number;
  /** Current API sort value: "field" ascending or "-field" descending. */
  sort?: string;
  onSortChange?: (sort: string) => void;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  onRowClick?: (row: T) => void;
  rowLabel?: (row: T) => string;
}

/** Server-driven table: sorting and paging only change parameters; the API does the work. */
export function DataTable<T>(props: DataTableProps<T>) {
  const { columns, rows, sort = '', page, pageSize, total } = props;
  const pageCount = Math.max(1, Math.ceil(total / pageSize));

  const toggleSort = (key: string) => props.onSortChange?.(sort === key ? `-${key}` : key);

  return (
    <div className="card overflow-hidden p-0">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
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
                    className="whitespace-nowrap px-4 py-2 text-left font-semibold text-slate-700"
                  >
                    {column.sortKey && props.onSortChange ? (
                      <button
                        type="button"
                        className="inline-flex items-center gap-1 hover:text-indigo-700 focus-visible:underline"
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
                className={props.onRowClick ? 'cursor-pointer hover:bg-slate-50' : undefined}
              >
                {columns.map((column) => (
                  <td
                    key={column.key}
                    className={column.className ?? 'whitespace-nowrap px-4 py-2 text-slate-700'}
                  >
                    {column.render(row)}
                  </td>
                ))}
                {props.onRowClick && (
                  <td className="px-4 py-2 text-right">
                    <button
                      type="button"
                      className="text-indigo-700 hover:underline"
                      aria-label={`Open ${props.rowLabel?.(row) ?? 'row'}`}
                      onClick={(event) => {
                        event.stopPropagation();
                        props.onRowClick?.(row);
                      }}
                    >
                      Open
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
        className="flex flex-wrap items-center justify-between gap-2 border-t border-slate-200 px-4 py-2 text-sm"
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
                className="rounded border border-slate-300 px-1 py-1"
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
          <button
            type="button"
            className="btn-secondary px-2 py-1"
            disabled={page <= 1}
            onClick={() => props.onPageChange(page - 1)}
          >
            Previous
          </button>
          <span aria-current="page">
            Page {page} of {pageCount}
          </span>
          <button
            type="button"
            className="btn-secondary px-2 py-1"
            disabled={page >= pageCount}
            onClick={() => props.onPageChange(page + 1)}
          >
            Next
          </button>
        </div>
      </nav>
    </div>
  );
}
