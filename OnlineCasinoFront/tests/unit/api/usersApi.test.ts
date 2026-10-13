import { apiFetch } from "../../../src/api/client";
import { usersApi } from "../../../src/api/users";

jest.mock("../../../src/api/client");

describe("usersApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (apiFetch as jest.Mock).mockResolvedValue({});
  });

  it("getMe GETs", async () => {
    await usersApi.getMe();
    expect(apiFetch).toHaveBeenCalledWith("/api/users/me");
  });

  it("updateMe PUTs", async () => {
    const data = { firstName: "a", pofileImage: "" };
    await usersApi.updateMe(data);
    expect(apiFetch).toHaveBeenCalledWith("/api/users/me", {
      method: "PUT",
      body: data,
    });
  });

  it("deleteMe DELETEs", async () => {
    await usersApi.deleteMe();
    expect(apiFetch).toHaveBeenCalledWith("/api/users/me", {
      method: "DELETE",
    });
  });

  it("getAll GETs", async () => {
    await usersApi.getAll();
    expect(apiFetch).toHaveBeenCalledWith("/api/users");
  });

  it("block POSTs", async () => {
    await usersApi.block(5);
    expect(apiFetch).toHaveBeenCalledWith("/api/users/5/block", {
      method: "POST",
    });
  });

  it("unblock POSTs", async () => {
    await usersApi.unblock(5);
    expect(apiFetch).toHaveBeenCalledWith("/api/users/5/unblock", {
      method: "POST",
    });
  });
});
