import { apiFetch } from "../../../src/api/client";
import { slotsApi } from "../../../src/api/slots";

jest.mock("../../../src/api/client");

describe("slotsApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue({});
  });

  it("spin POSTs", async () => {
    const data = { betAmount: 10, clientRoundId: "uuid" };
    await slotsApi.spin("slots", data);
    expect(apiFetch).toHaveBeenCalledWith("/api/games/slots/slots/spin", {
      method: "POST",
      body: data,
    });
  });
});
