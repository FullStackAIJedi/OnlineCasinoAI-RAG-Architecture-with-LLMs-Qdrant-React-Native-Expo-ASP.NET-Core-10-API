import { fireEvent, waitFor } from "@testing-library/react-native";

import { walletApi } from "@/api/wallet";
import HomeScreen from "@/app/(tabs)/index";
import { makeWallet, renderWithProviders } from "../../helpers";

jest.mock("@/api/wallet");

const mockPush = jest.fn();
jest.mock("expo-router", () => ({
  useRouter: () => ({ push: mockPush }),
}));

jest.mock("react-native-safe-area-context", () => {
  const React = require("react");
  const { View } = require("react-native");
  return {
    useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
    SafeAreaProvider: ({ children }: any) =>
      React.createElement(View, null, children),
    SafeAreaView: ({ children, ...props }: any) =>
      React.createElement(View, props, children),
  };
});

describe("HomeScreen", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (walletApi.get as jest.Mock).mockResolvedValue(
      makeWallet({ balance: 500 }),
    );
  });

  it("renders without crashing", async () => {
    const { toJSON } = await renderWithProviders(<HomeScreen />);
    expect(toJSON()).toBeTruthy();
  });

  it("shows wallet balance", async () => {
    const { getByText } = await renderWithProviders(<HomeScreen />);
    await waitFor(() => expect(getByText(/500.00/)).toBeTruthy());
  });

  it("shows currency", async () => {
    const { getByText } = await renderWithProviders(<HomeScreen />);
    await waitFor(() => expect(getByText(/EUR/)).toBeTruthy());
  });

  it("renders all game cards", async () => {
    const { getByText } = await renderWithProviders(<HomeScreen />);
    await waitFor(() => {
      expect(getByText("3 Reel Slots")).toBeTruthy();
      expect(getByText("4 Reel Slots")).toBeTruthy();
      expect(getByText("Roulette")).toBeTruthy();
      expect(getByText("Blackjack")).toBeTruthy();
    });
  });

  it("navigates on card press", async () => {
    const { getByText } = await renderWithProviders(<HomeScreen />);
    await waitFor(() => getByText("Roulette"));
    fireEvent.press(getByText("Roulette"));
    expect(mockPush).toHaveBeenCalledWith("/games/roulette");
  });

  it("shows fallback when wallet loading", async () => {
    (walletApi.get as jest.Mock).mockResolvedValue({
      balance: 0,
      currency: "EUR",
    });
    const { getByText } = await renderWithProviders(<HomeScreen />);
    await waitFor(() => expect(getByText(/0.00/)).toBeTruthy());
  });
});
