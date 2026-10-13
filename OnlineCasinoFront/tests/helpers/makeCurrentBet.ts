import type { CurrentBet } from "../../src/api/types";

export const makeCurrentBet = (
  overrides: Partial<CurrentBet> = {},
): CurrentBet => ({
  id: 1,
  type: "red",
  amount: 10,
  ...overrides,
});
