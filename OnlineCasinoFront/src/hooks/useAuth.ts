import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import type {
  ChangePasswordRequest,
  SignInRequest,
  SignUpRequest,
} from "../api/types";
import { ApiError } from "../api/errors";

import { authApi } from "../api/auth";
import { clearCookie, hasCookie } from "../api/client";
import { AUTH_QUERY_KEYS } from "../constants/authKeys";

/**
 * Authenticates the user and invalidates auth-related queries on success.
 *
 * @returns A mutation for signing in.
 */
export const useLogin = () => {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (data: SignInRequest) => authApi.login(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: AUTH_QUERY_KEYS.hasCookie });
      qc.invalidateQueries({ queryKey: ["wallet"] });
    },
  });
};

/**
 * Registers a new user and invalidates auth-related queries on success.
 *
 * @returns A mutation for signing up.
 */
export const useRegister = () => {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (data: SignUpRequest) => authApi.register(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: AUTH_QUERY_KEYS.hasCookie });
      qc.invalidateQueries({ queryKey: ["wallet"] });
    },
  });
};

/**
 * Logs the user out and clears all non-auth queries from the cache.
 *
 * @returns A mutation for signing out.
 */
export const useLogout = () => {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      try {
        await authApi.logout();
      } catch {}
      await clearCookie();
    },
    onSuccess: () => {
      qc.removeQueries({ predicate: (q) => q.queryKey[0] !== "auth" });
      qc.invalidateQueries({ queryKey: AUTH_QUERY_KEYS.hasCookie });
    },
  });
};

/**
 * Changes the current user's password.
 *
 * @returns A mutation for changing the password.
 */
export const useChangePassword = () => {
  return useMutation({
    mutationFn: (data: ChangePasswordRequest) => authApi.changePassword(data),
  });
};

/**
 * Checks whether the user has a valid session.
 *
 * Verifies both the local cookie presence and the server-side session.
 * Re-evaluates on mount, focus, and reconnect.
 *
 * @returns A query whose data is `true` if the user is authenticated.
 */
export const useIsAuthenticated = () => {
  return useQuery({
    queryKey: AUTH_QUERY_KEYS.hasCookie,
    queryFn: async (): Promise<boolean> => {
      const cookieExists = await hasCookie();
      if (!cookieExists) return false;

      try {
        await authApi.me();
        return true;
      } catch (e) {
        if (e instanceof ApiError && e.status === 401) return false;
        return cookieExists;
      }
    },
    staleTime: 0,
    gcTime: 0,
    retry: false,
    refetchOnMount: "always",
    refetchOnReconnect: "always",
    refetchOnWindowFocus: "always",
    refetchInterval: 5 * 60 * 1000,
    refetchIntervalInBackground: false,
  });
};
