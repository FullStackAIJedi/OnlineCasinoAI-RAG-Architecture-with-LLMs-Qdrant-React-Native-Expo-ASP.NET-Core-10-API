import { QueryClient } from "@tanstack/react-query";
import { waitFor } from "@testing-library/react-native";

import { gamesApi } from "../../../src/api/games";
import { useGame, useGames } from "../../../src/hooks/useGames";
import { createWrapper, makeGame, renderHookAsync } from "../../helpers";

jest.mock("../../../src/api/games");

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("useGames", () => {
  it("fetches all games", async () => {
    (gamesApi.getAll as jest.Mock).mockResolvedValue([makeGame()]);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useGames(), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
  });
});

describe("useGame", () => {
  it("fetches by code", async () => {
    (gamesApi.getByCode as jest.Mock).mockResolvedValue(makeGame());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useGame("blackjack"), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(gamesApi.getByCode).toHaveBeenCalledWith("blackjack");
  });

  it("is disabled when code is empty", async () => {
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useGame(""), {
      wrapper: createWrapper(qc),
    });

    expect(result.current.fetchStatus).toBe("idle");
    expect(gamesApi.getByCode).not.toHaveBeenCalled();
  });
});
