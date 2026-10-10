/**
 * Error thrown by {@link apiFetch} for any non-2xx HTTP response.
 *
 * Carries the HTTP status code alongside the message, so callers can
 * distinguish between authentication failures (401), validation errors (400),
 * server errors (5xx), etc.
 *
 * @example
 * try {
 *   await authApi.me();
 * } catch (e) {
 *   if (e instanceof ApiError && e.status === 401) {
 *     // session expired
 *   }
 * }
 */
export class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = "ApiError";
  }
}
