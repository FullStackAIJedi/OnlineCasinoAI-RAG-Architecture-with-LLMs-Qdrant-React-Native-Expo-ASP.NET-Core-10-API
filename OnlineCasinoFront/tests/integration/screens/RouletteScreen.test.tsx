import { cleanup, fireEvent, waitFor } from "@testing-library/react-native";

import { rouletteApi } from "@/api/roulette";
import { walletApi } from "@/api/wallet";
import RouletteScreen from "@/app/games/roulette";
import { makeCurrentBet, makeWallet, renderWithProviders } from "../../helpers";

jest.mock("@/api/roulette");
jest.mock("@/api/wallet");

afterEach(() => {
  cleanup();
  jest.resetAllMocks();
});

describe("RouletteScreen", () => {
  beforeEach(() => {
    (walletApi.get as jest.Mock).mockResolvedValue(
      makeWallet({ balance: 500 }),
    );
    (rouletteApi.getBets as jest.Mock).mockResolvedValue([]);
  });

  it("renders balance", async () => {
    const { getByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => expect(getByText(/500.00/)).toBeTruthy());
  });

  it("renders Place a bet section", async () => {
    const { getByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => expect(getByText("Place a bet")).toBeTruthy());
  });

  it("renders Place Bet button", async () => {
    const { getByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => expect(getByText("Place Bet")).toBeTruthy());
  });

  it("shows current bets count", async () => {
    const { getByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => expect(getByText(/Current bets 0/)).toBeTruthy());
  });

  it("shows existing bets", async () => {
    (rouletteApi.getBets as jest.Mock).mockResolvedValue([
      makeCurrentBet({ id: 1, type: "red", amount: 10 }),
    ]);
    const { getAllByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => {
      expect(getAllByText("Red").length).toBeGreaterThan(0);
    });
  });

  it("shows spin button", async () => {
    const { getByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => expect(getByText("Spin the Wheel")).toBeTruthy());
  });

  it("place bet validates amount", async () => {
    const { getByText, getByDisplayValue } = await renderWithProviders(
      <RouletteScreen />,
    );
    await waitFor(() => getByText(/500.00/));
    await fireEvent.changeText(getByDisplayValue("1"), "abc");
    await fireEvent.press(getByText("Place Bet"));
    await waitFor(() => expect(getByText("Invalid amount")).toBeTruthy());
  });

  it("calls placeBet on success", async () => {
    (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
    const { getByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => getByText(/500.00/));
    await fireEvent.press(getByText("Place Bet"));
    await waitFor(() => expect(rouletteApi.placeBet).toHaveBeenCalled());
  });

  it("shows error when spinning with no bets", async () => {
    const { getByText } = await renderWithProviders(<RouletteScreen />);
    await waitFor(() => getByText(/500.00/));
    await fireEvent.press(getByText("Spin the Wheel"));
    await waitFor(() =>
      expect(getByText("Place at least one bet before spinning")).toBeTruthy(),
    );
  });
});
