import { useCallback, useLayoutEffect, useRef } from "react";

/**
 * Returns a stable callback that always invokes the latest `fn`.
 *
 * Use this when passing callbacks into effects without wanting to
 * re-trigger the effect when the callback identity changes.
 *
 * @param fn - The callback to wrap.
 * @returns A stable reference to the latest callback.
 */
export function useEvent<T extends (...args: never[]) => unknown>(fn: T): T {
  const ref = useRef(fn);

  useLayoutEffect(() => {
    ref.current = fn;
  });

  return useCallback(((...args) => ref.current(...args)) as T, []);
}
