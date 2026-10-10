/**
 * Entry in the static game catalog shown on the home screen.
 */
export interface GameCatalogEntry {
  code: string;
  name: string;
  image: number;
  route: string;
}

/**
 * Static catalog of games shown on the home screen.
 *
 * Kept client-side because each entry carries a local asset and a
 * navigation route not present in the backend `Game` model.
 *
 * The `code` must match a backend game code for the game screen to work.
 */
export const GAMES: GameCatalogEntry[] = [
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
];

/**
 * Filter options for the game history screen.
 *
 * The empty `value` represents "no filter" (all games).
 */
export const GAME_OPTIONS = [
  { label: "All games", value: "" },
  { label: "Slots", value: "slots" },
  { label: "Roulette", value: "roulette" },
  { label: "Blackjack", value: "blackjack" },
] as const;
