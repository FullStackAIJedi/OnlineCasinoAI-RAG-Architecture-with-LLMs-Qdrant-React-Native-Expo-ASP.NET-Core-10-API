import { useMutation, useQueryClient } from "@tanstack/react-query";
import { slotsApi } from "../api/slots";

import type { SpinRequest } from "../api/types";

/**
 * Spins the slot machine for the given game.
 *
 * On success, invalidates the following caches:
 * - `wallet`  → the bet is deducted and any winnings are credited.
 * - `history` → the spin is recorded as a new round in the user's history.
 *
 * @param gameCode - Unique identifier of the slot game.
 * @param data     - Spin payload (bet amount, lines, etc.).
 */
export const useSpinSlots = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ gameCode, data }: { gameCode: string; data: SpinRequest }) =>
      slotsApi.spin(gameCode, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["wallet"] });
      qc.invalidateQueries({ queryKey: ["history"] });
    },
  });
};
