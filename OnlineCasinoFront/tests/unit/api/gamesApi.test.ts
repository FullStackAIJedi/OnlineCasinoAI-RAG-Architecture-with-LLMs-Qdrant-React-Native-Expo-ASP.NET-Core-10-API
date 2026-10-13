import { apiFetch } from "../../../src/api/client";
import { gamesApi } from "../../../src/api/games";

jest.mock("../../../src/api/client");

describe("gamesApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue([]);
  });

  it("getAll GETs /api/games", async () => {
    await gamesApi.getAll();
    expect(apiFetch).toHaveBeenCalledWith("/api/games");
  });

  it("getByCode GETs specific game", async () => {
    await gamesApi.getByCode("blackjack");
    expect(apiFetch).toHaveBeenCalledWith("/api/games/blackjack");
  });
});
