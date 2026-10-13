import type { AuthUser } from "../../src/api/types";

export const makeAuthUser = (overrides: Partial<AuthUser> = {}): AuthUser => ({
  id: 1,
  email: "test@test.com",
  nickname: "TestUser",
  role: "User",
  ...overrides,
});
