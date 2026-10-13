import {
  CARD_BACK,
  CARD_BACK_RED,
  CARD_IMAGES,
  getCardImage,
} from "../../../../src/components/blackjack/cardImages";

describe("CARD_IMAGES", () => {
  it("has 52 cards", () => {
    expect(Object.keys(CARD_IMAGES)).toHaveLength(52);
  });

  it("contains all suits", () => {
    expect(CARD_IMAGES["A♠"]).toBeDefined();
    expect(CARD_IMAGES["A♦"]).toBeDefined();
    expect(CARD_IMAGES["A♣"]).toBeDefined();
    expect(CARD_IMAGES["A♥"]).toBeDefined();
  });

  it("contains face cards", () => {
    expect(CARD_IMAGES["J♠"]).toBeDefined();
    expect(CARD_IMAGES["Q♠"]).toBeDefined();
    expect(CARD_IMAGES["K♠"]).toBeDefined();
  });
});

describe("CARD_BACK", () => {
  it("is defined", () => {
    expect(CARD_BACK).toBeDefined();
  });

  it("CARD_BACK_RED defined", () => {
    expect(CARD_BACK_RED).toBeDefined();
  });
});

describe("getCardImage", () => {
  it("returns card for known", () => {
    expect(getCardImage("A♠")).toBe(CARD_IMAGES["A♠"]);
  });

  it("returns back for ?", () => {
    expect(getCardImage("?")).toBe(CARD_BACK);
  });

  it("returns back for ??", () => {
    expect(getCardImage("??")).toBe(CARD_BACK);
  });

  it("returns back for empty", () => {
    expect(getCardImage("")).toBe(CARD_BACK);
  });

  it("returns back for unknown", () => {
    expect(getCardImage("ZZ")).toBe(CARD_BACK);
  });
});
