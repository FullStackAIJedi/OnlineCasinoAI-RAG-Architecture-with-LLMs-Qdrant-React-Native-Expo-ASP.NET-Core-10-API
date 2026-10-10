import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "../api/users";
import type { UpdateProfileRequest } from "../api/types";

/**
 * Fetches the currently authenticated user's profile.
 *
 * Shares the `["users", "me"]` cache with `useUpdateProfile`, so a successful
 * profile update automatically refreshes this query.
 */
export const useProfile = () => {
  return useQuery({
    queryKey: ["users", "me"],
    queryFn: usersApi.getMe,
  });
};

/**
 * Updates the current user's profile.
 *
 * On success, invalidates the `["users", "me"]` cache so the UI re-renders
 * with the freshly saved data.
 *
 * Expects the mutation to be called with an `UpdateProfileRequest` payload.
 */
export const useUpdateProfile = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateProfileRequest) => usersApi.updateMe(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["users", "me"] });
    },
  });
};

/**
 * Permanently deletes the current user's account.
 *
 * No cache invalidation is performed here — the caller is expected to
 * clear the session and navigate away after a successful deletion.
 */
export const useDeleteAccount = () => {
  return useMutation({
    mutationFn: usersApi.deleteMe,
  });
};

/**
 * Fetches the full list of users (admin view).
 *
 * Shares the `["users"]` key prefix with `useBlockUser` and `useUnblockUser`,
 * so blocking or unblocking a user refreshes this list automatically.
 */
export const useAllUsers = () => {
  return useQuery({
    queryKey: ["users", "all"],
    queryFn: usersApi.getAll,
  });
};

/**
 * Blocks a user by ID (admin action).
 *
 * On success, invalidates the entire `["users"]` namespace so any user list
 * or detail view currently on screen reflects the new status.
 *
 * Expects the mutation to be called with the target `userId`.
 */
export const useBlockUser = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: number) => usersApi.block(userId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
};

/**
 * Unblocks a user by ID (admin action).
 *
 * On success, invalidates the entire `["users"]` namespace so any user list
 * or detail view currently on screen reflects the new status.
 *
 * Expects the mutation to be called with the target `userId`.
 */
export const useUnblockUser = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: number) => usersApi.unblock(userId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
};
