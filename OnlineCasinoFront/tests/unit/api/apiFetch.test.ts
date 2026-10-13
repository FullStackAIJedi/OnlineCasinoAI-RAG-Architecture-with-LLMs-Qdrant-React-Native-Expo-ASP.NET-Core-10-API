import * as SecureStore from "expo-secure-store";

import {
  apiFetch,
  clearCookie,
  hasCookie,
  onUnauthorized,
} from "../../../src/api/client";
import { ApiError } from "../../../src/api/errors";

jest.mock("expo-secure-store");

const mockFetch = jest.fn();
global.fetch = mockFetch as any;

const okResponse = (body: any = {}, setCookie?: string) => ({
  ok: true,
  status: 200,
  headers: {
    get: (name: string) =>
      name === "set-cookie" && setCookie ? setCookie : null,
  },
  text: async () => JSON.stringify(body),
});

const errResponse = (status: number, body: any = {}) => ({
  ok: false,
  status,
  headers: { get: () => null },
  text: async () => JSON.stringify(body),
});

describe("apiFetch", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (SecureStore.getItemAsync as jest.Mock).mockResolvedValue(null);
  });

  describe("Success", () => {
    it("returns parsed JSON", async () => {
      mockFetch.mockResolvedValue(okResponse({ id: 1, name: "Test" }));
      const result = await apiFetch("/users/1");
      expect(result).toEqual({ id: 1, name: "Test" });
    });

    it("returns undefined on 204", async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 204,
        headers: { get: () => null },
      });
      const result = await apiFetch("/x", { method: "POST" });
      expect(result).toBeUndefined();
    });

    it("handles non-JSON text response", async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        headers: { get: () => null },
        text: async () => "plain text",
      });
      const result = await apiFetch("/text");
      expect(result).toBe("plain text");
    });

    it("handles empty body", async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        headers: { get: () => null },
        text: async () => "",
      });
      const result = await apiFetch("/empty");
      expect(result).toBeNull();
    });

    it("prepends BASE_URL", async () => {
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/test");
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining("/test"),
        expect.anything(),
      );
    });
  });

  describe("Errors", () => {
    it("throws ApiError on 404", async () => {
      mockFetch.mockResolvedValue(errResponse(404, { message: "Not found" }));
      await expect(apiFetch("/x")).rejects.toThrow(ApiError);
    });

    it("uses message from body", async () => {
      mockFetch.mockResolvedValue(errResponse(400, { message: "Bad" }));
      await expect(apiFetch("/x")).rejects.toThrow("Bad");
    });

    it("uses title when message missing", async () => {
      mockFetch.mockResolvedValue(errResponse(400, { title: "Bad req" }));
      await expect(apiFetch("/x")).rejects.toThrow("Bad req");
    });

    it("uses first error from errors object", async () => {
      mockFetch.mockResolvedValue(
        errResponse(422, { errors: { email: ["Invalid"] } }),
      );
      await expect(apiFetch("/x")).rejects.toThrow("Invalid");
    });

    it("uses statusCode from body", async () => {
      mockFetch.mockResolvedValue(errResponse(500, { statusCode: 503 }));
      try {
        await apiFetch("/x");
      } catch (e) {
        expect((e as ApiError).status).toBe(503);
      }
    });

    it("uses status from body when statusCode missing", async () => {
      mockFetch.mockResolvedValue(errResponse(500, { status: 502 }));
      try {
        await apiFetch("/x");
      } catch (e) {
        expect((e as ApiError).status).toBe(502);
      }
    });

    it("falls back to generic message", async () => {
      mockFetch.mockResolvedValue(errResponse(500, {}));
      await expect(apiFetch("/x")).rejects.toThrow("Something went wrong");
    });

    it("handles AbortError as 408", async () => {
      const abortError = new Error("Aborted");
      abortError.name = "AbortError";
      mockFetch.mockRejectedValue(abortError);
      await expect(apiFetch("/x")).rejects.toThrow("Request timed out");
    });

    it("rethrows non-abort errors", async () => {
      mockFetch.mockRejectedValue(new Error("Network fail"));
      await expect(apiFetch("/x")).rejects.toThrow("Network fail");
    });
  });

  describe("Cookies", () => {
    it("persists access_token", async () => {
      mockFetch.mockResolvedValue(
        okResponse({}, "access_token=abc123; Path=/"),
      );
      await apiFetch("/login", { method: "POST" });
      expect(SecureStore.setItemAsync).toHaveBeenCalledWith(
        "access_token_cookie",
        "access_token=abc123",
      );
    });

    it("sends saved cookie", async () => {
      (SecureStore.getItemAsync as jest.Mock).mockResolvedValue(
        "access_token=xyz",
      );
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/users");
      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({ Cookie: "access_token=xyz" }),
        }),
      );
    });

    it("skips cookie when skipAuth", async () => {
      (SecureStore.getItemAsync as jest.Mock).mockResolvedValue("token");
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/public", { skipAuth: true });
      const headers = mockFetch.mock.calls[0][1].headers;
      expect(headers.Cookie).toBeUndefined();
    });

    it("does not send cookie when not stored", async () => {
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/users");
      expect(mockFetch.mock.calls[0][1].headers.Cookie).toBeUndefined();
    });

    it("clears cookie and emits on 401", async () => {
      const listener = jest.fn();
      onUnauthorized(listener);
      mockFetch.mockResolvedValue(errResponse(401, { message: "Unauth" }));
      await expect(apiFetch("/x")).rejects.toThrow();
      expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith(
        "access_token_cookie",
      );
      expect(listener).toHaveBeenCalled();
    });

    it("does not emit on 401 when skipAuth", async () => {
      const listener = jest.fn();
      onUnauthorized(listener);
      mockFetch.mockResolvedValue(errResponse(401, { message: "Unauth" }));
      await expect(apiFetch("/x", { skipAuth: true })).rejects.toThrow();
      expect(listener).not.toHaveBeenCalled();
    });

    it("unsubscribe works", async () => {
      const listener = jest.fn();
      const unsub = onUnauthorized(listener);
      unsub();
      mockFetch.mockResolvedValue(errResponse(401));
      await expect(apiFetch("/x")).rejects.toThrow();
      expect(listener).not.toHaveBeenCalled();
    });
  });

  describe("Request options", () => {
    it("sets Content-Type when body", async () => {
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/users", { method: "POST", body: { name: "J" } });
      const args = mockFetch.mock.calls[0][1];
      expect(args.headers["Content-Type"]).toBe("application/json");
      expect(args.body).toBe(JSON.stringify({ name: "J" }));
    });

    it("does not set Content-Type without body", async () => {
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/users", { method: "DELETE" });
      expect(
        mockFetch.mock.calls[0][1].headers["Content-Type"],
      ).toBeUndefined();
    });

    it("merges custom headers", async () => {
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/x", { headers: { "X-Custom": "yes" } });
      expect(mockFetch.mock.calls[0][1].headers["X-Custom"]).toBe("yes");
    });

    it("always sets Accept header", async () => {
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/x");
      expect(mockFetch.mock.calls[0][1].headers.Accept).toBe(
        "application/json",
      );
    });

    it("uses credentials include", async () => {
      mockFetch.mockResolvedValue(okResponse());
      await apiFetch("/x");
      expect(mockFetch.mock.calls[0][1].credentials).toBe("include");
    });
  });

  describe("clearCookie / hasCookie", () => {
    it("clearCookie deletes item", async () => {
      await clearCookie();
      expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith(
        "access_token_cookie",
      );
    });

    it("hasCookie returns true when present", async () => {
      (SecureStore.getItemAsync as jest.Mock).mockResolvedValue("abc");
      expect(await hasCookie()).toBe(true);
    });

    it("hasCookie returns false when null", async () => {
      (SecureStore.getItemAsync as jest.Mock).mockResolvedValue(null);
      expect(await hasCookie()).toBe(false);
    });
  });
});
