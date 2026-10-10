import { apiFetch } from "./client";
import type { UpdateProfileRequest, UserListItem, UserProfile } from "./types";

/**
 * Users API client.
 *
 * Covers two distinct concerns:
 * - **Self-service**: `getMe`, `updateMe`, `deleteMe` — operate on the
 *   current user's own profile.
 * - **Admin**: `getAll`, `block`, `unblock` — operate on other users and
 *   require an elevated role (enforced server-side).
 */
export const usersApi = {

  /**
   * Fetches the current user's full profile.
   *
   * Prefer this over {@link authApi.me} when you need profile fields
   * (avatar, name, timestamps) rather than just session identity.
   *
   * @returns The current user's profile.
   */
  getMe: () => apiFetch<UserProfile>("/api/users/me"),

  /**
   * Updates the current user's profile.
   *
   * Full replacement (`PUT`) — fields omitted from `data` may be cleared
   * depending on server behavior, so always send the complete profile.
   *
   * @param data - The updated profile fields.
   * @returns The updated profile as persisted by the server.
   */
  updateMe: (data: UpdateProfileRequest) =>
    apiFetch<UserProfile>("/api/users/me", { method: "PUT", body: data }),

  /**
   * Permanently deletes the current user's account.
   *
   * Irreversible. The caller is responsible for clearing the session
   * (see `useDeleteAccount`).
   *
   * @returns `true` if the account was deleted successfully.
   */
  deleteMe: () => apiFetch<boolean>("/api/users/me", { method: "DELETE" }),

  /**
   * Fetches the full list of users (admin only).
   *
   * @returns All users in a compact list representation.
   */
  getAll: () => apiFetch<UserListItem[]>("/api/users"),

  /**
   * Blocks a user by ID (admin only).
   *
   * A blocked user cannot sign in or perform any authenticated actions.
   *
   * @param userId - Internal numeric ID of the user to block.
   * @returns `true` if the user was blocked successfully.
   */
  block: (userId: number) =>
    apiFetch<boolean>(`/api/users/${userId}/block`, { method: "POST" }),

  /**
   * Unblocks a previously blocked user (admin only).
   *
   * Restores the user's ability to sign in and use the app.
   *
   * @param userId - Internal numeric ID of the user to unblock.
   * @returns `true` if the user was unblocked successfully.
   */
  unblock: (userId: number) =>
    apiFetch<boolean>(`/api/users/${userId}/unblock`, { method: "POST" }),
};