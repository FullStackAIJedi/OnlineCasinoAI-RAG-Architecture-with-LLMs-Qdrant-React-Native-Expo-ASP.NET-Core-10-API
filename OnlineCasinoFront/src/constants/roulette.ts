// /**
//  * Sentinel value representing the "00" pocket on the roulette wheel.
//  *
//  * The backend uses 37 for "00" in API responses; the wheel uses 999
//  * internally so it cannot collide with any real number (0-36).
//  */
// export const ZERO_ZERO_SENTINEL = 999;

// /**
//  * The "00" pocket as sent by the backend.
//  */
// export const ZERO_ZERO_API = 37;

// /**
//  * Numbers that pay out as "red" on a European roulette wheel.
//  *
//  * The complementary set (1-36 minus these) is "black". Zero is neither.
//  */
// export const RED_NUMBERS = new Set([
//   1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36,
// ]);

// /**
//  * Physical order of numbers on the roulette wheel (clockwise).
//  *
//  * Differs from the numeric order (0-36) because the wheel layout is
//  * intentionally non-sequential.
//  */
// export const WHEEL_ORDER = [
//   0, 2, 14, 35, 23, 4, 16, 33, 21, 6, 18, 31, 19, 8, 12, 29, 25, 10, 27,
//   ZERO_ZERO_SENTINEL,
//   1, 13, 36, 24, 3, 15, 34, 22, 5, 17, 32, 20, 7, 11, 30, 26, 9, 28,
// ];

// /**
//  * Rotation applied to the roulette wheel image so the "0" pocket
//  * aligns with the pointer at the top.
//  *
//  * Specific to the wheel asset; must be adjusted if the image changes.
//  */
// export const IMAGE_ROTATION = "28deg";

// /**
//  * Angular correction added to the calculated wheel angle.
//  *
//  * Compensates for small misalignments between the mathematical model
//  * and the rendered image. Keep at 0 unless the ball visually misses.
//  */
// export const ANGLE_OFFSET = 0;

// /**
//  * Angular correction applied to the ball's final position.
//  *
//  * Corrects the dynamic spin result on every round. Keep at 0 unless
//  * the ball visually misses the winning pocket.
//  */
// export const BALL_ANGLE_OFFSET = 0;
