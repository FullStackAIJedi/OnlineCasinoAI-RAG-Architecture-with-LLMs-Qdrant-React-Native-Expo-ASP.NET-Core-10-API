import { cleanup, fireEvent, waitFor } from "@testing-library/react-native";

import { blackjackApi } from "@/api/blackjack";
import { walletApi } from "@/api/wallet";
import BlackjackScreen from "@/app/games/blackjack";
import {
  makeBlackjackState,
  makeWallet,
  renderWithProviders,
} from "../../helpers";

jest.mock("@/api/blackjack");
jest.mock("@/api/wallet");

jest.mock("react-native-safe-area-context", () => {
  const React = require("react");
  const { View } = require("react-native");
  return {
    useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
    SafeAreaProvider: ({ children }: { children: React.ReactNode }) =>
      React.createElement(View, null, children),
    SafeAreaView: ({
      children,
      ...props
    }: {
      children: React.ReactNode;
      [key: string]: unknown;
    }) => React.createElement(View, props, children),
  };
});

afterEach(() => {
  cleanup();
  jest.resetAllMocks();
});

describe("BlackjackScreen", () => {
  beforeEach(() => {
    (walletApi.get as jest.Mock).mockResolvedValue(
      makeWallet({ balance: 500 }),
    );
  });

  it("renders balance", async () => {
    const { getByText } = await renderWithProviders(<BlackjackScreen />);
    await waitFor(() => expect(getByText(/500.00/)).toBeTruthy());
  });

  it("renders Start button", async () => {
    const { getByText } = await renderWithProviders(<BlackjackScreen />);
    await waitFor(() => expect(getByText("Start")).toBeTruthy());
  });

  it("shows invalid bet error", async () => {
    const { getByText, getByDisplayValue } = await renderWithProviders(
      <BlackjackScreen />,
    );

    await waitFor(() => getByText(/500.00/));

    await fireEvent.changeText(getByDisplayValue("10"), "abc");
    await fireEvent.press(getByText("Start"));

    await waitFor(() => expect(getByText("Invalid bet")).toBeTruthy());
  });

  it("shows min bet error", async () => {
    const { getByText, getByDisplayValue } = await renderWithProviders(
      <BlackjackScreen />,
    );

    await waitFor(() => getByText(/500.00/));

    await fireEvent.changeText(getByDisplayValue("10"), "1");
    await fireEvent.press(getByText("Start"));

    await waitFor(() => expect(getByText(/Minimum bet/)).toBeTruthy());
  });

  it("shows max bet error", async () => {
    const { getByText, getByDisplayValue } = await renderWithProviders(
      <BlackjackScreen />,
    );

    await waitFor(() => getByText(/500.00/));

    await fireEvent.changeText(getByDisplayValue("10"), "10000");
    await fireEvent.press(getByText("Start"));

    await waitFor(() => expect(getByText(/Maximum bet/)).toBeTruthy());
  });

  it("shows insufficient balance error", async () => {
    (walletApi.get as jest.Mock).mockResolvedValue(makeWallet({ balance: 5 }));

    const { getByText, getByDisplayValue } = await renderWithProviders(
      <BlackjackScreen />,
    );

    await waitFor(() => getByText(/5.00/));

    await fireEvent.changeText(getByDisplayValue("10"), "100");
    await fireEvent.press(getByText("Start"));

    await waitFor(() => expect(getByText("Insufficient balance")).toBeTruthy());
  });

  it("starts game and shows actions", async () => {
    (blackjackApi.start as jest.Mock).mockResolvedValue(
      makeBlackjackState({ status: "PlayerTurn" }),
    );

    const { getByText } = await renderWithProviders(<BlackjackScreen />);
    await waitFor(() => getByText(/500.00/));
    await fireEvent.press(getByText("Start"));

    await waitFor(() => {
      expect(getByText("Hit")).toBeTruthy();
      expect(getByText("Stand")).toBeTruthy();
      expect(getByText("Double")).toBeTruthy();
    });
  });

  it("hits when Hit pressed", async () => {
    (blackjackApi.start as jest.Mock).mockResolvedValue(
      makeBlackjackState({ status: "PlayerTurn" }),
    );
    (blackjackApi.hit as jest.Mock).mockResolvedValue(makeBlackjackState());

    const { getByText } = await renderWithProviders(<BlackjackScreen />);
    await waitFor(() => getByText(/500.00/));
    await fireEvent.press(getByText("Start"));
    await waitFor(() => getByText("Hit"));
    await fireEvent.press(getByText("Hit"));

    await waitFor(() => expect(blackjackApi.hit).toHaveBeenCalled());
  });

  it("shows result when win", async () => {
    (blackjackApi.start as jest.Mock).mockResolvedValue(
      makeBlackjackState({ status: "PlayerTurn" }),
    );
    (blackjackApi.stand as jest.Mock).mockResolvedValue(
      makeBlackjackState({
        status: "Finished",
        result: "Win",
        winAmount: 20,
      }),
    );

    const { getByText } = await renderWithProviders(<BlackjackScreen />);
    await waitFor(() => getByText(/500.00/));
    await fireEvent.press(getByText("Start"));
    await waitFor(() => getByText("Stand"));
    await fireEvent.press(getByText("Stand"));

    await waitFor(() => expect(getByText(/You won/)).toBeTruthy());
  });

  it("shows New game when finished", async () => {
    (blackjackApi.start as jest.Mock).mockResolvedValue(
      makeBlackjackState({ status: "Finished", result: "Loss" }),
    );

    const { getByText } = await renderWithProviders(<BlackjackScreen />);
    await waitFor(() => getByText(/500.00/));
    await fireEvent.press(getByText("Start"));

    await waitFor(() => expect(getByText("New game")).toBeTruthy());
  });
});
