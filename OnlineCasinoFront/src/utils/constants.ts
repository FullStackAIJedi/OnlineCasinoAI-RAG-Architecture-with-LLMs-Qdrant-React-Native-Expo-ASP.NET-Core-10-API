/**
 * Filter options for the game history screen.
 *
 * The empty `value` represents "no filter" (all games) and is used
 * directly as the `game` query parameter.
 */
export const GAME_OPTIONS = [
  { label: "All games", value: "" },
  { label: "Slots", value: "slots" },
  { label: "Roulette", value: "roulette" },
  { label: "Blackjack", value: "blackjack" },
];

/**
 * Static catalog of games shown on the home screen.
 *
 * Kept client-side (not fetched from `/api/games`) because each entry also
 * carries a local asset (`image`) and a navigation `route` that are not
 * part of the backend `Game` model.
 *
 * Note: the `code` must match a backend game code for the game screen to work.
 */
export const GAMES = [
  {
    code: "slots",
    name: "3 Reel Slots",
    image: require("../../../OnlineCasinoFront/assets/gameCard/1.jpg"),
    route: "/games/slots?code=slots",
  },
  {
    code: "slots4",
    name: "4 Reel Slots",
    image: require("../../../OnlineCasinoFront/assets/gameCard/2.jpg"),
    route: "/games/slots?code=slots4",
  },
  {
    code: "roulette",
    name: "Roulette",
    image: require("../../../OnlineCasinoFront/assets/gameCard/3.jpg"),
    route: "/games/roulette",
  },
  {
    code: "blackjack",
    name: "Blackjack",
    image: require("../../../OnlineCasinoFront/assets/gameCard/4.jpg"),
    route: "/games/blackjack",
  },
] as const;

/**
 * Numbers that pay out as "red" on a European roulette wheel.
 *
 * The complementary set (1–36 minus these) is "black". Zero is neither.
 */
export const RED_NUMBERS = new Set([
  1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36,
]);
