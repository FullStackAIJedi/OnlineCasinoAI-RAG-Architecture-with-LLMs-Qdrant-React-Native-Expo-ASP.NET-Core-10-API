import { QueryClient } from "@tanstack/react-query";
import { act, waitFor } from "@testing-library/react-native";

import { walletApi } from "../../../src/api/wallet";
import {
  useDeposit,
  useTransactions,
  useWallet,
  useWithdraw,
} from "../../../src/hooks/useWallet";
import {
  createWrapper,
  makePagedResult,
  makeTransaction,
  makeWallet,
  renderHookAsync,
} from "../../helpers";

jest.mock("../../../src/api/wallet");

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("useWallet", () => {
  it("fetches wallet", async () => {
    (walletApi.get as jest.Mock).mockResolvedValue(makeWallet());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useWallet(), {
      wrapper: createWrapper(qc),
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe("useTransactions", () => {
  it("fetches with pagination", async () => {
    (walletApi.getTransactions as jest.Mock).mockResolvedValue(
      makePagedResult([makeTransaction()]),
    );
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useTransactions(2, 30), {
      wrapper: createWrapper(qc),
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(walletApi.getTransactions).toHaveBeenCalledWith(2, 30);
  });
});

describe("useDeposit", () => {
  it("calls deposit and refetches wallet", async () => {
    (walletApi.deposit as jest.Mock).mockResolvedValue(makeWallet());
    const qc = makeQc();
    const spy = jest.spyOn(qc, "refetchQueries");

    const { result } = await renderHookAsync(() => useDeposit(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      result.current.mutate(100);
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect((walletApi.deposit as jest.Mock).mock.calls[0][0]).toBe(100);
    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
  });
});

describe("useWithdraw", () => {
  it("calls withdraw and refetches", async () => {
    (walletApi.withdraw as jest.Mock).mockResolvedValue(makeWallet());
    const qc = makeQc();
    const spy = jest.spyOn(qc, "refetchQueries");

    const { result } = await renderHookAsync(() => useWithdraw(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      result.current.mutate(50);
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect((walletApi.withdraw as jest.Mock).mock.calls[0][0]).toBe(50);
    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
  });
});
