import {
  ALL_SYMBOL_CODES,
  getSymbol,
  SLOT_SYMBOLS,
} from "../../../../src/components/slots/symbols";

jest.mock("@expo/vector-icons", () => ({ Ionicons: () => null }));

describe("SLOT_SYMBOLS", () => {
  it("has 8 symbols", () => {
    expect(Object.keys(SLOT_SYMBOLS)).toHaveLength(8);
  });

  it("each has code/emoji/color/icon/label", () => {
    Object.values(SLOT_SYMBOLS).forEach((s) => {
      expect(s.code).toBeTruthy();
      expect(s.emoji).toBeTruthy();
      expect(s.color).toMatch(/^#/);
      expect(s.icon).toBeTruthy();
      expect(s.label).toBeTruthy();
    });
  });

  it("contains cherry and seven", () => {
    expect(SLOT_SYMBOLS.cherry).toBeDefined();
    expect(SLOT_SYMBOLS.seven).toBeDefined();
  });
});

describe("ALL_SYMBOL_CODES", () => {
  it("returns all codes", () => {
    expect(ALL_SYMBOL_CODES).toHaveLength(8);
  });
});

describe("getSymbol", () => {
  it("returns symbol for known code", () => {
    expect(getSymbol("cherry").emoji).toBe("🍒");
  });

  it("returns fallback for unknown", () => {
    expect(getSymbol("unknown").code).toBe("?");
  });

  it("returns fallback for undefined", () => {
    expect(getSymbol(undefined).code).toBe("?");
  });

  it("returns fallback for empty", () => {
    expect(getSymbol("").code).toBe("?");
  });
});
