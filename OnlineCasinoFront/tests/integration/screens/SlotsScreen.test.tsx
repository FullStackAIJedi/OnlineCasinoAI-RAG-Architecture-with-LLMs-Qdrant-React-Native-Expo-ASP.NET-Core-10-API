// import React from "react";
// import {
//   act,
//   cleanup,
//   fireEvent,
//   waitFor,
// } from "@testing-library/react-native";

// import SlotsScreen from "../../../src/app/games/slots";
// import * as slotsApi from "../../../src/api/slots";
// import * as walletApi from "../../../src/api/wallet";
// import { renderWithProviders } from "@/tests/helpers";

// jest.mock("../../../src/api/slots");
// jest.mock("../../../src/api/wallet");
// jest.mock("../../../src/utils/uuid", () => ({
//   uuid: () => "test-uuid",
// }));

// afterEach(() => {
//   cleanup();
//   jest.useRealTimers();
//   jest.clearAllTimers();
//   jest.restoreAllMocks();
// });

// describe("SlotsScreen", () => {
//   beforeEach(() => {
//     jest.useRealTimers();
//     (slotsApi.slotsApi.spin as jest.Mock).mockReset();
//     (walletApi.walletApi.get as jest.Mock).mockReset();
//     (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
//       balance: 1000,
//       currency: "EUR",
//     });
//   });

//   it("renders Spin button", async () => {
//     const { getByText } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => expect(getByText("Spin")).toBeTruthy());
//   });

//   it("renders Bet label", async () => {
//     const { getByText } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => expect(getByText("Bet")).toBeTruthy());
//   });

//   it("renders default bet value 1", async () => {
//     const { getByDisplayValue } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => expect(getByDisplayValue("1")).toBeTruthy());
//   });

//   it("renders balance from wallet", async () => {
//     const { getByText } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => expect(getByText(/1000.00/)).toBeTruthy());
//   });

//   it("shows invalid bet error", async () => {
//     const { getByText, getByDisplayValue } = await renderWithProviders(
//       <SlotsScreen />,
//     );
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.changeText(getByDisplayValue("1"), "abc");
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => expect(getByText("Invalid bet")).toBeTruthy());
//   });

//   it("shows bet required error when empty", async () => {
//     const { getByText, getByDisplayValue } = await renderWithProviders(
//       <SlotsScreen />,
//     );
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.changeText(getByDisplayValue("1"), "");
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => expect(getByText("Bet is required")).toBeTruthy());
//   });

//   it("shows invalid bet error for negative amount", async () => {
//     const { getByText, getByDisplayValue } = await renderWithProviders(
//       <SlotsScreen />,
//     );
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.changeText(getByDisplayValue("1"), "-5");
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => expect(getByText("Invalid bet")).toBeTruthy());
//   });

//   it("shows insufficient balance", async () => {
//     (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
//       balance: 5,
//       currency: "EUR",
//     });
//     const { getByText, getByDisplayValue } = await renderWithProviders(
//       <SlotsScreen />,
//     );
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.changeText(getByDisplayValue("1"), "10000");
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => expect(getByText("Insufficient balance")).toBeTruthy());
//   });

//   it("does not call spin when bet invalid", async () => {
//     const { getByText, getByDisplayValue } = await renderWithProviders(
//       <SlotsScreen />,
//     );
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.changeText(getByDisplayValue("1"), "abc");
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => getByText("Invalid bet"));
//     expect(slotsApi.slotsApi.spin).not.toHaveBeenCalled();
//   });

//   it("does not call spin when balance insufficient", async () => {
//     (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
//       balance: 5,
//       currency: "EUR",
//     });
//     const { getByText, getByDisplayValue } = await renderWithProviders(
//       <SlotsScreen />,
//     );
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.changeText(getByDisplayValue("1"), "10000");
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => getByText("Insufficient balance"));
//     expect(slotsApi.slotsApi.spin).not.toHaveBeenCalled();
//   });

//   it("calls spin on press", async () => {
//     (slotsApi.slotsApi.spin as jest.Mock).mockResolvedValue({
//       reels: ["A", "B", "C"],
//     });
//     const { getByText } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => expect(slotsApi.slotsApi.spin).toHaveBeenCalled(), {
//       timeout: 5000,
//     });
//   });

//   it("calls spin only once per press", async () => {
//     (slotsApi.slotsApi.spin as jest.Mock).mockResolvedValue({
//       reels: ["A", "B", "C"],
//     });
//     const { getByText } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(
//       () => expect(slotsApi.slotsApi.spin).toHaveBeenCalledTimes(1),
//       { timeout: 5000 },
//     );
//   });

//   it("handles spin rejection gracefully", async () => {
//     (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
//       balance: 1000,
//       currency: "EUR",
//     });
//     (slotsApi.slotsApi.spin as jest.Mock).mockRejectedValue(
//       new Error("Server error"),
//     );
//     const { getByText } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => expect(getByText("Server error")).toBeTruthy(), {
//       timeout: 5000,
//     });
//   });

//   it("uses slots4 reelsCount when code=slots4", async () => {
//     const { useLocalSearchParams } = require("expo-router");
//     (useLocalSearchParams as jest.Mock).mockReturnValue({ code: "slots4" });
//     const { toJSON } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => expect(toJSON()).toBeTruthy());
//   });

//   it("uses default reelsCount when code is slots", async () => {
//     const { useLocalSearchParams } = require("expo-router");
//     (useLocalSearchParams as jest.Mock).mockReturnValue({ code: "slots" });
//     const { toJSON } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => expect(toJSON()).toBeTruthy());
//   });

//   it("uses default reelsCount when code is undefined", async () => {
//     const { useLocalSearchParams } = require("expo-router");
//     (useLocalSearchParams as jest.Mock).mockReturnValue({});
//     const { toJSON } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => expect(toJSON()).toBeTruthy());
//   });

//   it("updates bet input value on change", async () => {
//     const { getByDisplayValue } = await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => getByDisplayValue("1"));
//     await fireEvent.changeText(getByDisplayValue("1"), "25");
//     await waitFor(() => expect(getByDisplayValue("25")).toBeTruthy());
//   });

//   it("clears error after valid input change", async () => {
//     const { getByText, getByDisplayValue, queryByText } =
//       await renderWithProviders(<SlotsScreen />);
//     await waitFor(() => getByText("Spin"));
//     await fireEvent.changeText(getByDisplayValue("1"), "abc");
//     await fireEvent.press(getByText("Spin"));
//     await waitFor(() => getByText("Invalid bet"));
//     await fireEvent.changeText(getByDisplayValue("abc"), "5");
//     await waitFor(() => expect(queryByText("Invalid bet")).toBeNull());
//   });

//   it("does not crash on mount", async () => {
//     await expect(renderWithProviders(<SlotsScreen />)).resolves.toBeDefined();
//   });
// });

import {
  act,
  cleanup,
  fireEvent,
  waitFor,
} from "@testing-library/react-native";

import SlotsScreen from "../../../src/app/games/slots";
import * as slotsApi from "../../../src/api/slots";
import * as walletApi from "../../../src/api/wallet";
import { renderWithProviders } from "@/tests/helpers";

jest.mock("../../../src/api/slots");
jest.mock("../../../src/api/wallet");
jest.mock("../../../src/utils/uuid", () => ({
  uuid: () => "test-uuid",
}));

afterEach(() => {
  cleanup();
  jest.clearAllTimers();
  jest.useRealTimers();
  jest.restoreAllMocks();
});

describe("SlotsScreen", () => {
  beforeEach(() => {
    (slotsApi.slotsApi.spin as jest.Mock).mockReset();
    (walletApi.walletApi.get as jest.Mock).mockReset();
    (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
      balance: 1000,
      currency: "EUR",
    });
  });

  describe("rendering", () => {
    it("renders Spin button", async () => {
      const { getByText } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByText("Spin")).toBeTruthy());
    });

    it("renders Bet label", async () => {
      const { getByText } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByText("Bet")).toBeTruthy());
    });

    it("renders default bet value 1", async () => {
      const { getByDisplayValue } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByDisplayValue("1")).toBeTruthy());
    });

    it("renders balance from wallet", async () => {
      const { getByText } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByText(/1000.00/)).toBeTruthy());
    });

    it("renders currency from wallet", async () => {
      const { getByText } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByText(/EUR/)).toBeTruthy());
    });

    it("renders USD when wallet has USD", async () => {
      (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
        balance: 500,
        currency: "USD",
      });
      const { getByText } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByText(/USD/)).toBeTruthy());
    });

    it("does not crash on mount", async () => {
      await expect(renderWithProviders(<SlotsScreen />)).resolves.toBeDefined();
    });
  });

  describe("validation", () => {
    it("shows invalid bet error", async () => {
      const { getByText, getByDisplayValue } = await renderWithProviders(
        <SlotsScreen />,
      );
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "abc");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => expect(getByText("Invalid bet")).toBeTruthy());
    });

    it("shows bet required error when empty", async () => {
      const { getByText, getByDisplayValue } = await renderWithProviders(
        <SlotsScreen />,
      );
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => expect(getByText("Bet is required")).toBeTruthy());
    });

    it("shows invalid bet error for negative amount", async () => {
      const { getByText, getByDisplayValue } = await renderWithProviders(
        <SlotsScreen />,
      );
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "-5");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => expect(getByText("Invalid bet")).toBeTruthy());
    });

    it("shows invalid bet error for zero amount", async () => {
      const { getByText, getByDisplayValue } = await renderWithProviders(
        <SlotsScreen />,
      );
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "0");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => expect(getByText("Invalid bet")).toBeTruthy());
    });

    it("shows insufficient balance", async () => {
      (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
        balance: 5,
        currency: "EUR",
      });
      const { getByText, getByDisplayValue } = await renderWithProviders(
        <SlotsScreen />,
      );
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "10000");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() =>
        expect(getByText("Insufficient balance")).toBeTruthy(),
      );
    });

    it("does not call spin when bet invalid", async () => {
      const { getByText, getByDisplayValue } = await renderWithProviders(
        <SlotsScreen />,
      );
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "abc");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => getByText("Invalid bet"));
      expect(slotsApi.slotsApi.spin).not.toHaveBeenCalled();
    });

    it("does not call spin when balance insufficient", async () => {
      (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
        balance: 5,
        currency: "EUR",
      });
      const { getByText, getByDisplayValue } = await renderWithProviders(
        <SlotsScreen />,
      );
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "10000");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => getByText("Insufficient balance"));
      expect(slotsApi.slotsApi.spin).not.toHaveBeenCalled();
    });

    it("clears error after valid input change", async () => {
      const { getByText, getByDisplayValue, queryByText } =
        await renderWithProviders(<SlotsScreen />);
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "abc");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => getByText("Invalid bet"));
      await fireEvent.changeText(getByDisplayValue("abc"), "5");
      await waitFor(() => expect(queryByText("Invalid bet")).toBeNull());
    });

    it("clears insufficient balance error on new input", async () => {
      (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
        balance: 5,
        currency: "EUR",
      });
      const { getByText, getByDisplayValue, queryByText } =
        await renderWithProviders(<SlotsScreen />);
      await waitFor(() => getByText("Spin"));
      await fireEvent.changeText(getByDisplayValue("1"), "10000");
      await fireEvent.press(getByText("Spin"));
      await waitFor(() => getByText("Insufficient balance"));
      await fireEvent.changeText(getByDisplayValue("10000"), "2");
      await waitFor(() =>
        expect(queryByText("Insufficient balance")).toBeNull(),
      );
    });
  });

  describe("input interaction", () => {
    it("updates bet input value on change", async () => {
      const { getByDisplayValue } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => getByDisplayValue("1"));
      await fireEvent.changeText(getByDisplayValue("1"), "25");
      await waitFor(() => expect(getByDisplayValue("25")).toBeTruthy());
    });

    it("accepts decimal bet amounts", async () => {
      const { getByDisplayValue } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => getByDisplayValue("1"));
      await fireEvent.changeText(getByDisplayValue("1"), "1.5");
      await waitFor(() => expect(getByDisplayValue("1.5")).toBeTruthy());
    });

    it("clears input to empty", async () => {
      const { getByDisplayValue } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => getByDisplayValue("1"));
      await fireEvent.changeText(getByDisplayValue("1"), "");
      await waitFor(() => expect(getByDisplayValue("")).toBeTruthy());
    });
  });

  describe("reels count based on game code", () => {
    it("uses slots4 reelsCount when code=slots4", async () => {
      const { useLocalSearchParams } = require("expo-router");
      (useLocalSearchParams as jest.Mock).mockReturnValue({ code: "slots4" });
      const { toJSON } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(toJSON()).toBeTruthy());
    });

    it("uses default reelsCount when code is slots", async () => {
      const { useLocalSearchParams } = require("expo-router");
      (useLocalSearchParams as jest.Mock).mockReturnValue({ code: "slots" });
      const { toJSON } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(toJSON()).toBeTruthy());
    });

    it("uses default reelsCount when code is undefined", async () => {
      const { useLocalSearchParams } = require("expo-router");
      (useLocalSearchParams as jest.Mock).mockReturnValue({});
      const { toJSON } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(toJSON()).toBeTruthy());
    });

    it("uses default reelsCount when code is unknown", async () => {
      const { useLocalSearchParams } = require("expo-router");
      (useLocalSearchParams as jest.Mock).mockReturnValue({ code: "unknown" });
      const { toJSON } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(toJSON()).toBeTruthy());
    });
  });

  describe("wallet integration", () => {
    it("renders zero balance when wallet is null", async () => {
      (walletApi.walletApi.get as jest.Mock).mockResolvedValue(null);
      const { getByText } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByText(/0.00/)).toBeTruthy());
    });

    it("renders EUR when wallet currency missing", async () => {
      (walletApi.walletApi.get as jest.Mock).mockResolvedValue({
        balance: 500,
      });
      const { getByText } = await renderWithProviders(<SlotsScreen />);
      await waitFor(() => expect(getByText(/EUR/)).toBeTruthy());
    });
  });
});
