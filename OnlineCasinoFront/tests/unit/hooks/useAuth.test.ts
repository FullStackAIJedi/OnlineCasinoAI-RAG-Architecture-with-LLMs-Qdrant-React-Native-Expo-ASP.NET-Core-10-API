import { QueryClient } from "@tanstack/react-query";
import { act, waitFor } from "@testing-library/react-native";

import { authApi } from "../../../src/api/auth";
import { ApiError } from "../../../src/api/errors";
import * as client from "../../../src/api/client";
import {
  useChangePassword,
  useIsAuthenticated,
  useLogin,
  useLogout,
  useRegister,
} from "../../../src/hooks/useAuth";
import { createWrapper, renderHookAsync } from "../../helpers";

jest.mock("../../../src/api/auth");
jest.mock("../../../src/api/client", () => ({
  ...jest.requireActual("../../../src/api/client"),
  clearCookie: jest.fn(),
  hasCookie: jest.fn(),
}));

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("useLogin", () => {
  it("calls authApi.login", async () => {
    (authApi.login as jest.Mock).mockResolvedValue({ message: "OK" });
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useLogin(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({ email: "a@b.com", password: "x" });
    });

    expect(authApi.login).toHaveBeenCalledWith({
      email: "a@b.com",
      password: "x",
    });
  });

  it("invalidates auth + wallet on success", async () => {
    (authApi.login as jest.Mock).mockResolvedValue({});
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useLogin(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({ email: "a@b.com", password: "x" });
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["auth", "hasCookie"] });
    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
  });

  it("exposes error", async () => {
    (authApi.login as jest.Mock).mockRejectedValue(new Error("Bad"));
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useLogin(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      try {
        await result.current.mutateAsync({ email: "a", password: "b" });
      } catch {}
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

describe("useRegister", () => {
  it("calls authApi.register", async () => {
    (authApi.register as jest.Mock).mockResolvedValue({});
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useRegister(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({
        email: "a",
        nickName: "n",
        password: "p",
      });
    });

    expect(authApi.register).toHaveBeenCalledWith({
      email: "a",
      nickName: "n",
      password: "p",
    });
  });

  it("invalidates auth + wallet", async () => {
    (authApi.register as jest.Mock).mockResolvedValue({});
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useRegister(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({
        email: "a",
        nickName: "n",
        password: "p",
      });
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["auth", "hasCookie"] });
    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
  });
});

describe("useLogout", () => {
  it("calls logout and clearCookie", async () => {
    (authApi.logout as jest.Mock).mockResolvedValue({});
    (client.clearCookie as jest.Mock).mockResolvedValue(undefined);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useLogout(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync();
    });

    expect(authApi.logout).toHaveBeenCalled();
    expect(client.clearCookie).toHaveBeenCalled();
  });

  it("still clears cookie if logout fails", async () => {
    (authApi.logout as jest.Mock).mockRejectedValue(new Error("network"));
    (client.clearCookie as jest.Mock).mockResolvedValue(undefined);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useLogout(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync();
    });

    expect(client.clearCookie).toHaveBeenCalled();
  });

  it("removes non-auth queries and invalidates auth", async () => {
    (authApi.logout as jest.Mock).mockResolvedValue({});
    (client.clearCookie as jest.Mock).mockResolvedValue(undefined);
    const qc = makeQc();
    const removeSpy = jest.spyOn(qc, "removeQueries");
    const invalidateSpy = jest.spyOn(qc, "invalidateQueries");

    const { result } = await renderHookAsync(() => useLogout(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync();
    });

    expect(removeSpy).toHaveBeenCalled();
    expect(invalidateSpy).toHaveBeenCalledWith({
      queryKey: ["auth", "hasCookie"],
    });
  });
});

describe("useChangePassword", () => {
  it("calls changePassword", async () => {
    (authApi.changePassword as jest.Mock).mockResolvedValue(true);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useChangePassword(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({
        currentPassword: "a",
        newPassword: "b",
      });
    });

    expect(authApi.changePassword).toHaveBeenCalledWith({
      currentPassword: "a",
      newPassword: "b",
    });
  });
});

describe("useIsAuthenticated", () => {
  it("returns false when no cookie", async () => {
    (client.hasCookie as jest.Mock).mockResolvedValue(false);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useIsAuthenticated(), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBe(false);
  });

  it("returns true when cookie + me succeed", async () => {
    (client.hasCookie as jest.Mock).mockResolvedValue(true);
    (authApi.me as jest.Mock).mockResolvedValue({});
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useIsAuthenticated(), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBe(true);
  });

  it("returns false when me returns 401", async () => {
    (client.hasCookie as jest.Mock).mockResolvedValue(true);
    (authApi.me as jest.Mock).mockRejectedValue(new ApiError(401, "x"));
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useIsAuthenticated(), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBe(false);
  });

  it("returns true when cookie exists but me fails with non-401", async () => {
    (client.hasCookie as jest.Mock).mockResolvedValue(true);
    (authApi.me as jest.Mock).mockRejectedValue(new Error("network"));
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useIsAuthenticated(), {
      wrapper: createWrapper(qc),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBe(true);
  });
});
