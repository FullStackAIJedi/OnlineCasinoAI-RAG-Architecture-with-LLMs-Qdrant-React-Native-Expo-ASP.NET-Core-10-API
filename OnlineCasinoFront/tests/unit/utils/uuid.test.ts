import { uuid } from "../../../src/utils/uuid";

describe("uuid", () => {
  it("returns a string", () => {
    expect(typeof uuid()).toBe("string");
  });

  it("matches v4 format", () => {
    expect(uuid()).toMatch(
      /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/,
    );
  });

  it("returns unique values", () => {
    const set = new Set(Array.from({ length: 100 }, uuid));
    expect(set.size).toBe(100);
  });

  it("always has version 4", () => {
    for (let i = 0; i < 20; i++) {
      expect(uuid()[14]).toBe("4");
    }
  });
});
