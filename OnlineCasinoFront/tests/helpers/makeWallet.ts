import type { Wallet } from "../../src/api/types";

export const makeWallet = (overrides: Partial<Wallet> = {}): Wallet => ({
  balance: 1000,
  currency: "EUR",
  ...overrides,
});
