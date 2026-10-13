import { QueryClient } from "@tanstack/react-query";
import { act, waitFor } from "@testing-library/react-native";

import { usersApi } from "../../../src/api/users";
import {
  useAllUsers,
  useBlockUser,
  useDeleteAccount,
  useProfile,
  useUnblockUser,
  useUpdateProfile,
} from "../../../src/hooks/useUsers";
import {
  createWrapper,
  makeUserListItem,
  makeUserProfile,
  renderHookAsync,
} from "../../helpers";

jest.mock("../../../src/api/users");

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("useProfile", () => {
  it("fetches profile", async () => {
    (usersApi.getMe as jest.Mock).mockResolvedValue(makeUserProfile());
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useProfile(), {
      wrapper: createWrapper(qc),
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe("useUpdateProfile", () => {
  it("updates and invalidates", async () => {
    (usersApi.updateMe as jest.Mock).mockResolvedValue(makeUserProfile());
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useUpdateProfile(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({ firstName: "a", pofileImage: "" });
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["users", "me"] });
  });
});

describe("useDeleteAccount", () => {
  it("calls deleteMe", async () => {
    (usersApi.deleteMe as jest.Mock).mockResolvedValue(true);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useDeleteAccount(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync();
    });

    expect(usersApi.deleteMe).toHaveBeenCalled();
  });
});

describe("useAllUsers", () => {
  it("fetches all users", async () => {
    (usersApi.getAll as jest.Mock).mockResolvedValue([makeUserListItem()]);
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useAllUsers(), {
      wrapper: createWrapper(qc),
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe("useBlockUser / useUnblockUser", () => {
  it("block invalidates users", async () => {
    (usersApi.block as jest.Mock).mockResolvedValue(true);
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useBlockUser(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync(5);
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["users"] });
  });

  it("unblock invalidates users", async () => {
    (usersApi.unblock as jest.Mock).mockResolvedValue(true);
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useUnblockUser(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync(5);
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["users"] });
  });
});
