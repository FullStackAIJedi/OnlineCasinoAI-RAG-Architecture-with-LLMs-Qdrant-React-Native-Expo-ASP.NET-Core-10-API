import { useCallback, useEffect, useState } from "react";
import { useFocusEffect } from "expo-router";

import { useWallet } from "./useWallet";
import { usePlaceBet, useRouletteBets, useSpinRoulette } from "./useRoulette";
import type {
  CurrentBet,
  PlaceBetRequest,
  RouletteSpinResponse,
} from "../api/types";
import { uuid } from "../utils/uuid";

const GAME_CODE = "roulette";

export type RouletteBetType =
  | "red"
  | "black"
  | "even"
  | "odd"
  | "low"
  | "high"
  | "dozen"
  | "number";

export interface FormErrors {
  value?: string;
  amount?: string;
}

/**
 * Parses a roulette number input into a numeric value.
 * "00" is mapped to 37.
 *
 * @param input - The raw input string.
 * @returns The parsed number, or null if invalid.
 */
export function parseRouletteNumber(input: string): number | null {
  const trimmed = input.trim();
  if (trimmed === "00") return 37;
  const v = parseInt(trimmed, 10);
  return isNaN(v) ? null : v;
}

/**
 * Encapsulates all roulette screen logic: bet form state, validation,
 * optimistic balance, spin lifecycle, and cache synchronization.
 *
 * @returns State and handlers for the roulette screen.
 */
export function useRouletteScreen() {
  const [betType, setBetType] = useState<RouletteBetType>("red");
  const [value, setValue] = useState("");
  const [amount, setAmount] = useState("1");
  const [errors, setErrors] = useState<FormErrors>({});
  const [spinning, setSpinning] = useState(false);
  const [winningNumber, setWinningNumber] = useState<number | null>(null);
  const [pendingResult, setPendingResult] =
    useState<RouletteSpinResponse | null>(null);
  const [optimisticBalance, setOptimisticBalance] = useState<number | null>(
    null,
  );
  const [resultText, setResultText] = useState<string | null>(null);
  const [resultColor, setResultColor] = useState<string>("");

  const { data: wallet, refetch: refetchWallet } = useWallet();
  const { data: bets, refetch: refetchBets } = useRouletteBets(GAME_CODE);
  const placeBet = usePlaceBet(GAME_CODE);
  const spin = useSpinRoulette(GAME_CODE);

  const currency = wallet?.currency ?? "EUR";
  const shownBalance = optimisticBalance ?? wallet?.balance ?? 0;

  const syncWallet = useCallback(async () => {
    await refetchWallet();
    setOptimisticBalance(null);
  }, [refetchWallet]);

  useFocusEffect(
    useCallback(() => {
      setOptimisticBalance(null);
      refetchWallet();
      refetchBets();
    }, [refetchWallet, refetchBets]),
  );

  useEffect(() => {
    setOptimisticBalance(null);
  }, [wallet?.balance]);

  const validateForm = (): FormErrors => {
    const errs: FormErrors = {};

    if (!amount.trim()) {
      errs.amount = "Amount is required";
    } else {
      const amt = parseFloat(amount);
      if (isNaN(amt) || amt <= 0) errs.amount = "Invalid amount";
      else if (amt > shownBalance) errs.amount = "Insufficient balance";
    }

    if (betType === "number" || betType === "dozen") {
      if (!value.trim()) {
        errs.value = "Value is required";
      } else if (betType === "number") {
        const parsed = parseRouletteNumber(value);
        if (parsed === null) errs.value = "Invalid value (0-36 or 00)";
        else if (parsed < 0 || parsed > 37) errs.value = "Must be 0-36 or 00";
      } else if (betType === "dozen") {
        const v = parseInt(value, 10);
        if (isNaN(v)) errs.value = "Invalid value";
        else if (v < 1 || v > 3) errs.value = "Must be 1-3";
      }
    }

    return errs;
  };

  const handlePlaceBet = () => {
    const errs = validateForm();
    if (Object.keys(errs).length > 0) {
      setErrors(errs);
      return;
    }
    setErrors({});

    const betAmount = parseFloat(amount);
    setOptimisticBalance((prev) => (prev ?? shownBalance) - betAmount);

    const body: PlaceBetRequest = {
      type: betType,
      amount: betAmount,
      ...(betType === "number" && {
        value: parseRouletteNumber(value) ?? undefined,
      }),
      ...(betType === "dozen" && { value: parseInt(value, 10) }),
    };

    placeBet.mutate(body, {
      onSuccess: async () => {
        await refetchBets();
        setValue("");
      },
      onError: async (err) => {
        await syncWallet();
        setResultColor("red");
        setResultText(err.message);
      },
    });
  };

  const handleSpin = () => {
    if (!bets || bets.length === 0) {
      setResultColor("orange");
      setResultText("Place at least one bet before spinning");
      return;
    }

    setResultText(null);
    setResultColor("");
    setSpinning(true);
    setWinningNumber(null);
    setPendingResult(null);

    spin.mutate(
      { clientRoundId: uuid() },
      {
        onSuccess: (data) => {
          setWinningNumber(data.winningNumber);
          setPendingResult(data);
        },
        onError: async (err) => {
          setSpinning(false);
          setPendingResult(null);
          await syncWallet();
          setResultColor("red");
          setResultText(err.message);
        },
      },
    );
  };

  const handleSpinStop = async () => {
    setSpinning(false);
    const data = pendingResult;
    if (!data) return;

    await syncWallet();
    await refetchBets();

    if (data.totalWin > 0) {
      setResultColor("green");
      setResultText(`You won ${data.totalWin.toFixed(2)} ${currency}`);
    } else if (data.result === "Push") {
      setResultColor("orange");
      setResultText("Push, bet returned");
    } else {
      setResultColor("red");
      setResultText("You lost. Better luck next time");
    }

    setPendingResult(null);
  };

  return {
    betType,
    setBetType,
    value,
    setValue,
    amount,
    setAmount,
    errors,
    setErrors,
    spinning,
    winningNumber,
    bets: bets as CurrentBet[] | undefined,
    currency,
    shownBalance,
    resultText,
    resultColor,
    placeBet,
    handlePlaceBet,
    handleSpin,
    handleSpinStop,
  };
}
