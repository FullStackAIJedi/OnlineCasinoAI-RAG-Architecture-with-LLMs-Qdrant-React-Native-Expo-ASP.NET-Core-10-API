import { apiFetch } from "./client";
import type { AuthUser, ChangePasswordRequest, SignInRequest, SignUpRequest } from "./types";

/**
 * Authentication API client.
 *
 * All requests are routed through `apiFetch`, which handles cookie
 * persistence and automatic auth headers.
 */
export const authApi = {

  /**
   * Registers a new user account.
   *
   * Public endpoint — `skipAuth: true` prevents attaching the auth cookie,
   * since the user does not have a session yet.
   *
   * @param data - Registration payload (email, password, etc.).
   * @returns A confirmation message from the server.
   */
  register: (data: SignUpRequest) =>
    apiFetch<{ message: string }>("/api/auth/register", {
      method: "POST",
      body: data,
      skipAuth: true,
    }),

  /**
   * Authenticates the user and establishes a session.
   *
   * On success, the server sets an auth cookie that `apiFetch` persists
   * locally for subsequent authenticated requests.
   *
   * Public endpoint — `skipAuth: true` prevents sending a stale cookie.
   *
   * @param data - Sign-in credentials (email/username + password).
   * @returns A confirmation message from the server.
   */
  login: (data: SignInRequest) =>
    apiFetch<{ message: string }>("/api/auth/login", {
      method: "POST",
      body: data,
      skipAuth: true,
    }),

  /**
   * Logs the user out and invalidates the server-side session.
   *
   * The local cookie is cleared separately by the caller (`useLogout`),
   * so a network failure here does not leave the client in a logged-in state.
   *
   * @returns A confirmation message from the server.
   */
  logout: () =>
    apiFetch<{ message: string }>("/api/auth/logout", { method: "POST" }),

  /**
   * Changes the current user's password.
   *
   * Requires an active session (auth cookie is attached automatically).
   *
   * @param data - Old password (for verification) and the new password.
   * @returns `true` if the password was changed successfully.
   */
  changePassword: (data: ChangePasswordRequest) =>
    apiFetch<boolean>("/api/auth/change-password", { method: "POST", body: data }),

  /**
   * Fetches the currently authenticated user's identity.
   *
   * Used as a lightweight session check — a `401` means the cookie is
   * expired or invalid, while a `200` confirms an active session.
   *
   * @returns The authenticated user's identity (id, email, roles, etc.).
   */
  me: () => apiFetch<AuthUser>("/api/auth/me"),
};
