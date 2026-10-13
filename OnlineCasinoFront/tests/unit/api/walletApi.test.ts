import { apiFetch } from "../../../src/api/client";
import { walletApi } from "../../../src/api/wallet";

jest.mock("../../../src/api/client");

describe("walletApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue({});
  });

  it("get GETs", async () => {
    await walletApi.get();
    expect(apiFetch).toHaveBeenCalledWith("/api/wallet");
  });

  it("getTransactions default pagination", async () => {
    await walletApi.getTransactions();
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/wallet/transactions?page=1&pageSize=20",
    );
  });

  it("getTransactions custom pagination", async () => {
    await walletApi.getTransactions(3, 50);
    expect(apiFetch).toHaveBeenCalledWith(
      "/api/wallet/transactions?page=3&pageSize=50",
    );
  });

  it("deposit POSTs amount", async () => {
    await walletApi.deposit(100);
    expect(apiFetch).toHaveBeenCalledWith("/api/wallet/deposit", {
      method: "POST",
      body: { amount: 100 },
    });
  });

  it("withdraw POSTs amount", async () => {
    await walletApi.withdraw(50);
    expect(apiFetch).toHaveBeenCalledWith("/api/wallet/withdraw", {
      method: "POST",
      body: { amount: 50 },
    });
  });
});
