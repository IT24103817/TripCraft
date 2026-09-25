import { useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';

export const PAGE_SIZES = [10, 20, 50, 100];

/**
 * List state (page, pageSize, sort, search and any filter) kept in the URL, so a list can be
 * bookmarked, shared and survives a refresh. Changing anything except the page goes back to page 1.
 */
export function useListParams(defaults: { sort?: string; pageSize?: number } = {}) {
  const [params, setParams] = useSearchParams();

  const page = Math.max(1, Number(params.get('page') ?? 1) || 1);
  const pageSize = Number(params.get('pageSize') ?? defaults.pageSize ?? 20) || 20;
  const sort = params.get('sort') ?? defaults.sort ?? '';
  const search = params.get('search') ?? '';

  const get = useCallback((name: string) => params.get(name) ?? '', [params]);

  const set = useCallback(
    (patch: Record<string, string | number | undefined>) => {
      setParams(
        (current) => {
          const next = new URLSearchParams(current);
          for (const [key, value] of Object.entries(patch)) {
            if (value === undefined || value === '') next.delete(key);
            else next.set(key, String(value));
          }
          if (!('page' in patch)) next.delete('page');
          return next;
        },
        { replace: true },
      );
    },
    [setParams],
  );

  return { page, pageSize, sort, search, get, set };
}
