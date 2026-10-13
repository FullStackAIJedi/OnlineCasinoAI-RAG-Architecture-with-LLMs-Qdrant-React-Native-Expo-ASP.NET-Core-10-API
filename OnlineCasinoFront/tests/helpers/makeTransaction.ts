import type { Transaction } from "../../src/api/types";

export const makeTransaction = (
  overrides: Partial<Transaction> = {},
): Transaction => ({
  id: 1,
  type: "deposit",
  amount: 100,
  balanceAfter: 1100,
  createdAt: new Date().toISOString(),
  ...overrides,
});
