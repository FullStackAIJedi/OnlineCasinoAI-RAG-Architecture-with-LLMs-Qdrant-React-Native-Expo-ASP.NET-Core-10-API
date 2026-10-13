import type { PagedResult } from "../../src/api/types";

export const makePagedResult = <T>(
  items: T[] = [],
  overrides: Partial<PagedResult<T>> = {},
): PagedResult<T> => ({
  items,
  page: 1,
  pageSize: 20,
  total: items.length,
  ...overrides,
});
