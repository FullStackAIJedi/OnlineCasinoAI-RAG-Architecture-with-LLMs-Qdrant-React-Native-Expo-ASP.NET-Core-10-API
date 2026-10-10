import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";

import { onUnauthorized } from "../api/client";
import { AUTH_QUERY_KEYS } from "../constants/authKeys";

/**
 * Subscribes to unauthorized (401) events from the API client and
 * invalidates auth-related queries so auth state is re-evaluated.
 *
 * Intended to be used once, in the root layout (e.g. inside AuthGuard).
 */
export function useUnauthorizedInvalidation() {
  const qc = useQueryClient();

  useEffect(() => {
    const unsubscribe = onUnauthorized(() => {
      qc.invalidateQueries({ queryKey: AUTH_QUERY_KEYS.hasCookie });
      qc.invalidateQueries({ queryKey: AUTH_QUERY_KEYS.me });
    });

    return unsubscribe;
  }, [qc]);
}
