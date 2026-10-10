import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { walletApi } from "../api/wallet";

/**
 * Fetches the current user's wallet (balance and currency).
 *
 * Always fresh: the balance is critical for gameplay, so the cache is
 * considered stale immediately and refetched on every mount and window focus.
 *
 * Note: `gcTime: 0` is intentionally NOT set — it would destroy the cache
 * entry between unmounts and cause a visible empty state on remount.
 */
export const useWallet = () => {
  return useQuery({
    queryKey: ["wallet"],
    queryFn: walletApi.get,
    staleTime: 0,
    refetchOnMount: "always",
    refetchOnWindowFocus: true,
  });
};

/**
 * Fetches a paginated list of the user's wallet transactions.
 *
 * Each page is cached under its own query key, so navigating back to a
 * previously viewed page serves the cached data before revalidating.
 *
 * @param page     - 1-based page number. Defaults to 1.
 * @param pageSize - Number of transactions per page. Defaults to 20.
 */
export const useTransactions = (page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: ["wallet", "transactions", page, pageSize],
    queryFn: () => walletApi.getTransactions(page, pageSize),
    staleTime: 0,
  });
};

/**
 * Deposits funds into the user's wallet.
 *
 * On success, awaits a refetch of the wallet query so the UI is guaranteed
 * to display the updated balance before the mutation resolves.
 *
 * Expects the mutation to be called with the deposit payload (amount, method, etc.).
 */
export const useDeposit = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: walletApi.deposit,
    onSuccess: async () => {
      // refetchQueries – изчаква завършването
      await qc.refetchQueries({ queryKey: ["wallet"] });
    },
  });
};

/**
 * Withdraws funds from the user's wallet.
 *
 * On success, awaits a refetch of the wallet query so the UI is guaranteed
 * to display the updated balance before the mutation resolves.
 *
 * Expects the mutation to be called with the withdrawal payload (amount, method, etc.).
 */
export const useWithdraw = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: walletApi.withdraw,
    onSuccess: async () => {
      await qc.refetchQueries({ queryKey: ["wallet"] });
    },
  });
};
