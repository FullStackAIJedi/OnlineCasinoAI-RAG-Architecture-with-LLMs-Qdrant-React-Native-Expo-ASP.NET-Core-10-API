import type { BlackjackState } from "../../src/api/types";

export const makeBlackjackState = (
  overrides: Partial<BlackjackState> = {},
): BlackjackState => ({
  sessionId: "session-1",
  playerHand: ["AS", "10H"],
  dealerHand: ["5S", "??"],
  playerScore: 21,
  dealerScore: 5,
  status: "PlayerTurn",
  betAmount: 10,
  winAmount: 0,
  balance: 990,
  ...overrides,
});
