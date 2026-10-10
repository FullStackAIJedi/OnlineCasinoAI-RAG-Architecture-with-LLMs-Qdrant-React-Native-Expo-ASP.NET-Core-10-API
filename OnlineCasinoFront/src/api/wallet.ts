import { apiFetch } from "./client";
import type { PagedResult, Transaction, Wallet } from "./types";

/**
 * Wallet API client.
 *
 * Covers reading the current balance, browsing transaction history,
 * and mutating the balance via deposits and withdrawals.
 *
 * Note: bet payouts and winnings are settled server-side by the game
 * endpoints (slots, roulette, blackjack) — they are not handled here.
 */
export const walletApi = {
  /**
   * Fetches the current user's wallet (balance and currency).
   *
   * @returns The current wallet state.
   */
  get: () => apiFetch<Wallet>("/api/wallet"),

  /**
   * Fetches a paginated list of the user's wallet transactions.
   *
   * Transactions include deposits, withdrawals, bets, and winnings —
   * ordered by the server (typically newest first).
   *
   * @param page     - 1-based page number. Defaults to 1.
   * @param pageSize - Number of transactions per page. Defaults to 20.
   * @returns A paginated result of transactions.
   */
  getTransactions: (page = 1, pageSize = 20) =>
    apiFetch<PagedResult<Transaction>>(
      `/api/wallet/transactions?page=${page}&pageSize=${pageSize}`,
    ),

  /**
   * Deposits funds into the user's wallet.
   *
   * @param amount - Amount to deposit (in the wallet's currency).
   * @returns The updated wallet after the deposit.
   */
  deposit: (amount: number) =>
    apiFetch<Wallet>("/api/wallet/deposit", {
      method: "POST",
      body: { amount },
    }),

  /**
   * Withdraws funds from the user's wallet.
   *
   * @param amount - Amount to withdraw (in the wallet's currency).
   * @returns The updated wallet after the withdrawal.
   * @throws {ApiError} If the balance is insufficient (typically 400).
   */
  withdraw: (amount: number) =>
    apiFetch<Wallet>("/api/wallet/withdraw", {
      method: "POST",
      body: { amount },
    }),
};
