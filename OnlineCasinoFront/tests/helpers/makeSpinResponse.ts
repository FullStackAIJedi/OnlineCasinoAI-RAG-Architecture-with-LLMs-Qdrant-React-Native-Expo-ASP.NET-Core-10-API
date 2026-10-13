import type { SpinResponse } from "../../src/api/types";

export const makeSpinResponse = (
  overrides: Partial<SpinResponse> = {},
): SpinResponse => ({
  roundId: "round-1",
  reels: ["cherry", "lemon", "bell"],
  betAmount: 10,
  winAmount: 0,
  balance: 990,
  result: "loss",
  matchCount: 0,
  ...overrides,
});
