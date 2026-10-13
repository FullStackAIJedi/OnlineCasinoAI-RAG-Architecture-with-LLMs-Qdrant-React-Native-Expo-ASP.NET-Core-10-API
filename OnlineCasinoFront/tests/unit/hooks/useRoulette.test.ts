import { QueryClient } from "@tanstack/react-query";
import { act, waitFor } from "@testing-library/react-native";

import { rouletteApi } from "../../../src/api/roulette";
import {
  rouletteKeys,
  usePlaceBet,
  useRouletteBets,
  useSpinRoulette,
} from "../../../src/hooks/useRoulette";
import { createWrapper, makeCurrentBet, renderHookAsync } from "../../helpers";

jest.mock("../../../src/api/roulette");

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("rouletteKeys", () => {
  it("all", () => {
    expect(rouletteKeys.all).toEqual(["roulette"]);
  });

  it("bets", () => {
    expect(rouletteKeys.bets("r1")).toEqual(["roulette", "bets", "r1"]);
  });
});

describe("useRouletteBets", () => {
  it("fetches when gameCode set", async () => {
    (rouletteApi.getBets as jest.Mock).mockResolvedValue([makeCurrentBet()]);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useRouletteBets("r1"), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(rouletteApi.getBets).toHaveBeenCalledWith("r1");
  });

  it("disabled when empty", async () => {
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useRouletteBets(""), {
      wrapper: createWrapper(qc),
    });
    expect(result.current.fetchStatus).toBe("idle");
  });
});

describe("usePlaceBet", () => {
  it("calls placeBet and invalidates bets", async () => {
    (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => usePlaceBet("r1"), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({ type: "red", amount: 10 });
    });

    expect(rouletteApi.placeBet).toHaveBeenCalledWith("r1", {
      type: "red",
      amount: 10,
    });
    expect(spy).toHaveBeenCalledWith({ queryKey: rouletteKeys.bets("r1") });
  });
});

describe("useSpinRoulette", () => {
  it("invalidates roulette + wallet + history", async () => {
    (rouletteApi.spin as jest.Mock).mockResolvedValue({});
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useSpinRoulette("r1"), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({ clientRoundId: "u" });
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: rouletteKeys.all });
    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
    expect(spy).toHaveBeenCalledWith({ queryKey: ["history"] });
  });
});
