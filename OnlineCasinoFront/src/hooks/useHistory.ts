import { useQuery } from "@tanstack/react-query";
import { historyApi } from "../api/history";

/**
 * Fetches a paginated list of the user's game history.
 *
 * Each combination of `game`, `page` and `pageSize` is cached under its own
 * query key, so switching pages or filters reuses previously fetched data
 * instead of refetching.
 *
 * @param game     - Optional game filter (e.g. "blackjack"). Omit to fetch all games.
 * @param page     - 1-based page number. Defaults to 1.
 * @param pageSize - Number of items per page. Defaults to 20.
 */
export const useHistory = (game?: string, page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: ["history", game, page, pageSize],
    queryFn: () => historyApi.getAll(game, page, pageSize),
  });
};

/**
 * Fetches the details of a single round by its ID.
 *
 * The query is disabled until a non-empty `roundId` is provided, which allows
 * the hook to be called unconditionally (e.g. before navigation params resolve)
 * without triggering a request with an invalid ID.
 *
 * @param roundId - Unique round identifier. When empty, the query stays idle.
 */
export const useRound = (roundId: string) => {
  return useQuery({
    queryKey: ["history", "round", roundId],
    queryFn: () => historyApi.getByRoundId(roundId),
    enabled: !!roundId,
  });
};
