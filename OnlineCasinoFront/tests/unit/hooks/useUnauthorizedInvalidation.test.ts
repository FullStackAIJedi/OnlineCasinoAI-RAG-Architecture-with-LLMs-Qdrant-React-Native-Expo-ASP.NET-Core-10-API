import { QueryClient } from "@tanstack/react-query";
import { act, waitFor, renderHook } from "@testing-library/react-native";

import * as client from "../../../src/api/client";
import { useUnauthorizedInvalidation } from "../../../src/hooks/useUnauthorizedInvalidation";
import { createWrapper, renderHookAsync } from "../../helpers";

describe("useUnauthorizedInvalidation", () => {
  it("subscribes on mount", async () => {
    const spy = jest.spyOn(client, "onUnauthorized");
    const qc = new QueryClient();
    await renderHookAsync(() => useUnauthorizedInvalidation(), {
      wrapper: createWrapper(qc),
    });
    await waitFor(() => expect(spy).toHaveBeenCalled());
  });

  it("invalidates auth queries on event", async () => {
    let listener: () => void = () => {};
    jest.spyOn(client, "onUnauthorized").mockImplementation((l) => {
      listener = l;
      return () => {};
    });

    const qc = new QueryClient();
    const spy = jest.spyOn(qc, "invalidateQueries");
    await renderHookAsync(() => useUnauthorizedInvalidation(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      listener();
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["auth", "hasCookie"] });
    expect(spy).toHaveBeenCalledWith({ queryKey: ["auth", "me"] });
  });
});
