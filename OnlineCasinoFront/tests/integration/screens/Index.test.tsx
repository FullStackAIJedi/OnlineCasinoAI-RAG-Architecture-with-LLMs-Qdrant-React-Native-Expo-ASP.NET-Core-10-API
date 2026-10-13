import { waitFor } from "@testing-library/react-native";

import Index from "../../../src/app/index";
import * as authHook from "../../../src/hooks/useAuth";
import { renderWithProviders } from "../../helpers";

const mockRedirect = jest.fn();
jest.mock("expo-router", () => ({
  Redirect: ({ href }: { href: string }) => {
    mockRedirect(href);
    return null;
  },
}));

describe("Index", () => {
  beforeEach(() => jest.clearAllMocks());

  it("redirects to tabs when authenticated", async () => {
    jest.spyOn(authHook, "useIsAuthenticated").mockReturnValue({
      data: true,
      isPending: false,
    } as never);

    await renderWithProviders(<Index />);

    await waitFor(() => expect(mockRedirect).toHaveBeenCalledWith("/(tabs)"));
  });

  it("redirects to login when not authenticated", async () => {
    jest.spyOn(authHook, "useIsAuthenticated").mockReturnValue({
      data: false,
      isPending: false,
    } as never);

    await renderWithProviders(<Index />);

    await waitFor(() => expect(mockRedirect).toHaveBeenCalledWith("/login"));
  });

  it("renders nothing while pending", async () => {
    jest.spyOn(authHook, "useIsAuthenticated").mockReturnValue({
      data: undefined,
      isPending: true,
    } as never);

    await renderWithProviders(<Index />);

    expect(mockRedirect).not.toHaveBeenCalled();
  });
});
