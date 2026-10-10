import { apiFetch } from "./client";
import type { BlackjackState, StartBlackjackRequest } from "./types";

/**
 * Blackjack game API client.
 *
 * A session is created via `start()` and then mutated through the action
 * endpoints (`hit`, `stand`, `double`). Every mutating call returns the
 * full updated `BlackjackState`, so the client never needs to re-fetch.
 */
export const blackjackApi = {
  /**
   * Starts a new Blackjack session at the given table.
   *
   * Places the initial bet and deals the opening hand.
   *
   * @param gameCode - Unique identifier of the Blackjack table.
   * @param data - Start payload as `StartBlackjackRequest`.
   * @returns The initial game state as `BlackjackState`.
   */
  start: (gameCode: string, data: StartBlackjackRequest) =>
    apiFetch<BlackjackState>(`/api/games/blackjack/${gameCode}/start`, {
      method: "POST",
      body: data,
    }),

  /**
   * Requests another card ("hit") for the active session.
   *
   * @param sessionId - Unique identifier of the current Blackjack session.
   * @returns The updated game state as `BlackjackState`.
   */
  hit: (sessionId: string) =>
    apiFetch<BlackjackState>(`/api/games/blackjack/sessions/${sessionId}/hit`, {
      method: "POST",
    }),

  /**
   * Ends the player's turn ("stand") for the active session.
   *
   * The dealer then plays out their hand and the round is settled.
   *
   * @param sessionId - Unique identifier of the current Blackjack session.
   * @returns The final game state as `BlackjackState`.
   */
  stand: (sessionId: string) =>
    apiFetch<BlackjackState>(
      `/api/games/blackjack/sessions/${sessionId}/stand`,
      {
        method: "POST",
      },
    ),

  /**
   * Doubles the player's bet ("double down") and deals exactly one more card.
   *
   * Only valid as the first action after the initial deal.
   *
   * @param sessionId - Unique identifier of the current Blackjack session.
   * @returns The updated game state as `BlackjackState`.
   */
  double: (sessionId: string) =>
    apiFetch<BlackjackState>(
      `/api/games/blackjack/sessions/${sessionId}/double`,
      {
        method: "POST",
      },
    ),

  /**
   * Fetches the current state of an existing Blackjack session.
   *
   * Useful for rehydrating the UI after navigation, reconnect, or app resume.
   *
   * @param sessionId - Unique identifier of the Blackjack session.
   * @returns The current game state as `BlackjackState`.
   */
  getState: (sessionId: string) =>
    apiFetch<BlackjackState>(`/api/games/blackjack/sessions/${sessionId}`),
};
