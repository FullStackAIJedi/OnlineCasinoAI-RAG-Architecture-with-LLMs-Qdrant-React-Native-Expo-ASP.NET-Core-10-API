import { QueryClient } from "@tanstack/react-query";
import { act } from "@testing-library/react-native";

import { blackjackApi } from "../../../src/api/blackjack";
import {
  useDouble,
  useHit,
  useStand,
  useStartBlackjack,
} from "../../../src/hooks/useBlackjack";
import {
  makeBlackjackState,
  createWrapper,
  renderHookAsync,
} from "../../helpers";

jest.mock("../../../src/api/blackjack");

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("useStartBlackjack", () => {
  it("calls start with gameCode and body", async () => {
    (blackjackApi.start as jest.Mock).mockResolvedValue(makeBlackjackState());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useStartBlackjack("bj1"), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({ betAmount: 10 });
    });

    expect(blackjackApi.start).toHaveBeenCalledWith("bj1", { betAmount: 10 });
  });

  it("invalidates wallet", async () => {
    (blackjackApi.start as jest.Mock).mockResolvedValue(makeBlackjackState());
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useStartBlackjack("bj1"), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({ betAmount: 10 });
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
  });
});

describe("useHit/useStand/useDouble", () => {
  it.each([
    ["useHit", useHit, blackjackApi.hit],
    ["useStand", useStand, blackjackApi.stand],
    ["useDouble", useDouble, blackjackApi.double],
  ])("%s calls API with sessionId", async (_name, hook, api) => {
    (api as jest.Mock).mockResolvedValue(makeBlackjackState());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => hook(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync("s1");
    });

    expect(api).toHaveBeenCalledWith("s1");
  });

  it.each([
    ["useHit", useHit],
    ["useStand", useStand],
    ["useDouble", useDouble],
  ])("%s invalidates wallet", async (_name, hook) => {
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => hook(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync("s1");
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
  });
});
