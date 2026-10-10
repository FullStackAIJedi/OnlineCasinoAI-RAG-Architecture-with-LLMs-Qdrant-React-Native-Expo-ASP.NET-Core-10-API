
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
];

/**
 * Numbers that pay out as "red" on a European roulette wheel.
 *
 * The complementary set (1–36 minus these) is "black". Zero is neither.
 */
export const RED_NUMBERS = new Set([
  1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36,
]);


// ROULETTE ====================

/**
 * Physical order of numbers on the roulette wheel (clockwise).
 *
 * Differs from the numeric order (0–36) because the wheel layout is
 * intentionally non-sequential. `999` is a sentinel for the "00" pocket
 * used on American-style wheels.
 *
 * Required for animating the wheel spin and highlighting the winning pocket.
 */
// export const WHEEL_ORDER = [
//   0, 2, 14, 35, 23, 4, 16, 33, 21, 6, 18, 31, 19, 8, 12, 29, 25, 10, 27,
//   999,  // 00
//   1, 13, 36, 24, 3, 15, 34, 22, 5, 17, 32, 20, 7, 11, 30, 26, 9, 28,
// ];

/**
 * Rotation applied to the roulette wheel image so that the "0" pocket
 * aligns with the pointer at the top.
 *
 * This value is specific to the wheel asset (`roulette-wheel.png`) and
 * must be adjusted if the image is replaced or re-cropped.
 */
//export const IMAGE_ROTATION = "28deg";

/**
 * Angular correction added to the calculated ball angle so the pointer
 * lands exactly on the winning number.
 *
 * The calculated angle comes from `WHEEL_ORDER` and the wheel geometry;
 * this offset compensates for small misalignments between the mathematical
 * model and the rendered image (e.g. pointer position, wheel crop).
 *
 * Keep at `0` unless the ball visually misses the winning pocket.
 * Positive values rotate clockwise, negative counter-clockwise (in degrees).
 */
//export const ANGLE_OFFSET = 0;

/**
 * Angular correction applied to the ball's final position so it lands
 * exactly on the winning number.
 *
 * The ball angle is calculated from `WHEEL_ORDER` and the wheel geometry;
 * this offset compensates for small misalignments between the mathematical
 * model and the rendered image (pointer position, wheel crop, asset quirks).
 *
 * Unlike `IMAGE_ROTATION` (which statically aligns the wheel image with the
 * pointer), this value corrects the *dynamic* spin result on every round.
 *
 * Keep at `0` unless the ball visually misses the winning pocket.
 * Positive values rotate clockwise, negative counter-clockwise (in degrees).
 */
//export const BALL_ANGLE_OFFSET = 0; 


