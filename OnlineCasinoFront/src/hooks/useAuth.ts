import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import type { ChangePasswordRequest, SignInRequest, SignUpRequest } from "../api/types";
import { ApiError } from "../api/errors";

import { authApi } from "../api/auth";
import { clearCookie, hasCookie } from "../api/client";

export const useLogin = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: SignInRequest) => authApi.login(data),
    onSuccess: async () => {
      await qc.refetchQueries({ queryKey: ["auth", "hasCookie"] });
      qc.invalidateQueries({ queryKey: ["wallet"] });
    },
  });
};

export const useRegister = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: SignUpRequest) => authApi.register(data),
    onSuccess: async () => {
      await qc.refetchQueries({ queryKey: ["auth", "hasCookie"] });
      qc.invalidateQueries({ queryKey: ["wallet"] });
    },
  });
};

export const useLogout = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      try {
        await authApi.logout();
      } catch {
      }
      await clearCookie();
    },
    onSuccess: async () => {
      qc.clear();
      await qc.refetchQueries({ queryKey: ["auth", "hasCookie"] });
    },
  });
};


export const useChangePassword = () => {
  return useMutation({
    mutationFn: (data: ChangePasswordRequest) => authApi.changePassword(data),
  });
};

export const useMe = () => {
  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: authApi.me,
    retry: false,
    staleTime: 0,
    gcTime: 0,
  });
};

export const useIsAuthenticated = () => {
  return useQuery({
    queryKey: ["auth", "hasCookie"],
    queryFn: async (): Promise<boolean> => {
      const cookieExists = await hasCookie();
      if (!cookieExists) return false;

      try {
        await authApi.me();
        return true;
      } catch (e) {
        if (e instanceof ApiError && e.status === 401) {
          return false;
        }
        return cookieExists;
      }
    },
    staleTime: 0,
    retry: false, 
  });
};
