// import { fireEvent, waitFor } from "@testing-library/react-native";

// import { historyApi } from "@/api/history";
// import HistoryScreen from "@/app/(tabs)/history";
// import {
//   makeGameRound,
//   makePagedResult,
//   renderWithProviders,
// } from "../../helpers";

// jest.mock("@/api/history");

// jest.mock("react-native-safe-area-context", () => {
//   const React = require("react");
//   const { View } = require("react-native");
//   return {
//     useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
//     SafeAreaProvider: ({ children }: any) =>
//       React.createElement(View, null, children),
//     SafeAreaView: ({ children, ...props }: any) =>
//       React.createElement(View, props, children),
//   };
// });

// describe("HistoryScreen", () => {
//   beforeEach(() => {
//     jest.clearAllMocks();
//     (historyApi.getAll as jest.Mock).mockResolvedValue(makePagedResult());
//   });

//   it("renders empty state", async () => {
//     const { getByText } = await renderWithProviders(<HistoryScreen />);
//     await waitFor(() => expect(getByText("No rounds played yet")).toBeTruthy());
//   });

//   it("renders rounds", async () => {
//     (historyApi.getAll as jest.Mock).mockResolvedValue(
//       makePagedResult([
//         makeGameRound({ roundId: "r1", game: "slots", result: "Win" }),
//         makeGameRound({ roundId: "r2", game: "roulette", result: "Loss" }),
//       ]),
//     );
//     const { getByText } = await renderWithProviders(<HistoryScreen />);
//     await waitFor(() => {
//       expect(getByText("SLOTS")).toBeTruthy();
//       expect(getByText("ROULETTE")).toBeTruthy();
//     });
//   });

//   it("shows Win badge", async () => {
//     (historyApi.getAll as jest.Mock).mockResolvedValue(
//       makePagedResult([makeGameRound({ result: "Win" })]),
//     );
//     const { getByText } = await renderWithProviders(<HistoryScreen />);
//     await waitFor(() => expect(getByText("Win")).toBeTruthy());
//   });

//   it("shows Push badge", async () => {
//     (historyApi.getAll as jest.Mock).mockResolvedValue(
//       makePagedResult([makeGameRound({ result: "Push" })]),
//     );
//     const { getByText } = await renderWithProviders(<HistoryScreen />);
//     await waitFor(() => expect(getByText("Push")).toBeTruthy());
//   });

//   it("opens dropdown", async () => {
//     const { getByText, getAllByText } = await renderWithProviders(
//       <HistoryScreen />,
//     );
//     await waitFor(() => expect(getByText("Game")).toBeTruthy());
//     fireEvent.press(getByText("All games"));
//     await waitFor(() => {
//       expect(getAllByText("Slots").length).toBeGreaterThan(0);
//     });
//   });

//   it("filters by game", async () => {
//     const { getByText, getAllByText } = await renderWithProviders(
//       <HistoryScreen />,
//     );
//     await waitFor(() => getByText("All games"));
//     fireEvent.press(getByText("All games"));
//     const slotsItems = getAllByText("Slots");
//     fireEvent.press(slotsItems[slotsItems.length - 1]);
//     await waitFor(() =>
//       expect(historyApi.getAll).toHaveBeenCalledWith("slots", 1, 20),
//     );
//   });
// });

import { cleanup, fireEvent, waitFor } from "@testing-library/react-native";

import { historyApi } from "@/api/history";
import HistoryScreen from "@/app/(tabs)/history";
import {
  makeGameRound,
  makePagedResult,
  renderWithProviders,
} from "../../helpers";

jest.mock("@/api/history");

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

const mockGetAll = historyApi.getAll as jest.Mock;

afterEach(() => {
  cleanup();
  jest.clearAllMocks();
});

describe("HistoryScreen", () => {
  beforeEach(() => {
    mockGetAll.mockResolvedValue(makePagedResult());
  });

  it("renders empty state", async () => {
    const { getByText } = await renderWithProviders(<HistoryScreen />);
    await waitFor(() => expect(getByText("No rounds played yet")).toBeTruthy());
  });

  it("renders rounds", async () => {
    mockGetAll.mockResolvedValue(
      makePagedResult([
        makeGameRound({ roundId: "r1", game: "slots", result: "Win" }),
        makeGameRound({ roundId: "r2", game: "roulette", result: "Loss" }),
      ]),
    );
    const { getByText } = await renderWithProviders(<HistoryScreen />);
    await waitFor(() => {
      expect(getByText("SLOTS")).toBeTruthy();
      expect(getByText("ROULETTE")).toBeTruthy();
    });
  });

  it("shows Win badge", async () => {
    mockGetAll.mockResolvedValue(
      makePagedResult([makeGameRound({ result: "Win" })]),
    );
    const { getByText } = await renderWithProviders(<HistoryScreen />);
    await waitFor(() => expect(getByText("Win")).toBeTruthy());
  });

  it("shows Push badge", async () => {
    mockGetAll.mockResolvedValue(
      makePagedResult([makeGameRound({ result: "Push" })]),
    );
    const { getByText } = await renderWithProviders(<HistoryScreen />);
    await waitFor(() => expect(getByText("Push")).toBeTruthy());
  });

  it("opens dropdown", async () => {
    const { getByText, getAllByText } = await renderWithProviders(
      <HistoryScreen />,
    );
    await waitFor(() => expect(getByText("Game")).toBeTruthy());
    await fireEvent.press(getByText("All games"));
    await waitFor(() => {
      expect(getAllByText("Slots").length).toBeGreaterThan(0);
    });
  });

  it("filters by game", async () => {
    const { getByText, getAllByText } = await renderWithProviders(
      <HistoryScreen />,
    );
    await waitFor(() => getByText("All games"));
    await fireEvent.press(getByText("All games"));
    await waitFor(() => getAllByText("Slots"));
    const slotsItems = getAllByText("Slots");
    await fireEvent.press(slotsItems[slotsItems.length - 1]);
    await waitFor(() =>
      expect(mockGetAll).toHaveBeenCalledWith("slots", 1, 20),
    );
  });
});
