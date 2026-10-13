import { blackjackApi } from "../../../src/api/blackjack";
import { apiFetch } from "../../../src/api/client";

jest.mock("../../../src/api/client");

describe("blackjackApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue({});
  });

  it("start POSTs to table start", async () => {
    await blackjackApi.start("bj1", { betAmount: 10 });
    expect(apiFetch).toHaveBeenCalledWith("/api/games/blackjack/bj1/start", {
      method: "POST",
      body: { betAmount: 10 },
    });
  });

  it("hit POSTs to sessions hit", async () => {
    await blackjackApi.hit("s1");
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/games/blackjack/sessions/s1/hit",
      { method: "POST" },
    );
  });

  it("stand POSTs to sessions stand", async () => {
    await blackjackApi.stand("s1");
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/games/blackjack/sessions/s1/stand",
      { method: "POST" },
    );
  });

  it("double POSTs to sessions double", async () => {
    await blackjackApi.double("s1");
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/games/blackjack/sessions/s1/double",
      { method: "POST" },
    );
  });

  it("getState GETs session", async () => {
    await blackjackApi.getState("s1");
    expect(apiFetch).toHaveBeenCalledWith("/api/games/blackjack/sessions/s1");
  });
});
