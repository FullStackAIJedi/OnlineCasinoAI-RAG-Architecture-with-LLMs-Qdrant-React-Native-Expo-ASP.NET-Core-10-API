import { ApiError } from "../../../src/api/errors";
import { createQueryClient } from "../../../src/lib/queryClient";

describe("createQueryClient", () => {
  it("returns a QueryClient", () => {
    const qc = createQueryClient();
    expect(qc).toBeDefined();
    expect(typeof qc.invalidateQueries).toBe("function");
  });

  it("retries non-401 errors once", async () => {
    const qc = createQueryClient();
    const retry = qc.getDefaultOptions().queries?.retry as Function;
    expect(retry(0, new Error("x"))).toBe(true);
    expect(retry(1, new Error("x"))).toBe(false);
  });

  it("does not retry on 401", () => {
    const qc = createQueryClient();
    const retry = qc.getDefaultOptions().queries?.retry as Function;
    expect(retry(0, new ApiError(401, "x"))).toBe(false);
  });

  it("sets staleTime to 30s", () => {
    const qc = createQueryClient();
    expect(qc.getDefaultOptions().queries?.staleTime).toBe(30 * 1000);
  });

  it("disables refetchOnWindowFocus", () => {
    const qc = createQueryClient();
    expect(qc.getDefaultOptions().queries?.refetchOnWindowFocus).toBe(false);
  });

  it("invalidates auth queries on 401 query error", async () => {
    const qc = createQueryClient();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const cache = qc.getQueryCache();
    const query = cache.build(qc, {
      queryKey: ["test"],
      queryFn: async () => {
        throw new ApiError(401, "Unauth");
      },
    });
    await query.fetch().catch(() => {});
    expect(spy).toHaveBeenCalledWith({ queryKey: ["auth", "hasCookie"] });
    expect(spy).toHaveBeenCalledWith({ queryKey: ["auth", "me"] });
  });
});
