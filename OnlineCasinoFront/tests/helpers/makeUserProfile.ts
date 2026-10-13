import type { UserProfile } from "../../src/api/types";

/**
 * Creates a mock `UserProfile` for tests.
 *
 * Defaults represent a fully-populated profile. Pass `overrides` to
 * customize individual fields for a specific test.
 *
 * @param overrides - Partial fields to override on the default profile.
 * @returns A `UserProfile` object.
 *
 * @example
 * const profile = makeUserProfile({ nickName: "Alice", role: "Admin" });
 */
export const makeUserProfile = (
  overrides: Partial<UserProfile> = {},
): UserProfile => ({
  id: 1,
  email: "test@test.com",
  pofileImage: "",
  nickName: "TestUser",
  firstName: "Test",
  lastName: "User",
  role: "User",
  createdAt: new Date("2024-01-01T00:00:00Z").toISOString(),
  ...overrides,
});
