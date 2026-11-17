import { ApiError } from "./errors";

export interface AskRequest {
  question: string;
}

/**
 * Response from Casino.RagService.
 *
 * Corresponds to `Casino.RagService.Models.ChatResponse`.
 */
export interface AskResponse {
  answer: string;
  sources: string[];
}

/**
 * Base URL of Casino.RagService.
 *
 * Read once at module load. Falls back to localhost for development.
 * In production, `EXPO_PUBLIC_RAG_URL` must be set (see `.env`).
 */
const RAG_BASE_URL = process.env.EXPO_PUBLIC_RAG_URL ?? "http://localhost:8081";

/** Request timeout for RAG calls (LLM + retrieval can be slow). */
const REQUEST_TIMEOUT_MS = 30_000;

/**
 * Error envelope returned by Casino.RagService for non-2xx responses.
 * Mirrors the `{ message }` shape produced by `PolicyChatController`.
 */
interface RagErrorResponse {
  message?: string;
  title?: string;
}

/**
 * Performs an HTTP request against Casino.RagService.
 *
 * Deliberately separate from `apiFetch` in `client.ts` because:
 * - RAG lives on a different host/port (`EXPO_PUBLIC_RAG_URL`).
 * - RAG endpoints are public — no auth cookie is attached.
 * - RAG latency profile differs (LLM + vector search), so a longer
 *   timeout and its own retry policy are appropriate.
 *
 * Responsibilities:
 * - Serializes the body as JSON.
 * - Applies a per-request timeout via `AbortController`.
 * - Normalizes non-2xx responses into {@link ApiError}.
 * - Tolerates non-JSON responses (HTML error pages, plain text).
 *
 * @param path - API path relative to {@link RAG_BASE_URL}.
 * @param options - Fetch options plus an optional JSON `body`.
 * @returns The parsed response body, typed as `T`.
 * @throws {ApiError} For any non-2xx response or timeout.
 */
async function ragFetch<T>(
  path: string,
  options: Omit<RequestInit, "body"> & { body?: unknown } = {},
): Promise<T> {
  const { body, headers, ...rest } = options;
  const url = `${RAG_BASE_URL}${path}`;

  console.log("[RAG] BASE_URL =", RAG_BASE_URL);
  console.log("[RAG] Full URL =", url);
  console.log("[RAG] Body =", JSON.stringify(body));

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  let res: Response;
  try {
    res = await fetch(url, {
      ...rest,
      signal: controller.signal,
      headers: {
        Accept: "application/json",
        ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
        ...headers,
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch (e) {
    if (e instanceof Error && e.name === "AbortError") {
      throw new ApiError(408, "RAG request timed out");
    }
    // Network-level failure (DNS, connection refused, etc.)
    const message = e instanceof Error ? e.message : "Network request failed";
    throw new ApiError(0, `RAG service unreachable: ${message}`);
  } finally {
    clearTimeout(timeoutId);
  }

  // 204 No Content — nothing to parse.
  if (res.status === 204) {
    return undefined as T;
  }

  const text = await res.text();
  const data: unknown = text ? safeJsonParse(text) : null;

  if (!res.ok) {
    const err = data as RagErrorResponse | null;
    const message =
      err?.message ?? err?.title ?? `RAG request failed (HTTP ${res.status})`;
    throw new ApiError(res.status, message);
  }

  return data as T;
}

/**
 * Parses a string as JSON, returning the raw string if parsing fails.
 *
 * Used to tolerate non-JSON responses (HTML error pages, plain text)
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
 * RAG API client for Casino.RagService.
 *
 * Endpoint: `POST /api/chat/policy-question`
 * Public endpoint — no auth cookie is attached.
 */
export const ragApi = {
  /**
   * Sends a question to the policy Q&A service.
   *
   * @param question - The user's question (must not be empty).
   * @returns The answer and a list of sources (policy file names).
   * @throws {ApiError} For any non-2xx response or timeout.
   */
  ask: (question: string) =>
    ragFetch<AskResponse>("/api/chat/policy-question", {
      method: "POST",
      body: { question } satisfies AskRequest,
    }),
};
