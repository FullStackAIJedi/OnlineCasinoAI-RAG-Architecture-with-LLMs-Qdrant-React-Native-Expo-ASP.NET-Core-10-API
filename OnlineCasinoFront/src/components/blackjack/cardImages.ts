// /**
//  * Мапва card код (напр. "A♠") към require() на съответния PNG файл.
//  * React Native изисква статични require() – не можеш динамично да строиш път.
//  *
//  * Файловете са в assets/cards/ с имена от сорта на "01_A_spades.png".
//  */

// export const CARD_IMAGES: Record<string, any> = {
//   // Spades ♠ (01-13)
//   "A♠":  require("../../../assets/cards/01_A_spades.png"),
//   "2♠":  require("../../../assets/cards/02_2_spades.png"),
//   "3♠":  require("../../../assets/cards/03_3_spades.png"),
//   "4♠":  require("../../../assets/cards/04_4_spades.png"),
//   "5♠":  require("../../../assets/cards/05_5_spades.png"),
//   "6♠":  require("../../../assets/cards/06_6_spades.png"),
//   "7♠":  require("../../../assets/cards/07_7_spades.png"),
//   "8♠":  require("../../../assets/cards/08_8_spades.png"),
//   "9♠":  require("../../../assets/cards/09_9_spades.png"),
//   "10♠": require("../../../assets/cards/10_10_spades.png"),
//   "J♠":  require("../../../assets/cards/11_J_spades.png"),
//   "Q♠":  require("../../../assets/cards/12_Q_spades.png"),
//   "K♠":  require("../../../assets/cards/13_K_spades.png"),

//   // Diamonds ♦ (14-26)
//   "A♦":  require("../../../assets/cards/14_A_diamonds.png"),
//   "2♦":  require("../../../assets/cards/15_2_diamonds.png"),
//   "3♦":  require("../../../assets/cards/16_3_diamonds.png"),
//   "4♦":  require("../../../assets/cards/17_4_diamonds.png"),
//   "5♦":  require("../../../assets/cards/18_5_diamonds.png"),
//   "6♦":  require("../../../assets/cards/19_6_diamonds.png"),
//   "7♦":  require("../../../assets/cards/20_7_diamonds.png"),
//   "8♦":  require("../../../assets/cards/21_8_diamonds.png"),
//   "9♦":  require("../../../assets/cards/22_9_diamonds.png"),
//   "10♦": require("../../../assets/cards/23_10_diamonds.png"),
//   "J♦":  require("../../../assets/cards/24_J_diamonds.png"),
//   "Q♦":  require("../../../assets/cards/25_Q_diamonds.png"),
//   "K♦":  require("../../../assets/cards/26_K_diamonds.png"),

//   // Clubs ♣ (27-39)
//   "A♣":  require("../../../assets/cards/27_A_clubs.png"),
//   "2♣":  require("../../../assets/cards/28_2_clubs.png"),
//   "3♣":  require("../../../assets/cards/29_3_clubs.png"),
//   "4♣":  require("../../../assets/cards/30_4_clubs.png"),
//   "5♣":  require("../../../assets/cards/31_5_clubs.png"),
//   "6♣":  require("../../../assets/cards/32_6_clubs.png"),
//   "7♣":  require("../../../assets/cards/33_7_clubs.png"),
//   "8♣":  require("../../../assets/cards/34_8_clubs.png"),
//   "9♣":  require("../../../assets/cards/35_9_clubs.png"),
//   "10♣": require("../../../assets/cards/36_10_clubs.png"),
//   "J♣":  require("../../../assets/cards/37_J_clubs.png"),
//   "Q♣":  require("../../../assets/cards/38_Q_clubs.png"),
//   "K♣":  require("../../../assets/cards/39_K_clubs.png"),

//   // Hearts ♥ (40-52)
//   "A♥":  require("../../../assets/cards/40_A_hearts.png"),
//   "2♥":  require("../../../assets/cards/41_2_hearts.png"),
//   "3♥":  require("../../../assets/cards/42_3_hearts.png"),
//   "4♥":  require("../../../assets/cards/43_4_hearts.png"),
//   "5♥":  require("../../../assets/cards/44_5_hearts.png"),
//   "6♥":  require("../../../assets/cards/45_6_hearts.png"),
//   "7♥":  require("../../../assets/cards/46_7_hearts.png"),
//   "8♥":  require("../../../assets/cards/47_8_hearts.png"),
//   "9♥":  require("../../../assets/cards/48_9_hearts.png"),
//   "10♥": require("../../../assets/cards/49_10_hearts.png"),
//   "J♥":  require("../../../assets/cards/50_J_hearts.png"),
//   "Q♥":  require("../../../assets/cards/51_Q_hearts.png"),
//   "K♥":  require("../../../assets/cards/52_K_hearts.png"),
// };

// /** Гръб на картата – син (за дилъра). */
// export const CARD_BACK = require("../../../assets/cards/back_blue.png");

// /** Алтернативен червен гръб – ако искаш да го ползваш някъде. */
// export const CARD_BACK_RED = require("../../../assets/cards/back_red.png");

// /**
//  * Връща изображението за дадена карта.
//  * Ако картата е "?" или липсва → връща гърба.
//  */
// export const getCardImage = (card: string) => {
//   if (!card || card === "?" || card === "??") {
//     return CARD_BACK;
//   }
//   return CARD_IMAGES[card] ?? CARD_BACK;
// };

/**
 * Static map from card code (e.g. "A♠") to its PNG asset.
 *
 * React Native requires static `require()` calls, so the mapping
 * is declared explicitly rather than built dynamically.
 */
export const CARD_IMAGES: Record<string, number> = {
  "A♠": require("../../../assets/cards/01_A_spades.png"),
  "2♠": require("../../../assets/cards/02_2_spades.png"),
  "3♠": require("../../../assets/cards/03_3_spades.png"),
  "4♠": require("../../../assets/cards/04_4_spades.png"),
  "5♠": require("../../../assets/cards/05_5_spades.png"),
  "6♠": require("../../../assets/cards/06_6_spades.png"),
  "7♠": require("../../../assets/cards/07_7_spades.png"),
  "8♠": require("../../../assets/cards/08_8_spades.png"),
  "9♠": require("../../../assets/cards/09_9_spades.png"),
  "10♠": require("../../../assets/cards/10_10_spades.png"),
  "J♠": require("../../../assets/cards/11_J_spades.png"),
  "Q♠": require("../../../assets/cards/12_Q_spades.png"),
  "K♠": require("../../../assets/cards/13_K_spades.png"),

  "A♦": require("../../../assets/cards/14_A_diamonds.png"),
  "2♦": require("../../../assets/cards/15_2_diamonds.png"),
  "3♦": require("../../../assets/cards/16_3_diamonds.png"),
  "4♦": require("../../../assets/cards/17_4_diamonds.png"),
  "5♦": require("../../../assets/cards/18_5_diamonds.png"),
  "6♦": require("../../../assets/cards/19_6_diamonds.png"),
  "7♦": require("../../../assets/cards/20_7_diamonds.png"),
  "8♦": require("../../../assets/cards/21_8_diamonds.png"),
  "9♦": require("../../../assets/cards/22_9_diamonds.png"),
  "10♦": require("../../../assets/cards/23_10_diamonds.png"),
  "J♦": require("../../../assets/cards/24_J_diamonds.png"),
  "Q♦": require("../../../assets/cards/25_Q_diamonds.png"),
  "K♦": require("../../../assets/cards/26_K_diamonds.png"),

  "A♣": require("../../../assets/cards/27_A_clubs.png"),
  "2♣": require("../../../assets/cards/28_2_clubs.png"),
  "3♣": require("../../../assets/cards/29_3_clubs.png"),
  "4♣": require("../../../assets/cards/30_4_clubs.png"),
  "5♣": require("../../../assets/cards/31_5_clubs.png"),
  "6♣": require("../../../assets/cards/32_6_clubs.png"),
  "7♣": require("../../../assets/cards/33_7_clubs.png"),
  "8♣": require("../../../assets/cards/34_8_clubs.png"),
  "9♣": require("../../../assets/cards/35_9_clubs.png"),
  "10♣": require("../../../assets/cards/36_10_clubs.png"),
  "J♣": require("../../../assets/cards/37_J_clubs.png"),
  "Q♣": require("../../../assets/cards/38_Q_clubs.png"),
  "K♣": require("../../../assets/cards/39_K_clubs.png"),

  "A♥": require("../../../assets/cards/40_A_hearts.png"),
  "2♥": require("../../../assets/cards/41_2_hearts.png"),
  "3♥": require("../../../assets/cards/42_3_hearts.png"),
  "4♥": require("../../../assets/cards/43_4_hearts.png"),
  "5♥": require("../../../assets/cards/44_5_hearts.png"),
  "6♥": require("../../../assets/cards/45_6_hearts.png"),
  "7♥": require("../../../assets/cards/46_7_hearts.png"),
  "8♥": require("../../../assets/cards/47_8_hearts.png"),
  "9♥": require("../../../assets/cards/48_9_hearts.png"),
  "10♥": require("../../../assets/cards/49_10_hearts.png"),
  "J♥": require("../../../assets/cards/50_J_hearts.png"),
  "Q♥": require("../../../assets/cards/51_Q_hearts.png"),
  "K♥": require("../../../assets/cards/52_K_hearts.png"),
};

/**
 * Blue card back asset, used for face-down cards.
 */
export const CARD_BACK = require("../../../assets/cards/back_blue.png");

/**
 * Red card back asset, available as an alternative card back.
 */
export const CARD_BACK_RED = require("../../../assets/cards/back_red.png");

/**
 * Resolves the image asset for a given card code.
 *
 * Returns the card back when the card is unknown, empty, or "?".
 *
 * @param card - Card code (e.g. "A♠"), or "?" for face-down cards.
 * @returns The resolved image asset.
 */
export const getCardImage = (card: string): number => {
  if (!card || card === "?" || card === "??") {
    return CARD_BACK;
  }
  return CARD_IMAGES[card] ?? CARD_BACK;
};
