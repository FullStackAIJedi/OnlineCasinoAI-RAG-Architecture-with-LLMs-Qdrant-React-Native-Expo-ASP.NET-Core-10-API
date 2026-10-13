import { cleanup, fireEvent, waitFor } from "@testing-library/react-native";

import { walletApi } from "@/api/wallet";
import WalletScreen from "@/app/(tabs)/wallet";
import {
  makePagedResult,
  makeTransaction,
  makeWallet,
  renderWithProviders,
} from "../../helpers";

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
  jest.useRealTimers();
});

describe("WalletScreen", () => {
  beforeEach(() => {
    (walletApi.get as jest.Mock).mockReset();
    (walletApi.getTransactions as jest.Mock).mockReset();
    (walletApi.deposit as jest.Mock).mockReset();
    (walletApi.withdraw as jest.Mock).mockReset();

    (walletApi.get as jest.Mock).mockResolvedValue(makeWallet());
    (walletApi.getTransactions as jest.Mock).mockResolvedValue(
      makePagedResult(),
    );
  });

  it("renders balance", async () => {
    const { getByText } = await renderWithProviders(<WalletScreen />);
    await waitFor(() => expect(getByText(/1000.00/)).toBeTruthy());
  });

  it("renders Deposit button", async () => {
    const { getByText } = await renderWithProviders(<WalletScreen />);
    await waitFor(() => expect(getByText("Deposit")).toBeTruthy());
  });

  it("renders Withdrawal button", async () => {
    const { getByText } = await renderWithProviders(<WalletScreen />);
    await waitFor(() => expect(getByText("Withdrawal")).toBeTruthy());
  });

  it("shows transactions", async () => {
    (walletApi.getTransactions as jest.Mock).mockResolvedValue(
      makePagedResult([makeTransaction({ type: "deposit", amount: 100 })]),
    );
    const { getByText } = await renderWithProviders(<WalletScreen />);
    await waitFor(() => expect(getByText("deposit")).toBeTruthy());
  });

  it("shows empty state", async () => {
    const { getByText } = await renderWithProviders(<WalletScreen />);
    await waitFor(() => expect(getByText("No transactions")).toBeTruthy());
  });

  it("opens deposit modal", async () => {
    const { getByText, getByPlaceholderText } = await renderWithProviders(
      <WalletScreen />,
    );
    await waitFor(() => getByText("Deposit"));
    await fireEvent.press(getByText("Deposit"));
    await waitFor(() => expect(getByPlaceholderText("10.00")).toBeTruthy());
  });

  it("deposits on OK", async () => {
    (walletApi.deposit as jest.Mock).mockResolvedValue(
      makeWallet({ balance: 1100 }),
    );
    const { getByText, getByPlaceholderText } = await renderWithProviders(
      <WalletScreen />,
    );
    await waitFor(() => getByText("Deposit"));
    await fireEvent.press(getByText("Deposit"));
    await waitFor(() => getByPlaceholderText("10.00"));
    await fireEvent.changeText(getByPlaceholderText("10.00"), "100");
    await fireEvent.press(getByText("ОК"));
    await waitFor(() =>
      expect((walletApi.deposit as jest.Mock).mock.calls[0][0]).toBe(100),
    );
  });

  it("shows invalid amount error", async () => {
    const { getByText, getByPlaceholderText } = await renderWithProviders(
      <WalletScreen />,
    );
    await waitFor(() => getByText("Deposit"));
    await fireEvent.press(getByText("Deposit"));
    await waitFor(() => getByPlaceholderText("10.00"));
    await fireEvent.changeText(getByPlaceholderText("10.00"), "abc");
    await fireEvent.press(getByText("ОК"));
    await waitFor(() => expect(getByText("Invalid amount")).toBeTruthy());
  });

  it("shows negative amount error", async () => {
    const { getByText, getByPlaceholderText } = await renderWithProviders(
      <WalletScreen />,
    );
    await waitFor(() => getByText("Deposit"));
    await fireEvent.press(getByText("Deposit"));
    await waitFor(() => getByPlaceholderText("10.00"));
    await fireEvent.changeText(getByPlaceholderText("10.00"), "-5");
    await fireEvent.press(getByText("ОК"));
    await waitFor(() => expect(getByText("Invalid amount")).toBeTruthy());
  });

  it("shows success on deposit", async () => {
    (walletApi.deposit as jest.Mock).mockResolvedValue(makeWallet());
    const { getByText, getByPlaceholderText } = await renderWithProviders(
      <WalletScreen />,
    );
    await waitFor(() => getByText("Deposit"));
    await fireEvent.press(getByText("Deposit"));
    await waitFor(() => getByPlaceholderText("10.00"));
    await fireEvent.changeText(getByPlaceholderText("10.00"), "100");
    await fireEvent.press(getByText("ОК"));
    await waitFor(() => expect(getByText("Deposit successful")).toBeTruthy());
  });
});
