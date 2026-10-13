import type { UserListItem } from "../../src/api/types";

export const makeUserListItem = (
  overrides: Partial<UserListItem> = {},
): UserListItem => ({
  id: 1,
  email: "test@test.com",
  nickName: "TestUser",
  pofileImage: "",
  role: "User",
  isBlocked: false,
  balance: 1000,
  ...overrides,
});
