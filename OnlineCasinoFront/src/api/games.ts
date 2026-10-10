import { apiFetch } from "./client";
import type { Game } from "./types";

/**
 * Games catalog API client.
 *
 * Provides read-only access to the list of available games and to
 * individual game metadata. No caching or auth logic lives here —
 * that is handled by the React Query hooks and `apiFetch`.
 */
export const gamesApi = {
  /**
   * Fetches the full catalog of available games.
   *
   * @returns An array of all games supported by the platform.
   */
  getAll: () => apiFetch<Game[]>("/api/games"),

  /**
   * Fetches a single game by its unique code.
   *
   * @param code - Unique game code (e.g. `"blackjack"`, `"roulette"`).
   * @returns The game metadata, or throws `ApiError` (404) if not found.
   */
  getByCode: (code: string) => apiFetch<Game>(`/api/games/${code}`),
};
