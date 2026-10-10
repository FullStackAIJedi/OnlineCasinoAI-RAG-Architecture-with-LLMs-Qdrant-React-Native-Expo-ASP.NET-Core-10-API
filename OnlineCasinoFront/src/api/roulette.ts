// import { apiFetch } from "./client";
// import type {
//   CurrentBet,
//   PlaceBetRequest,
//   RouletteSpinRequest,
//   RouletteSpinResponse,
// } from "./types";

// /**
//  * Roulette game API client.
//  *
//  * Covers the full bet lifecycle on a table:
//  * - `getBets`   → read the currently placed bets.
//  * - `placeBet`  → add or update a bet before the spin.
//  * - `spin`      → resolve the round and return the outcome.
//  */
// export const rouletteApi = {

//   /**
//    * Fetches all bets currently placed on the given roulette table.
//    *
//    * Typically polled or refetched after `placeBet` so the UI reflects
//    * the shared state of the table.
//    *
//    * @param gameCode - Unique identifier of the roulette table.
//    * @returns The list of active bets on the table.
//    */
//   getBets: (gameCode: string) =>
//     apiFetch<CurrentBet[]>(`/api/games/roulette/${gameCode}/bets`),

//   /**
//    * Places or updates a bet on the roulette table.
//    *
//    * Calling this multiple times for the same bet type replaces the previous
//    * amount (server-side upsert semantics), rather than stacking bets.
//    *
//    * @param gameCode - Unique identifier of the roulette table.
//    * @param data     - Bet payload (type, amount, etc.).
//    * @returns The updated bet as stored on the server.
//    */
//   placeBet: (gameCode: string, data: PlaceBetRequest) =>
//     apiFetch<CurrentBet>(`/api/games/roulette/${gameCode}/bet`, {
//       method: "POST",
//       body: data,
//     }),

//     /**
//    * Spins the roulette wheel and resolves all bets on the table.
//    *
//    * This is a mutating, non-idempotent action — a successful call settles
//    * the round, updates the wallet, and records the round in history.
//    *
//    * @param gameCode - Unique identifier of the roulette table.
//    * @param data     - Spin payload (e.g. client round ID for idempotency).
//    * @returns The spin result, including the winning number and payouts.
//    */
//   spin: (gameCode: string, data: RouletteSpinRequest) =>
//     apiFetch<RouletteSpinResponse>(`/api/games/roulette/${gameCode}/spin`, {
//       method: "POST",
//       body: data,
//     }),
// };

import { apiFetch } from "./client";
import type {
  CurrentBet,
  PlaceBetRequest,
  RouletteSpinRequest,
  RouletteSpinResponse,
} from "./types";

/**
 * Roulette game API client.
 *
 * Covers the full bet lifecycle on a table:
 * - `getBets`   reads the currently placed bets.
 * - `placeBet`  adds or updates a bet before the spin.
 * - `spin`      resolves the round and returns the outcome.
 */
export const rouletteApi = {
  /**
   * Fetches all bets currently placed on the given roulette table.
   *
   * Typically polled or refetched after `placeBet` so the UI reflects
   * the shared state of the table.
   *
   * @param gameCode - Unique identifier of the roulette table.
   * @returns The list of active bets as `CurrentBet[]`.
   */
  getBets: (gameCode: string) =>
    apiFetch<CurrentBet[]>(`/api/games/roulette/${gameCode}/bets`),

  /**
   * Places or updates a bet on the roulette table.
   *
   * Calling this multiple times for the same bet type replaces the previous
   * amount (server-side upsert semantics), rather than stacking bets.
   *
   * @param gameCode - Unique identifier of the roulette table.
   * @param data - Bet payload as `PlaceBetRequest`.
   * @returns The updated bet as `CurrentBet`.
   */
  placeBet: (gameCode: string, data: PlaceBetRequest) =>
    apiFetch<CurrentBet>(`/api/games/roulette/${gameCode}/bet`, {
      method: "POST",
      body: data,
    }),

  /**
   * Spins the roulette wheel and resolves all bets on the table.
   *
   * This is a mutating, non-idempotent action. A successful call settles
   * the round, updates the wallet, and records the round in history.
   *
   * @param gameCode - Unique identifier of the roulette table.
   * @param data - Spin payload as `RouletteSpinRequest`.
   * @returns The spin result as `RouletteSpinResponse`.
   */
  spin: (gameCode: string, data: RouletteSpinRequest) =>
    apiFetch<RouletteSpinResponse>(`/api/games/roulette/${gameCode}/spin`, {
      method: "POST",
      body: data,
    }),
};