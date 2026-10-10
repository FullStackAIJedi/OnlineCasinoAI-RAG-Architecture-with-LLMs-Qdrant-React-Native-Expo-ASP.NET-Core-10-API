import { apiFetch } from "./client";
import type {
  AuthUser,
  ChangePasswordRequest,
  SignInRequest,
  SignUpRequest,
} from "./types";

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
   * @param data - Registration payload as `SignUpRequest`.
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
   * @param data - Sign-in credentials as `SignInRequest`.
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
   * @returns A confirmation message from the server.
   */
  logout: () =>
    apiFetch<{ message: string }>("/api/auth/logout", { method: "POST" }),

  /**
   * Changes the current user's password.
   *
   * Requires an active session.
   *
   * @param data - Password change payload as `ChangePasswordRequest`.
   * @returns `true` if the password was changed successfully.
   */
  changePassword: (data: ChangePasswordRequest) =>
    apiFetch<boolean>("/api/auth/change-password", {
      method: "POST",
      body: data,
    }),

  /**
   * Fetches the currently authenticated user's identity.
   *
   * Used as a lightweight session check — a `401` means the cookie is
   * expired or invalid, while a `200` confirms an active session.
   *
   * @returns The authenticated user's identity as `AuthUser`.
   */
  me: () => apiFetch<AuthUser>("/api/auth/me"),
};
