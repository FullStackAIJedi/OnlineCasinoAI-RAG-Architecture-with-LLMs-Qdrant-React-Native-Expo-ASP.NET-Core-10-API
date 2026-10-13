import { parseRouletteNumber } from "../../../src/hooks/useRouletteScreen";

describe("parseRouletteNumber", () => {
  it("parses 0", () => {
    expect(parseRouletteNumber("0")).toBe(0);
  });

  it("parses 36", () => {
    expect(parseRouletteNumber("36")).toBe(36);
  });

  it("maps 00 to 37", () => {
    expect(parseRouletteNumber("00")).toBe(37);
  });

  it("trims whitespace", () => {
    expect(parseRouletteNumber("  5  ")).toBe(5);
  });

  it("returns null for empty", () => {
    expect(parseRouletteNumber("")).toBeNull();
  });

  it("returns null for letters", () => {
    expect(parseRouletteNumber("abc")).toBeNull();
  });

  it("returns null for whitespace-only", () => {
    expect(parseRouletteNumber("   ")).toBeNull();
  });

  it("parses negative numbers (validated later)", () => {
    expect(parseRouletteNumber("-5")).toBe(-5);
  });

  it("parses decimal as int", () => {
    expect(parseRouletteNumber("5.9")).toBe(5);
  });
});
