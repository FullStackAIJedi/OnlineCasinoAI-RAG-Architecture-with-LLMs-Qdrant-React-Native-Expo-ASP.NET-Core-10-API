import { useQuery } from "@tanstack/react-query";
import { gamesApi } from "../api/games";

/**
 * Fetches the full list of available games.
 *
 * Cached for 10 minutes (`staleTime`) because the catalog rarely changes
 * and refetching on every mount would be wasteful.
 */
export const useGames = () => {
  return useQuery({
    queryKey: ["games"],
    queryFn: gamesApi.getAll,
    staleTime: 10 * 60 * 1000,
  });
};

/**
 * Fetches a single game by its code.
 *
 * The query is disabled until a non-empty `code` is provided, which allows
 * the hook to be called unconditionally at the top level of a component
 * (e.g. before route params are resolved).
 *
 * @param code - Unique game code. When empty, the query stays idle.
 */
export const useGame = (code: string) => {
  return useQuery({
    queryKey: ["games", code],
    queryFn: () => gamesApi.getByCode(code),
    enabled: !!code,
  });
};
