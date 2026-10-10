import { apiFetch } from "./client";
import type { GameRound, PagedResult } from "./types";

/**
 * Game history API client.
 *
 * Exposes paginated access to past rounds, with optional filtering by game,
 * plus lookup of a single round by its ID.
 */
export const historyApi = {

  /**
   * Fetches a paginated list of the user's game history.
   *
   * Query parameters are serialized via `URLSearchParams`, so values are
   * automatically URL-encoded.
   *
   * @param game     - Optional game filter (e.g. `"blackjack"`). Omit to fetch all games.
   * @param page     - 1-based page number. Defaults to 1.
   * @param pageSize - Number of items per page. Defaults to 20.
   * @returns A paginated result containing the rounds and pagination metadata.
   */
  getAll: (game?: string, page = 1, pageSize = 20) => {
    const params = new URLSearchParams();
    if (game) params.set("game", game);
    params.set("page", String(page));
    params.set("pageSize", String(pageSize));
    return apiFetch<PagedResult<GameRound>>(`/api/game-history?${params}`);
  },

  /**
   * Fetches a single round by its ID.
   *
   * @param roundId - Unique identifier of the round.
   * @returns The round details, or throws `ApiError` (404) if not found.
   */
  getByRoundId: (roundId: string) =>
    apiFetch<GameRound>(`/api/game-history/${roundId}`),
};
