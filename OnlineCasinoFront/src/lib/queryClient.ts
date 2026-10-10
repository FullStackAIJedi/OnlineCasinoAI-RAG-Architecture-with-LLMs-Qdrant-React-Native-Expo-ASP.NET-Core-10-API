import {
  QueryClient,
  QueryCache,
  MutationCache,
} from "@tanstack/react-query";

import { ApiError } from "../api/errors";
import { AUTH_QUERY_KEYS } from "../constants/authKeys";

/**
 * Checks whether an unknown error represents a 401 Unauthorized response.
 *
 * @param error - The error to inspect.
 * @returns True if the error is an ApiError with status 401.
 */
function isUnauthorized(error: unknown): boolean {
  return error instanceof ApiError && error.status === 401;
}

/**
 * Invalidates all auth-related queries so the app re-evaluates auth state.
 *
 * @param client - The QueryClient instance to invalidate queries on.
 */
function invalidateAuthQueries(client: QueryClient) {
  client.invalidateQueries({ queryKey: AUTH_QUERY_KEYS.hasCookie });
  client.invalidateQueries({ queryKey: AUTH_QUERY_KEYS.me });
}

/**
 * Creates a configured QueryClient with global error handling,
 * auth invalidation on 401, and default query options.
 *
 * Exposed as a factory to allow fresh instances for tests.
 *
 * @returns A fully configured QueryClient.
 */
export function createQueryClient(): QueryClient {
  let client!: QueryClient;

  client = new QueryClient({
    queryCache: new QueryCache({
      onError: (error) => {
        if (isUnauthorized(error)) invalidateAuthQueries(client);
      },
    }),
    mutationCache: new MutationCache({
      onError: (error) => {
        if (isUnauthorized(error)) invalidateAuthQueries(client);
      },
    }),
    defaultOptions: {
      queries: {
        retry: (failureCount, error) => {
          if (isUnauthorized(error)) return false;
          return failureCount < 1;
        },
        staleTime: 30 * 1000,
        refetchOnWindowFocus: false,
      },
    },
  });

  return client;
}

export const queryClient = createQueryClient();
