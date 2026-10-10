import * as SecureStore from "expo-secure-store";

import { ApiError } from "./errors";
import type { ErrorResponse } from "./types";

/** Base URL of the backend API. */
const BASE_URL = process.env.EXPO_PUBLIC_API_URL;
if (!BASE_URL) {
  throw new Error(
    "EXPO_PUBLIC_API_URL is not set. Create a .env file in the project root.",
  );
}

/** SecureStore key under which the auth cookie is persisted. */
const COOKIE_KEY = "access_token_cookie";

/** Request timeout in milliseconds. */
const REQUEST_TIMEOUT_MS = 15000;

/**
 * Options accepted by {@link apiFetch}.
 *
 * Extends the native `RequestInit` but replaces `body` with a plain object
 * that is automatically JSON-serialized.
 */
interface FetchOptions extends Omit<RequestInit, "body"> {
  /** Request body, automatically JSON-serialized. */
  body?: unknown;
  /** Skip attaching the stored auth cookie (used by public endpoints). */
  skipAuth?: boolean;
}

type Listener = () => void;
const unauthorizedListeners = new Set<Listener>();

/**
 * Subscribes to "unauthorized" events, emitted whenever a non-public
 * request returns 401.
 *
 * Typical use: navigate to the login screen and clear app state.
 *
 * @param listener - Callback invoked on every unauthorized event.
 * @returns An unsubscribe function. Call it on cleanup (e.g. in `useEffect`).
 */
export function onUnauthorized(listener: Listener): () => void {
  unauthorizedListeners.add(listener);
  return () => {
    unauthorizedListeners.delete(listener);
  };
}

/** Notifies all registered listeners that the session is no longer valid. */
function emitUnauthorized(): void {
  unauthorizedListeners.forEach((l) => l());
}

/**
 * Performs an HTTP request against the backend API.
 *
 * Responsibilities:
 * - Attaches the persisted auth cookie (unless `skipAuth` is set).
 * - Persists any `Set-Cookie` returned by the server into SecureStore.
 * - Parses the response body as JSON (falling back to plain text).
 * - Normalizes errors into {@link ApiError}.
 * - On 401, clears the stored cookie and emits an "unauthorized" event
 *   so listeners can react (e.g. redirect to login).
 *
 * @param path - API path, relative to {@link BASE_URL} (e.g. `/api/auth/me`).
 * @param options - Fetch options plus app-specific flags (`skipAuth`, `body`).
 * @returns The parsed response body, typed as `T`.
 * @throws {ApiError} For any non-2xx response.
 */
async function apiFetch<T>(
  path: string,
  options: FetchOptions = {},
): Promise<T> {
  const { body, headers, skipAuth, ...rest } = options;
  const url = `${BASE_URL}${path}`;

  const savedCookie = skipAuth
    ? null
    : await SecureStore.getItemAsync(COOKIE_KEY);

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  let res: Response;
  try {
    res = await fetch(url, {
      ...rest,
      signal: controller.signal,
      credentials: "include",
      headers: {
        Accept: "application/json",
        ...(savedCookie ? { Cookie: savedCookie } : {}),
        ...(body ? { "Content-Type": "application/json" } : {}),
        ...headers,
      },
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch (e) {
    if (e instanceof Error && e.name === "AbortError") {
      throw new ApiError(408, "Request timed out");
    }
    throw e;
  } finally {
    clearTimeout(timeoutId);
  }

  const setCookie = res.headers.get("set-cookie");
  if (setCookie) {
    const match = setCookie.match(/access_token=([^;]+)/);
    if (match) {
      const cookieValue = `access_token=${match[1]}`;
      await SecureStore.setItemAsync(COOKIE_KEY, cookieValue);
    }
  }

  if (res.status === 204) {
    return undefined as T;
  }

  const text = await res.text();
  const data = text ? safeJsonParse(text) : null;

  if (!res.ok) {
    if (res.status === 401) {
      await SecureStore.deleteItemAsync(COOKIE_KEY);
      if (!skipAuth) {
        emitUnauthorized();
      }
    }

    const err = data as ErrorResponse | null;
    let message = "Something went wrong";
    if (err?.message) message = err.message;
    else if (err?.title) message = err.title;
    else if (err?.errors) {
      const firstError = Object.values(err.errors)[0];
      if (firstError?.length) message = firstError[0];
    }

    throw new ApiError(err?.statusCode ?? err?.status ?? res.status, message);
  }

  return data as T;
}

/**
 * Parses a string as JSON, returning the raw string if parsing fails.
 *
 * Used to tolerate non-JSON responses (HTML error pages, plain text, etc.)
 * without crashing the request pipeline.
 *
 * @param text - The raw response text.
 * @returns The parsed value, or the original string.
 */
function safeJsonParse(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

/**
 * Removes the persisted auth cookie.
 *
 * Used on explicit logout and whenever the server reports the session
 * as invalid.
 */
export async function clearCookie(): Promise<void> {
  await SecureStore.deleteItemAsync(COOKIE_KEY);
}

/**
 * Checks whether an auth cookie is currently stored locally.
 *
 * This is a fast, offline-safe check — it does NOT validate the cookie
 * against the server. For real validation, call `authApi.me()`.
 *
 * @returns `true` if a cookie exists in SecureStore, otherwise `false`.
 */
export async function hasCookie(): Promise<boolean> {
  return (await SecureStore.getItemAsync(COOKIE_KEY)) !== null;
}

export { apiFetch, BASE_URL };
