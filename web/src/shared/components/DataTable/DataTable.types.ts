import type { ReactNode } from 'react';

export interface Column<T> {
  key: string;
  header: string;
  render: (row: T) => ReactNode;
  /** API sort field (e.g. "startDate"). Only columns with a sortKey get a sort button. */
  sortKey?: string;
  className?: string;
}

export interface DataTableProps<T> {
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
  className?: string;
}
