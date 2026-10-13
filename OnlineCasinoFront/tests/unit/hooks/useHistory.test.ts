import { QueryClient } from "@tanstack/react-query";
import { waitFor } from "@testing-library/react-native";

import { historyApi } from "../../../src/api/history";
import { useHistory, useRound } from "../../../src/hooks/useHistory";
import {
  createWrapper,
  makeGameRound,
  makePagedResult,
  renderHookAsync,
} from "../../helpers";

jest.mock("../../../src/api/history");

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("useHistory", () => {
  it("fetches history with defaults", async () => {
    (historyApi.getAll as jest.Mock).mockResolvedValue(makePagedResult());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useHistory(), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(historyApi.getAll).toHaveBeenCalledWith(undefined, 1, 20);
  });

  it("fetches history with filter", async () => {
    (historyApi.getAll as jest.Mock).mockResolvedValue(makePagedResult());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useHistory("slots"), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(historyApi.getAll).toHaveBeenCalledWith("slots", 1, 20);
  });

  it("uses different query key per page", async () => {
    (historyApi.getAll as jest.Mock).mockResolvedValue(makePagedResult());
    const qc = makeQc();
    await renderHookAsync(() => useHistory("slots", 2), {
      wrapper: createWrapper(qc),
    });
    await waitFor(() =>
      expect(historyApi.getAll).toHaveBeenCalledWith("slots", 2, 20),
    );
  });
});

describe("useRound", () => {
  it("fetches round by id", async () => {
    (historyApi.getByRoundId as jest.Mock).mockResolvedValue(makeGameRound());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useRound("r1"), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(historyApi.getByRoundId).toHaveBeenCalledWith("r1");
  });

  it("disabled when empty", async () => {
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useRound(""), {
      wrapper: createWrapper(qc),
    });
    expect(result.current.fetchStatus).toBe("idle");
  });
});
