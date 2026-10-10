// import { apiFetch } from "./client";
// import type { SpinRequest, SpinResponse } from "./types";

// /**
//  * Slots game API client.
//  *
//  * Minimal by design: a single spin endpoint that resolves the round,
//  * updates the wallet, and records the round in history server-side.
//  */
// export const slotsApi = {

//   /**
//    * Spins the slot machine and resolves the round.
//    *
//    * Mutating, non-idempotent action with broad side effects:
//    * - deducts the bet from the wallet,
//    * - credits any winnings,
//    * - records the round in the user's game history.
//    *
//    * The request should include a unique client round ID (see `uuid()`),
//    * which the server uses as an idempotency key so that a retried spin
//    * does not settle the round twice.
//    *
//    * @param gameCode - Unique identifier of the slot game.
//    * @param data     - Spin payload (bet amount, lines, client round ID, etc.).
//    * @returns The spin result, including reels, payouts, and updated balance.
//    */
//   spin: (gameCode: string, data: SpinRequest) =>
//     apiFetch<SpinResponse>(`/api/games/slots/${gameCode}/spin`, {
//       method: "POST",
//       body: data,
//     }),
// };

import { apiFetch } from "./client";
import type { SpinRequest, SpinResponse } from "./types";

/**
 * Slots game API client.
 *
 * Minimal by design: a single spin endpoint that resolves the round,
 * updates the wallet, and records the round in history server-side.
 */
export const slotsApi = {
  /**
   * Spins the slot machine and resolves the round.
   *
   * Mutating, non-idempotent action with broad side effects:
   * - deducts the bet from the wallet,
   * - credits any winnings,
   * - records the round in the user's game history.
   *
   * The request should include a unique client round ID (see `uuid()`),
   * which the server uses as an idempotency key so that a retried spin
   * does not settle the round twice.
   *
   * @param gameCode - Unique identifier of the slot game.
   * @param data - Spin payload as `SpinRequest`.
   * @returns The spin result as `SpinResponse`.
   */
  spin: (gameCode: string, data: SpinRequest) =>
    apiFetch<SpinResponse>(`/api/games/slots/${gameCode}/spin`, {
      method: "POST",
      body: data,
    }),
};
