import { useMutation, useQueryClient } from "@tanstack/react-query";

import { blackjackApi } from "../api/blackjack";
import type { StartBlackjackRequest } from "../api/types";

/**
 * Starts a new Blackjack session for the given table.
 *
 * Invalidates the wallet cache on success, because placing a bet
 * immediately affects the user's balance.
 *
 * @param gameCode - Identifier of the Blackjack table to start.
 * @returns A mutation for starting a Blackjack session.
 */
export const useStartBlackjack = (gameCode: string) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: StartBlackjackRequest) =>
      blackjackApi.start(gameCode, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["wallet"] });
    },
  });
};

/**
 * Requests another card for the current Blackjack session.
 *
 * Invalidates the wallet cache on success, because a hit may
 * trigger a bet settlement.
 *
 * @returns A mutation for hitting in a Blackjack session.
 */
export const useHit = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (sessionId: string) => blackjackApi.hit(sessionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["wallet"] }),
  });
};

/**
 * Ends the player's turn for the current Blackjack session.
 *
 * Invalidates the wallet cache on success, because standing
 * finalizes the hand and may result in a payout or loss.
 *
 * @returns A mutation for standing in a Blackjack session.
 */
export const useStand = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (sessionId: string) => blackjackApi.stand(sessionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["wallet"] }),
  });
};

/**
 * Doubles the player's bet for the current Blackjack session.
 *
 * Invalidates the wallet cache on success, because doubling
 * immediately deducts an additional bet amount.
 *
 * @returns A mutation for doubling down in a Blackjack session.
 */
export const useDouble = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (sessionId: string) => blackjackApi.double(sessionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["wallet"] }),
  });
};
