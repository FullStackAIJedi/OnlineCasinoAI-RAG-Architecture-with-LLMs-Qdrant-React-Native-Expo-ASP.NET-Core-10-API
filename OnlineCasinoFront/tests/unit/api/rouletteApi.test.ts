import { apiFetch } from "../../../src/api/client";
import { rouletteApi } from "../../../src/api/roulette";

jest.mock("../../../src/api/client");

describe("rouletteApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue({});
  });

  it("getBets GETs bets", async () => {
    await rouletteApi.getBets("r1");
    expect(apiFetch).toHaveBeenCalledWith("/api/games/roulette/r1/bets");
  });

  it("placeBet POSTs", async () => {
    const data = { type: "red", amount: 10 };
    await rouletteApi.placeBet("r1", data);
    expect(apiFetch).toHaveBeenCalledWith("/api/games/roulette/r1/bet", {
      method: "POST",
      body: data,
    });
  });

  it("spin POSTs", async () => {
    const data = { clientRoundId: "uuid" };
    await rouletteApi.spin("r1", data);
    expect(apiFetch).toHaveBeenCalledWith("/api/games/roulette/r1/spin", {
      method: "POST",
      body: data,
    });
  });
});
