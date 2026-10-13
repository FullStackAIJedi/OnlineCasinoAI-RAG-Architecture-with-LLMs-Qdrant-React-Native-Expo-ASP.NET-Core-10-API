import type { Game } from "../../src/api/types";

export const makeGame = (overrides: Partial<Game> = {}): Game => ({
  id: 1,
  code: "blackjack",
  name: "Blackjack",
  type: "table",
  enabled: true,
  minBet: 5,
  maxBet: 500,
  ...overrides,
});
