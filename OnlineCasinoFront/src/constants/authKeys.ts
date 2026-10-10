/**
 * Query keys for auth-related queries.
 *
 * Centralized to keep cache invalidation consistent across the app.
 */
export const AUTH_QUERY_KEYS = {
  hasCookie: ["auth", "hasCookie"] as const,
  me: ["auth", "me"] as const,
};
