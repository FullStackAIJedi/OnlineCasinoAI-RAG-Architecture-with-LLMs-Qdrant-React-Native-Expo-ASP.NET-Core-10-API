import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { rouletteApi } from "../api/roulette";
import type {
  CurrentBet,
  PlaceBetRequest,
  RouletteSpinRequest,
  RouletteSpinResponse,
} from "../api/types";

/**
 * Query key factory for all roulette-related queries.
 *
 * Centralizes the key structure so invalidation stays consistent.
 */
export const rouletteKeys = {
  all: ["roulette"] as const,
  bets: (gameCode: string) => [...rouletteKeys.all, "bets", gameCode] as const,
};

/**
 * Fetches the current bets placed on the given roulette table.
 *
 * The query is disabled until a non-empty `gameCode` is provided.
 *
 * @param gameCode - Unique identifier of the roulette table.
 * @returns A query whose data is `CurrentBet[]`.
 */
export const useRouletteBets = (gameCode: string) => {
  return useQuery<CurrentBet[]>({
    queryKey: rouletteKeys.bets(gameCode),
    queryFn: () => rouletteApi.getBets(gameCode),
    enabled: !!gameCode,
  });
};

/**
 * Places a bet on the current roulette table.
 *
 * On success, invalidates the bets cache for this table.
 *
 * @param gameCode - Unique identifier of the roulette table.
 * @returns A mutation for placing a bet.
 */
export const usePlaceBet = (gameCode: string) => {
  const qc = useQueryClient();
  return useMutation<CurrentBet, Error, PlaceBetRequest>({
    mutationFn: (data) => rouletteApi.placeBet(gameCode, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: rouletteKeys.bets(gameCode) });
    },
  });
};

/**
 * Spins the roulette wheel and resolves all bets on the table.
 *
 * On success, invalidates roulette, wallet, and history caches.
 *
 * @param gameCode - Unique identifier of the roulette table.
 * @returns A mutation for spinning the wheel.
 */
export const useSpinRoulette = (gameCode: string) => {
  const qc = useQueryClient();
  return useMutation<RouletteSpinResponse, Error, RouletteSpinRequest>({
    mutationFn: (data) => rouletteApi.spin(gameCode, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: rouletteKeys.all });
      qc.invalidateQueries({ queryKey: ["wallet"] });
      qc.invalidateQueries({ queryKey: ["history"] });
    },
  });
};
