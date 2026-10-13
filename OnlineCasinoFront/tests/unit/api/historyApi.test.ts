import { apiFetch } from "../../../src/api/client";
import { historyApi } from "../../../src/api/history";

jest.mock("../../../src/api/client");

describe("historyApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue({ items: [] });
  });

  it("getAll with no game", async () => {
    await historyApi.getAll();
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/game-history?page=1&pageSize=20",
    );
  });

  it("getAll with game filter", async () => {
    await historyApi.getAll("slots");
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/game-history?game=slots&page=1&pageSize=20",
    );
  });

  it("getAll with custom page/pageSize", async () => {
    await historyApi.getAll(undefined, 3, 50);
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/game-history?page=3&pageSize=50",
    );
  });

  it("getAll URL-encodes game", async () => {
    await historyApi.getAll("with space");
    expect(apiFetch).toHaveBeenCalledWith(
      expect.stringContaining("game=with+space"),
    );
  });

  it("getByRoundId GETs round", async () => {
    await historyApi.getByRoundId("r1");
    expect(apiFetch).toHaveBeenCalledWith("/api/game-history/r1");
  });
});
