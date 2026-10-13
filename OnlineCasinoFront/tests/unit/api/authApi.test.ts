import { authApi } from "../../../src/api/auth";
import { apiFetch } from "../../../src/api/client";

jest.mock("../../../src/api/client");

describe("authApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue({ message: "OK" });
  });

  it("register POSTs with skipAuth", async () => {
    const body = { email: "a@b.com", nickName: "n", password: "p" };
    await authApi.register(body);
    expect(apiFetch).toHaveBeenCalledWith("/api/auth/register", {
      method: "POST",
      body,
      skipAuth: true,
    });
  });

  it("login POSTs with skipAuth", async () => {
    const body = { email: "a@b.com", password: "p" };
    await authApi.login(body);
    expect(apiFetch).toHaveBeenCalledWith("/api/auth/login", {
      method: "POST",
      body,
      skipAuth: true,
    });
  });

  it("logout POSTs", async () => {
    await authApi.logout();
    expect(apiFetch).toHaveBeenCalledWith("/api/auth/logout", {
      method: "POST",
    });
  });

  it("changePassword POSTs", async () => {
    const body = { currentPassword: "a", newPassword: "b" };
    await authApi.changePassword(body);
    expect(apiFetch).toHaveBeenCalledWith("/api/auth/change-password", {
      method: "POST",
      body,
    });
  });

  it("me GETs", async () => {
    await authApi.me();
    expect(apiFetch).toHaveBeenCalledWith("/api/auth/me");
  });
});
