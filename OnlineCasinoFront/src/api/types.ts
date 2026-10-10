/**
 * Standard error envelope returned by the backend for non-2xx responses.
 * 
 * (ASP.NET Core, custom) populate different fields.
 * `apiFetch` normalizes all of them into a single message.
 */
export interface ErrorResponse {
  /** HTTP status code (some backends use `statusCode`). */
  statusCode?: number;
 /** HTTP status code (alternative field name). */
  status?: number;
 /** Human-readable error message. */
  message?: string;
  /** Short error title (from ASP.NET ProblemDetails). */
  title?: string;
  /** Field-level validation errors: field name → list of messages. */
  errors?: Record<string, string[]>;
}

// Auth

/** Payload for creating a new user account. */
export interface SignUpRequest {
  email: string;
  nickName: string;
  password: string;
}

/** Credentials for signing in. */
export interface SignInRequest {
  email: string;
  password: string;
}

/** Payload for changing the current user's password. */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

/** Identity of the currently authenticated user (session-level). */
export interface AuthUser {
  id: number;
  email: string;
  nickname: string;
  role: string;
}

// Users

/** Full profile of the current user. */
export interface UserProfile {
  id: number;
  email: string; 
  /** URL or key of the profile image. */
  pofileImage: string; 
  nickName: string;
  firstName?: string;
  lastName?: string;
  role: string;
  createdAt: string;
}

/** Payload for updating the current user's profile. */
export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  /** URL or key of the profile image. */
  pofileImage: string;
}

/** Compact user representation used in admin lists. */
export interface UserListItem {
  id: number;
  email: string;
  nickName: string;
  pofileImage: string;
  role: string;
  isBlocked: boolean;
  balance: number;
}

// Wallet

/** Current user's wallet. */
export interface Wallet {
  balance: number;
  currency: string;
}

/** A single wallet transaction. */
export interface Transaction {
  id: number;
  /** Transaction type (`"deposit"`, `"withdraw"`, `"bet"`, `"win"`). */
  type: string;
  amount: number;
  /** Wallet balance after this transaction was applied. */
  balanceAfter: number;
  createdAt: string;
}

/**
 * Standard paginated response envelope.
 *
 * @typeParam T - Type of a single item in the page.
 */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
}

// Games

/** Metadata for a game in the catalog. */
export interface Game {
  id: number;
  /** Unique, stable game code (e.g. `"blackjack"`, `"roulette"`). */
  code: string;
  name: string;
  /** Game category/type (e.g. `"table"`, `"slots"`). */
  type: string;
  enabled: boolean;
  minBet: number;
  maxBet: number;
}

// Slots

/** Payload for spinning the slot machine. */
export interface SpinRequest {
  betAmount: number;
  /**
   * Client-generated unique round ID (see `uuid()`).
   * Used server-side as an idempotency key to prevent duplicate spins on retry.
   */
  clientRoundId: string;
}

/** Result of a single slot spin. */
export interface SpinResponse {
  roundId: string;
  /** The three (4 or more) symbols on the reels after the spin. */
  reels: string[];
  betAmount: number;
  winAmount: number;
  /** Wallet balance after the spin was settled. */
  balance: number;
  /** Outcome summary (e.g. `"win"`, `"loss"`, `"jackpot"`). */
  result: string;
  /** Number of matching symbols (for win evaluation). */
  matchCount: number;
}

// Roulette

/** Payload for placing a bet on a roulette table. */
export interface PlaceBetRequest {
  /** Bet type (e.g. `"straight"`, `"red"`, `"dozen"`). */
  type: string;
  /** Specific value for the bet type (e.g. number for `"straight"`). */
  value?: number;
  amount: number;
}

/** A bet currently placed on the roulette table. */
export interface CurrentBet {
  id: number;
  type: string;
  value?: number;
  amount: number;
}

/** Payload for spinning the roulette wheel. */
export interface RouletteSpinRequest {
  /** Client-generated unique round ID, used as an idempotency key. */
  clientRoundId: string;
}

/** Result of a roulette spin. */
export interface RouletteSpinResponse {
  roundId: string;
  /** The winning number (0–36). */
  winningNumber: number;
   /** The winning color (`"red"`, `"black"`, or `"green"` for 0). */
  winningColor: string;
  /** Total amount bet across all bets in the round. */
  totalBet: number;
  /** Total amount won across all bets in the round. */
  totalWin: number;
  /** Wallet balance after the spin was settled. */
  balance: number;
  /** Outcome summary (e.g. `"win"`, `"loss"`). */
  result: string;
}



// Blackjack

/** Payload for starting a new Blackjack session. */
export interface StartBlackjackRequest {
  betAmount: number;
}

/** Current state of a Blackjack session. */
export interface BlackjackState {
  sessionId: string; 
  /** Cards in the player's hand (e.g. `["AS", "10H"]`). */
  playerHand: string[];
  /** Cards in the dealer's hand. The second card may be hidden while in progress. */
  dealerHand: string[];
  /** Total score of the player's hand. */
  playerScore: number;
  /** Total score of the dealer's hand. */
  dealerScore: number;
  /** Session status (e.g. `"in_progress"`, `"player_bust"`, `"finished"`). */
  status: string;
  betAmount: number;
  winAmount: number;
  /** Outcome summary, set only once the round is finished. */
  result?: string;
  /** Wallet balance after the current action. */
  balance: number;
}

// History

/** A single past game round in the user's history. */
export interface GameRound {
  roundId: string;
  /** Game code the round belongs to (e.g. `"slots"`, `"roulette"`). */
  game: string;
  betAmount: number;
  winAmount: number;
  /** Outcome summary (e.g. `"win"`, `"loss"`). Optional for non-settled rounds. */
  result?: string;
  createdAt: string;
}
