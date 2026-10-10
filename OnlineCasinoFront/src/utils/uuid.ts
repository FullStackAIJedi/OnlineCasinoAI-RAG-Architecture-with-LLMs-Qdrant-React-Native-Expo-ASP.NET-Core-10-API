/**
 * Generates a UUID v4-like identifier without external dependencies.
 *
 * Suitable for client-side identifiers such as `ClientRoundId` used in bets.
 *
 * Note: relies on `Math.random()`, so it is NOT cryptographically secure.
 * Do not use it for security-sensitive values (tokens, secrets, nonces).
 * For those, use `expo-crypto`'s `randomUUID()` or `crypto.randomUUID()`.
 *
 * @returns A 36-character UUID v4-formatted string (e.g. `"f47ac10b-58cc-4372-a567-0e02b2c3d479"`).
 */
export function uuid(): string {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}
