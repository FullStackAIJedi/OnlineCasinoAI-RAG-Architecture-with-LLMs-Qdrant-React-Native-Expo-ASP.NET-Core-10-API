import { GAMES, GAME_OPTIONS, RED_NUMBERS } from "../../../src/utils/constants";

describe("GAMES", () => {
  it("has 4 entries", () => {
    expect(GAMES).toHaveLength(4);
  });

  it("each has code/name/image/route", () => {
    GAMES.forEach((g) => {
      expect(g.code).toBeTruthy();
      expect(g.name).toBeTruthy();
      expect(g.image).toBeDefined();
      expect(g.route).toMatch(/^\/games\//);
    });
  });

  it("codes match backend conventions", () => {
    const codes = GAMES.map((g) => g.code);
    expect(codes).toContain("slots");
    expect(codes).toContain("slots4");
    expect(codes).toContain("roulette");
    expect(codes).toContain("blackjack");
  });
});

describe("GAME_OPTIONS", () => {
  it("starts with empty value (all games)", () => {
    expect(GAME_OPTIONS[0].value).toBe("");
  });

  it("has 4 options", () => {
    expect(GAME_OPTIONS).toHaveLength(4);
  });
});

describe("RED_NUMBERS", () => {
  it("has 18 entries", () => {
    expect(RED_NUMBERS.size).toBe(18);
  });

  it("contains 1 and 36", () => {
    expect(RED_NUMBERS.has(1)).toBe(true);
    expect(RED_NUMBERS.has(36)).toBe(true);
  });

  it("does not contain 0", () => {
    expect(RED_NUMBERS.has(0)).toBe(false);
  });

  it("contains only 1-36", () => {
    RED_NUMBERS.forEach((n) => {
      expect(n).toBeGreaterThanOrEqual(1);
      expect(n).toBeLessThanOrEqual(36);
    });
  });
});
