import { useCallback, useRef, useState } from "react";
import { useFocusEffect } from "expo-router";

import { useWallet } from "./useWallet";
import { useSpinSlots } from "./useSlots";
import type { SpinResponse } from "../api/types";
import { uuid } from "../utils/uuid";

const DEFAULT_BET = "1";
const DEFAULT_CURRENCY = "EUR";
const MIN_SPIN_DURATION_MS = 2000;

const REELS_COUNT: Record<string, number> = {
  slots: 3,
  slots4: 4,
};
const DEFAULT_REELS_COUNT = 3;
const DEFAULT_GAME_CODE = "slots";

export interface SlotsScreenOptions {
  gameCode?: string;
}

/**
 * Encapsulates all slots screen logic: bet form, spin lifecycle,
 * optimistic balance, reel timing, and result display.
 *
 * @param options - Screen options.
 * @param options.gameCode - Slot game code. Defaults to "slots".
 * @returns State and handlers for the slots screen.
 */
export function useSlotsScreen({
  gameCode = DEFAULT_GAME_CODE,
}: SlotsScreenOptions = {}) {
  const reelsCount = REELS_COUNT[gameCode] ?? DEFAULT_REELS_COUNT;

  const [bet, setBet] = useState(DEFAULT_BET);
  const [error, setError] = useState<string | undefined>();
  const [spinning, setSpinning] = useState(false);
  const [settling, setSettling] = useState(false);
  const [finalSymbols, setFinalSymbols] = useState<string[] | null>(null);
  const [matchCount, setMatchCount] = useState(0);
  const [lastWin, setLastWin] = useState(0);
  const [pendingResult, setPendingResult] = useState<SpinResponse | null>(null);
  const [optimisticBalance, setOptimisticBalance] = useState<number | null>(
    null,
  );

  const spinTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const spin = useSpinSlots();
  const { data: wallet, refetch: refetchWallet } = useWallet();

  const currency = wallet?.currency ?? DEFAULT_CURRENCY;
  const shownBalance = optimisticBalance ?? wallet?.balance ?? 0;

  const clearSpinTimer = useCallback(() => {
    if (spinTimerRef.current) {
      clearTimeout(spinTimerRef.current);
      spinTimerRef.current = null;
    }
  }, []);

  const syncWallet = useCallback(async () => {
    await refetchWallet();
    setOptimisticBalance(null);
  }, [refetchWallet]);

  useFocusEffect(
    useCallback(() => {
      setOptimisticBalance(null);
      refetchWallet();
      return clearSpinTimer;
    }, [refetchWallet, clearSpinTimer]),
  );

  const validateBet = useCallback((): number | null => {
    const betAmount = parseFloat(bet);

    if (!bet.trim()) {
      setError("Bet is required");
      return null;
    }
    if (isNaN(betAmount) || betAmount <= 0) {
      setError("Invalid bet");
      return null;
    }
    if (betAmount > shownBalance) {
      setError("Insufficient balance");
      return null;
    }

    return betAmount;
  }, [bet, shownBalance]);

  const finishSpin = useCallback((data: SpinResponse) => {
    setPendingResult(data);
    setFinalSymbols(data.reels);
    setSpinning(false);
  }, []);

  const handleSpin = useCallback(() => {
    const betAmount = validateBet();
    if (betAmount === null) return;

    clearSpinTimer();

    setError(undefined);
    setFinalSymbols(null);
    setMatchCount(0);
    setLastWin(0);
    setPendingResult(null);
    setSpinning(true);
    setSettling(true);

    setOptimisticBalance((prev) => (prev ?? shownBalance) - betAmount);

    const startTime = Date.now();

    spin.mutate(
      {
        gameCode,
        data: { betAmount, clientRoundId: uuid() },
      },
      {
        onSuccess: (data) => {
          const elapsed = Date.now() - startTime;
          const remaining = Math.max(0, MIN_SPIN_DURATION_MS - elapsed);

          if (remaining > 0) {
            spinTimerRef.current = setTimeout(
              () => finishSpin(data),
              remaining,
            );
          } else {
            finishSpin(data);
          }
        },
        onError: async (err) => {
          setSpinning(false);
          setSettling(false);
          setPendingResult(null);
          setError(err.message);
          await syncWallet();
        },
      },
    );
  }, [
    validateBet,
    clearSpinTimer,
    spin,
    gameCode,
    shownBalance,
    finishSpin,
    syncWallet,
  ]);

  const handleReelsStop = useCallback(async () => {
    const data = pendingResult;
    if (!data) return;

    setMatchCount(data.matchCount);
    setLastWin(data.winAmount);

    await syncWallet();

    setPendingResult(null);
    setSettling(false);
  }, [pendingResult, syncWallet]);

  const handleBetChange = useCallback(
    (t: string) => {
      setBet(t);
      if (error) setError(undefined);
    },
    [error],
  );

  return {
    bet,
    handleBetChange,
    error,
    spinning,
    settling,
    finalSymbols,
    matchCount,
    lastWin,
    reelsCount,
    currency,
    shownBalance,
    handleSpin,
    handleReelsStop,
  };
}
