import type { GameRound } from "../../src/api/types";

export const makeGameRound = (
  overrides: Partial<GameRound> = {},
): GameRound => ({
  roundId: "round-1",
  game: "slots",
  betAmount: 10,
  winAmount: 0,
  result: "Loss",
  createdAt: new Date().toISOString(),
  ...overrides,
});
