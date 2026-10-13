// import { QueryClient } from "@tanstack/react-query";
// import { act, waitFor } from "@testing-library/react-native";

// jest.mock("../../../src/api/roulette");
// jest.mock("../../../src/api/wallet");
// jest.mock("../../../src/utils/uuid", () => ({
//   uuid: () => "test-uuid",
// }));

// jest.mock("expo-router", () => ({
//   useFocusEffect: (cb: () => void) => {
//     const React = require("react");
//     React.useEffect(cb, [cb]);
//   },
//   useRouter: () => ({ push: jest.fn(), back: jest.fn() }),
//   useLocalSearchParams: () => ({}),
// }));

// import { rouletteApi } from "../../../src/api/roulette";
// import { walletApi } from "../../../src/api/wallet";
// import { useRouletteScreen } from "../../../src/hooks/useRouletteScreen";
// import { createWrapper, makeCurrentBet, renderHookAsync } from "../../helpers";

// const makeQc = () =>
//   new QueryClient({ defaultOptions: { queries: { retry: false } } });

// const setup = async (walletBalance = 500, bets: any[] = []) => {
//   (walletApi.get as jest.Mock).mockResolvedValue({
//     balance: walletBalance,
//     currency: "EUR",
//   });
//   (rouletteApi.getBets as jest.Mock).mockResolvedValue(bets);

//   const qc = makeQc();
//   const hook = await renderHookAsync(() => useRouletteScreen(), {
//     wrapper: createWrapper(qc),
//   });

//   await waitFor(() =>
//     expect(hook.result.current.shownBalance).toBe(walletBalance),
//   );

//   return { hook, qc };
// };

// describe("useRouletteScreen", () => {
//   beforeEach(() => {
//     jest.clearAllMocks();
//   });

//   describe("initial state", () => {
//     it("defaults betType to red", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.betType).toBe("red");
//     });

//     it("defaults amount to 1", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.amount).toBe("1");
//     });

//     it("defaults value to empty", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.value).toBe("");
//     });

//     it("defaults errors to empty object", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.errors).toEqual({});
//     });

//     it("defaults spinning to false", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.spinning).toBe(false);
//     });

//     it("defaults winningNumber to null", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.winningNumber).toBeNull();
//     });

//     it("defaults resultText to null", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.resultText).toBeNull();
//     });

//     it("uses wallet currency", async () => {
//       (walletApi.get as jest.Mock).mockResolvedValue({
//         balance: 500,
//         currency: "USD",
//       });
//       (rouletteApi.getBets as jest.Mock).mockResolvedValue([]);
//       const qc = makeQc();
//       const hook = await renderHookAsync(() => useRouletteScreen(), {
//         wrapper: createWrapper(qc),
//       });
//       await waitFor(() => expect(hook.result.current.currency).toBe("USD"));
//     });

//     it("falls back to EUR when wallet missing", async () => {
//       (walletApi.get as jest.Mock).mockResolvedValue(null);
//       (rouletteApi.getBets as jest.Mock).mockResolvedValue([]);
//       const qc = makeQc();
//       const hook = await renderHookAsync(() => useRouletteScreen(), {
//         wrapper: createWrapper(qc),
//       });
//       await waitFor(() => expect(hook.result.current.currency).toBe("EUR"));
//     });

//     it("falls back to 0 balance", async () => {
//       (walletApi.get as jest.Mock).mockResolvedValue(null);
//       (rouletteApi.getBets as jest.Mock).mockResolvedValue([]);
//       const qc = makeQc();
//       const hook = await renderHookAsync(() => useRouletteScreen(), {
//         wrapper: createWrapper(qc),
//       });
//       await waitFor(() => expect(hook.result.current.shownBalance).toBe(0));
//     });
//   });

//   describe("validation", () => {
//     it("rejects empty amount", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.amount).toBe("Amount is required"),
//       );
//     });

//     it("rejects non-numeric amount", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("abc");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.amount).toBe("Invalid amount"),
//       );
//     });

//     it("rejects zero amount", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("0");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.amount).toBe("Invalid amount"),
//       );
//     });

//     it("rejects negative amount", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("-5");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.amount).toBe("Invalid amount"),
//       );
//     });

//     it("rejects amount over balance", async () => {
//       const { hook } = await setup(100);

//       await act(async () => {
//         hook.result.current.setAmount("500");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.amount).toBe("Insufficient balance"),
//       );
//     });

//     it("requires value for number bet", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBe("Value is required"),
//       );
//     });

//     it("rejects invalid number value", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("abc");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBe(
//           "Invalid value (0-36 or 00)",
//         ),
//       );
//     });

//     it("rejects number > 37", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("99");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBe("Must be 0-36 or 00"),
//       );
//     });

//     it("accepts 0 for number bet", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("0");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBeUndefined(),
//       );
//     });

//     it("accepts 00 for number bet", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("00");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBeUndefined(),
//       );
//     });

//     it("requires value for dozen bet", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("dozen");
//       });
//       await act(async () => {
//         hook.result.current.setValue("");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBe("Value is required"),
//       );
//     });

//     it("rejects dozen value 0", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("dozen");
//       });
//       await act(async () => {
//         hook.result.current.setValue("0");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBe("Must be 1-3"),
//       );
//     });

//     it("rejects dozen value 4", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("dozen");
//       });
//       await act(async () => {
//         hook.result.current.setValue("4");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBe("Must be 1-3"),
//       );
//     });

//     it("accepts dozen value 1", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("dozen");
//       });
//       await act(async () => {
//         hook.result.current.setValue("1");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBeUndefined(),
//       );
//     });

//     it("accepts dozen value 3", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("dozen");
//       });
//       await act(async () => {
//         hook.result.current.setValue("3");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.value).toBeUndefined(),
//       );
//     });
//   });

//   describe("handlePlaceBet", () => {
//     it("does not call API when invalid", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("abc");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       expect(rouletteApi.placeBet).not.toHaveBeenCalled();
//     });

//     it("calls placeBet with correct body for red", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("10");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.placeBet).toHaveBeenCalledWith("roulette", {
//           type: "red",
//           amount: 10,
//         }),
//       );
//     });

//     it("includes value for number bet", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("7");
//       });
//       await act(async () => {
//         hook.result.current.setAmount("10");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.placeBet).toHaveBeenCalledWith("roulette", {
//           type: "number",
//           amount: 10,
//           value: 7,
//         }),
//       );
//     });

//     it("maps 00 to 37 in body", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("00");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.placeBet).toHaveBeenCalledWith(
//           "roulette",
//           expect.objectContaining({ value: 37 }),
//         ),
//       );
//     });

//     it("includes value for dozen bet", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("dozen");
//       });
//       await act(async () => {
//         hook.result.current.setValue("2");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.placeBet).toHaveBeenCalledWith("roulette", {
//           type: "dozen",
//           amount: 1,
//           value: 2,
//         }),
//       );
//     });

//     it("applies optimistic balance deduction", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );
//       const { hook } = await setup(500);

//       await act(async () => {
//         hook.result.current.setAmount("50");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() => expect(hook.result.current.shownBalance).toBe(450));
//     });

//     it("clears value on success", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setBetType("number");
//       });
//       await act(async () => {
//         hook.result.current.setValue("7");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() => expect(hook.result.current.value).toBe(""));
//     });

//     it("sets error text on failure", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockRejectedValue(
//         new Error("Bet failed"),
//       );
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("10");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.resultText).toBe("Bet failed");
//         expect(hook.result.current.resultColor).toBe("red");
//       });
//     });

//     it("restores balance on failure", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockRejectedValue(
//         new Error("Bet failed"),
//       );
//       const { hook } = await setup(500);

//       await act(async () => {
//         hook.result.current.setAmount("50");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() => expect(hook.result.current.shownBalance).toBe(500));
//     });

//     it("clears error on valid input after invalid attempt", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("abc");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });
//       await waitFor(() =>
//         expect(hook.result.current.errors.amount).toBe("Invalid amount"),
//       );

//       await act(async () => {
//         hook.result.current.setAmount("10");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.errors.amount).toBeUndefined(),
//       );
//     });

//     it("calls API once per successful bet", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("10");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.placeBet).toHaveBeenCalledTimes(1),
//       );
//     });

//     it("places bet with amount as number", async () => {
//       (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.setAmount("25.5");
//       });
//       await act(async () => {
//         hook.result.current.handlePlaceBet();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.placeBet).toHaveBeenCalledWith(
//           "roulette",
//           expect.objectContaining({ amount: 25.5 }),
//         ),
//       );
//     });
//   });

//   describe("handleSpin", () => {
//     it("requires at least one bet", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.resultText).toBe(
//           "Place at least one bet before spinning",
//         );
//         expect(rouletteApi.spin).not.toHaveBeenCalled();
//       });
//     });

//     it("calls spin when bets exist", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 5,
//         winningColor: "red",
//         totalBet: 10,
//         totalWin: 0,
//         balance: 490,
//         result: "Loss",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.spin).toHaveBeenCalledWith("roulette", {
//           clientRoundId: "test-uuid",
//         }),
//       );
//     });

//     it("sets spinning true immediately", async () => {
//       (rouletteApi.spin as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );

//       const { hook } = await setup(500, [makeCurrentBet()]);

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => expect(hook.result.current.spinning).toBe(true));
//     });

//     it("resets winningNumber before spin", async () => {
//       (rouletteApi.spin as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );

//       const { hook } = await setup(500, [makeCurrentBet()]);

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => expect(hook.result.current.winningNumber).toBeNull());
//     });

//     it("sets winningNumber from spin response", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 17,
//         winningColor: "black",
//         totalBet: 10,
//         totalWin: 0,
//         balance: 490,
//         result: "Loss",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => expect(hook.result.current.winningNumber).toBe(17));
//     });

//     it("stops spinning on spin error", async () => {
//       (rouletteApi.spin as jest.Mock).mockRejectedValue(
//         new Error("Spin failed"),
//       );

//       const { hook } = await setup(500, [makeCurrentBet()]);

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.spinning).toBe(false);
//         expect(hook.result.current.resultText).toBe("Spin failed");
//         expect(hook.result.current.resultColor).toBe("red");
//       });
//     });

//     it("clears result text on new spin", async () => {
//       (rouletteApi.spin as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );

//       const { hook } = await setup(500, [makeCurrentBet()]);

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => expect(hook.result.current.resultText).toBeNull());
//     });

//     it("uses uuid from mock", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 5,
//         winningColor: "red",
//         totalBet: 10,
//         totalWin: 0,
//         balance: 490,
//         result: "Loss",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() =>
//         expect(rouletteApi.spin).toHaveBeenCalledWith(
//           "roulette",
//           expect.objectContaining({ clientRoundId: "test-uuid" }),
//         ),
//       );
//     });
//   });

//   describe("handleSpinStop", () => {
//     it("does nothing when no pending result", async () => {
//       const { hook } = await setup();
//       await act(async () => {
//         await hook.result.current.handleSpinStop();
//       });
//       expect(hook.result.current.resultText).toBeNull();
//     });

//     it("shows win text when totalWin > 0", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 5,
//         winningColor: "red",
//         totalBet: 10,
//         totalWin: 20,
//         balance: 520,
//         result: "Win",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);
//       await act(async () => hook.result.current.handleSpin());
//       await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

//       await act(async () => {
//         await hook.result.current.handleSpinStop();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.resultText).toMatch(/You won 20.00/);
//         expect(hook.result.current.resultColor).toBe("green");
//       });
//     });

//     it("shows push text for Push result", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 0,
//         winningColor: "green",
//         totalBet: 10,
//         totalWin: 0,
//         balance: 500,
//         result: "Push",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);
//       await act(async () => hook.result.current.handleSpin());
//       await waitFor(() => expect(hook.result.current.winningNumber).toBe(0));

//       await act(async () => {
//         await hook.result.current.handleSpinStop();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.resultText).toBe("Push, bet returned");
//         expect(hook.result.current.resultColor).toBe("orange");
//       });
//     });

//     it("shows loss text otherwise", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 5,
//         winningColor: "red",
//         totalBet: 10,
//         totalWin: 0,
//         balance: 490,
//         result: "Loss",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);
//       await act(async () => hook.result.current.handleSpin());
//       await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

//       await act(async () => {
//         await hook.result.current.handleSpinStop();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.resultText).toMatch(/You lost/);
//         expect(hook.result.current.resultColor).toBe("red");
//       });
//     });

//     it("clears spinning", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 5,
//         winningColor: "red",
//         totalBet: 10,
//         totalWin: 0,
//         balance: 490,
//         result: "Loss",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);
//       await act(async () => hook.result.current.handleSpin());
//       await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

//       await act(async () => {
//         await hook.result.current.handleSpinStop();
//       });

//       expect(hook.result.current.spinning).toBe(false);
//     });

//     it("keeps winningNumber after stop", async () => {
//       (rouletteApi.spin as jest.Mock).mockResolvedValue({
//         winningNumber: 5,
//         winningColor: "red",
//         totalBet: 10,
//         totalWin: 0,
//         balance: 490,
//         result: "Loss",
//         roundId: "r1",
//       });

//       const { hook } = await setup(500, [makeCurrentBet()]);
//       await act(async () => hook.result.current.handleSpin());
//       await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

//       await act(async () => {
//         await hook.result.current.handleSpinStop();
//       });

//       expect(hook.result.current.winningNumber).toBe(5);
//     });
//   });

//   describe("setters", () => {
//     it("setBetType updates betType", async () => {
//       const { hook } = await setup();
//       await act(async () => {
//         hook.result.current.setBetType("black");
//       });
//       expect(hook.result.current.betType).toBe("black");
//     });

//     it("setAmount updates amount", async () => {
//       const { hook } = await setup();
//       await act(async () => {
//         hook.result.current.setAmount("99");
//       });
//       expect(hook.result.current.amount).toBe("99");
//     });

//     it("setValue updates value", async () => {
//       const { hook } = await setup();
//       await act(async () => {
//         hook.result.current.setValue("7");
//       });
//       expect(hook.result.current.value).toBe("7");
//     });

//     it("setErrors updates errors", async () => {
//       const { hook } = await setup();
//       await act(async () => {
//         hook.result.current.setErrors({ amount: "test" });
//       });
//       expect(hook.result.current.errors.amount).toBe("test");
//     });

//     it("setBetType to each valid type", async () => {
//       const { hook } = await setup();
//       const types = [
//         "red",
//         "black",
//         "even",
//         "odd",
//         "low",
//         "high",
//         "dozen",
//         "number",
//       ] as const;

//       for (const t of types) {
//         await act(async () => {
//           hook.result.current.setBetType(t);
//         });
//         expect(hook.result.current.betType).toBe(t);
//       }
//     });
//   });

//   describe("bets loading", () => {
//     it("loads bets from API", async () => {
//       const bets = [
//         makeCurrentBet({ id: 1, type: "red", amount: 10 }),
//         makeCurrentBet({ id: 2, type: "black", amount: 20 }),
//       ];
//       const { hook } = await setup(500, bets);

//       await waitFor(() => expect(hook.result.current.bets?.length).toBe(2));
//     });

//     it("shows empty bets array when none", async () => {
//       const { hook } = await setup();
//       await waitFor(() => expect(hook.result.current.bets?.length).toBe(0));
//     });

//     it("bets is undefined while loading", async () => {
//       (rouletteApi.getBets as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );
//       (walletApi.get as jest.Mock).mockResolvedValue({
//         balance: 500,
//         currency: "EUR",
//       });

//       const qc = makeQc();
//       const hook = await renderHookAsync(() => useRouletteScreen(), {
//         wrapper: createWrapper(qc),
//       });

//       expect(hook.result.current.bets).toBeUndefined();
//     });
//   });
// });

import { QueryClient } from "@tanstack/react-query";
import { act, waitFor } from "@testing-library/react-native";

jest.mock("../../../src/api/roulette");
jest.mock("../../../src/api/wallet");
jest.mock("../../../src/utils/uuid", () => ({
  uuid: () => "test-uuid",
}));

jest.mock("expo-router", () => ({
  useFocusEffect: (cb: () => void) => {
    const React = require("react");
    React.useEffect(cb, [cb]);
  },
  useRouter: () => ({ push: jest.fn(), back: jest.fn() }),
  useLocalSearchParams: () => ({}),
}));

import { rouletteApi } from "../../../src/api/roulette";
import { walletApi } from "../../../src/api/wallet";
import { useRouletteScreen } from "../../../src/hooks/useRouletteScreen";
import { createWrapper, makeCurrentBet, renderHookAsync } from "../../helpers";

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

const setup = async (walletBalance = 500, bets: any[] = []) => {
  (walletApi.get as jest.Mock).mockResolvedValue({
    balance: walletBalance,
    currency: "EUR",
  });
  (rouletteApi.getBets as jest.Mock).mockResolvedValue(bets);

  const qc = makeQc();
  const hook = await renderHookAsync(() => useRouletteScreen(), {
    wrapper: createWrapper(qc),
  });

  await waitFor(() =>
    expect(hook.result.current.shownBalance).toBe(walletBalance),
  );

  return { hook, qc };
};

describe("useRouletteScreen", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (walletApi.get as jest.Mock).mockReset();
    (rouletteApi.getBets as jest.Mock).mockReset();
    (rouletteApi.placeBet as jest.Mock).mockReset();
    (rouletteApi.spin as jest.Mock).mockReset();
  });

  afterEach(() => {
    jest.clearAllTimers();
    jest.useRealTimers();
    jest.restoreAllMocks();
  });

  describe("initial state", () => {
    it("defaults betType to red", async () => {
      const { hook } = await setup();
      expect(hook.result.current.betType).toBe("red");
    });

    it("defaults amount to 1", async () => {
      const { hook } = await setup();
      expect(hook.result.current.amount).toBe("1");
    });

    it("defaults value to empty", async () => {
      const { hook } = await setup();
      expect(hook.result.current.value).toBe("");
    });

    it("defaults errors to empty object", async () => {
      const { hook } = await setup();
      expect(hook.result.current.errors).toEqual({});
    });

    it("defaults spinning to false", async () => {
      const { hook } = await setup();
      expect(hook.result.current.spinning).toBe(false);
    });

    it("defaults winningNumber to null", async () => {
      const { hook } = await setup();
      expect(hook.result.current.winningNumber).toBeNull();
    });

    it("defaults resultText to null", async () => {
      const { hook } = await setup();
      expect(hook.result.current.resultText).toBeNull();
    });

    it("uses wallet currency", async () => {
      (walletApi.get as jest.Mock).mockResolvedValue({
        balance: 500,
        currency: "USD",
      });
      (rouletteApi.getBets as jest.Mock).mockResolvedValue([]);
      const qc = makeQc();
      const hook = await renderHookAsync(() => useRouletteScreen(), {
        wrapper: createWrapper(qc),
      });
      await waitFor(() => expect(hook.result.current.currency).toBe("USD"));
    });

    it("falls back to EUR when wallet missing", async () => {
      (walletApi.get as jest.Mock).mockResolvedValue(null);
      (rouletteApi.getBets as jest.Mock).mockResolvedValue([]);
      const qc = makeQc();
      const hook = await renderHookAsync(() => useRouletteScreen(), {
        wrapper: createWrapper(qc),
      });
      await waitFor(() => expect(hook.result.current.currency).toBe("EUR"));
    });

    it("falls back to 0 balance", async () => {
      (walletApi.get as jest.Mock).mockResolvedValue(null);
      (rouletteApi.getBets as jest.Mock).mockResolvedValue([]);
      const qc = makeQc();
      const hook = await renderHookAsync(() => useRouletteScreen(), {
        wrapper: createWrapper(qc),
      });
      await waitFor(() => expect(hook.result.current.shownBalance).toBe(0));
    });
  });

  describe("validation", () => {
    it("rejects empty amount", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.amount).toBe("Amount is required"),
      );
    });

    it("rejects non-numeric amount", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("abc");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.amount).toBe("Invalid amount"),
      );
    });

    it("rejects zero amount", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("0");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.amount).toBe("Invalid amount"),
      );
    });

    it("rejects negative amount", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("-5");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.amount).toBe("Invalid amount"),
      );
    });

    it("rejects amount over balance", async () => {
      const { hook } = await setup(100);

      await act(async () => {
        hook.result.current.setAmount("500");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.amount).toBe("Insufficient balance"),
      );
    });

    it("requires value for number bet", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBe("Value is required"),
      );
    });

    it("rejects invalid number value", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("abc");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBe(
          "Invalid value (0-36 or 00)",
        ),
      );
    });

    it("rejects number > 37", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("99");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBe("Must be 0-36 or 00"),
      );
    });

    it("accepts 0 for number bet", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("0");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBeUndefined(),
      );
    });

    it("accepts 00 for number bet", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("00");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBeUndefined(),
      );
    });

    it("requires value for dozen bet", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("dozen");
      });
      await act(async () => {
        hook.result.current.setValue("");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBe("Value is required"),
      );
    });

    it("rejects dozen value 0", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("dozen");
      });
      await act(async () => {
        hook.result.current.setValue("0");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBe("Must be 1-3"),
      );
    });

    it("rejects dozen value 4", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("dozen");
      });
      await act(async () => {
        hook.result.current.setValue("4");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBe("Must be 1-3"),
      );
    });

    it("accepts dozen value 1", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("dozen");
      });
      await act(async () => {
        hook.result.current.setValue("1");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBeUndefined(),
      );
    });

    it("accepts dozen value 3", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("dozen");
      });
      await act(async () => {
        hook.result.current.setValue("3");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.value).toBeUndefined(),
      );
    });
  });

  describe("handlePlaceBet", () => {
    it("does not call API when invalid", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("abc");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      expect(rouletteApi.placeBet).not.toHaveBeenCalled();
    });

    it("calls placeBet with correct body for red", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("10");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(rouletteApi.placeBet).toHaveBeenCalledWith("roulette", {
          type: "red",
          amount: 10,
        }),
      );
    });

    it("includes value for number bet", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("7");
      });
      await act(async () => {
        hook.result.current.setAmount("10");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(rouletteApi.placeBet).toHaveBeenCalledWith("roulette", {
          type: "number",
          amount: 10,
          value: 7,
        }),
      );
    });

    it("maps 00 to 37 in body", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("00");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(rouletteApi.placeBet).toHaveBeenCalledWith(
          "roulette",
          expect.objectContaining({ value: 37 }),
        ),
      );
    });

    it("includes value for dozen bet", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("dozen");
      });
      await act(async () => {
        hook.result.current.setValue("2");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(rouletteApi.placeBet).toHaveBeenCalledWith("roulette", {
          type: "dozen",
          amount: 1,
          value: 2,
        }),
      );
    });

    it("applies optimistic balance deduction", async () => {
      (rouletteApi.placeBet as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );
      const { hook } = await setup(500);

      await act(async () => {
        hook.result.current.setAmount("50");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() => expect(hook.result.current.shownBalance).toBe(450));
    });

    it("clears value on success", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setBetType("number");
      });
      await act(async () => {
        hook.result.current.setValue("7");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() => expect(hook.result.current.value).toBe(""));
    });

    it("sets error text on failure", async () => {
      (rouletteApi.placeBet as jest.Mock).mockRejectedValue(
        new Error("Bet failed"),
      );
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("10");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() => {
        expect(hook.result.current.resultText).toBe("Bet failed");
        expect(hook.result.current.resultColor).toBe("red");
      });
    });

    it("restores balance on failure", async () => {
      (rouletteApi.placeBet as jest.Mock).mockRejectedValue(
        new Error("Bet failed"),
      );
      const { hook } = await setup(500);

      await act(async () => {
        hook.result.current.setAmount("50");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() => expect(hook.result.current.shownBalance).toBe(500));
    });

    it("clears error on valid input after invalid attempt", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("abc");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });
      await waitFor(() =>
        expect(hook.result.current.errors.amount).toBe("Invalid amount"),
      );

      await act(async () => {
        hook.result.current.setAmount("10");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(hook.result.current.errors.amount).toBeUndefined(),
      );
    });

    it("calls API once per successful bet", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("10");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(rouletteApi.placeBet).toHaveBeenCalledTimes(1),
      );
    });

    it("places bet with amount as number", async () => {
      (rouletteApi.placeBet as jest.Mock).mockResolvedValue(makeCurrentBet());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.setAmount("25.5");
      });
      await act(async () => {
        hook.result.current.handlePlaceBet();
      });

      await waitFor(() =>
        expect(rouletteApi.placeBet).toHaveBeenCalledWith(
          "roulette",
          expect.objectContaining({ amount: 25.5 }),
        ),
      );
    });
  });

  describe("handleSpin", () => {
    it("requires at least one bet", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => {
        expect(hook.result.current.resultText).toBe(
          "Place at least one bet before spinning",
        );
        expect(rouletteApi.spin).not.toHaveBeenCalled();
      });
    });

    it("calls spin when bets exist", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 5,
        winningColor: "red",
        totalBet: 10,
        totalWin: 0,
        balance: 490,
        result: "Loss",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(rouletteApi.spin).toHaveBeenCalledWith("roulette", {
          clientRoundId: "test-uuid",
        }),
      );
    });

    it("sets spinning true immediately", async () => {
      (rouletteApi.spin as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );

      const { hook } = await setup(500, [makeCurrentBet()]);

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.spinning).toBe(true));
    });

    it("resets winningNumber before spin", async () => {
      (rouletteApi.spin as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );

      const { hook } = await setup(500, [makeCurrentBet()]);

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.winningNumber).toBeNull());
    });

    it("sets winningNumber from spin response", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 17,
        winningColor: "black",
        totalBet: 10,
        totalWin: 0,
        balance: 490,
        result: "Loss",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.winningNumber).toBe(17));
    });

    it("stops spinning on spin error", async () => {
      (rouletteApi.spin as jest.Mock).mockRejectedValue(
        new Error("Spin failed"),
      );

      const { hook } = await setup(500, [makeCurrentBet()]);

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => {
        expect(hook.result.current.spinning).toBe(false);
        expect(hook.result.current.resultText).toBe("Spin failed");
        expect(hook.result.current.resultColor).toBe("red");
      });
    });

    it("clears result text on new spin", async () => {
      (rouletteApi.spin as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );

      const { hook } = await setup(500, [makeCurrentBet()]);

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.resultText).toBeNull());
    });

    it("uses uuid from mock", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 5,
        winningColor: "red",
        totalBet: 10,
        totalWin: 0,
        balance: 490,
        result: "Loss",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(rouletteApi.spin).toHaveBeenCalledWith(
          "roulette",
          expect.objectContaining({ clientRoundId: "test-uuid" }),
        ),
      );
    });
  });

  describe("handleSpinStop", () => {
    it("does nothing when no pending result", async () => {
      const { hook } = await setup();
      await act(async () => {
        await hook.result.current.handleSpinStop();
      });
      expect(hook.result.current.resultText).toBeNull();
    });

    it("shows win text when totalWin > 0", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 5,
        winningColor: "red",
        totalBet: 10,
        totalWin: 20,
        balance: 520,
        result: "Win",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);
      await act(async () => hook.result.current.handleSpin());
      await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

      await act(async () => {
        await hook.result.current.handleSpinStop();
      });

      await waitFor(() => {
        expect(hook.result.current.resultText).toMatch(/You won 20.00/);
        expect(hook.result.current.resultColor).toBe("green");
      });
    });

    it("shows push text for Push result", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 0,
        winningColor: "green",
        totalBet: 10,
        totalWin: 0,
        balance: 500,
        result: "Push",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);
      await act(async () => hook.result.current.handleSpin());
      await waitFor(() => expect(hook.result.current.winningNumber).toBe(0));

      await act(async () => {
        await hook.result.current.handleSpinStop();
      });

      await waitFor(() => {
        expect(hook.result.current.resultText).toBe("Push, bet returned");
        expect(hook.result.current.resultColor).toBe("orange");
      });
    });

    it("shows loss text otherwise", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 5,
        winningColor: "red",
        totalBet: 10,
        totalWin: 0,
        balance: 490,
        result: "Loss",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);
      await act(async () => hook.result.current.handleSpin());
      await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

      await act(async () => {
        await hook.result.current.handleSpinStop();
      });

      await waitFor(() => {
        expect(hook.result.current.resultText).toMatch(/You lost/);
        expect(hook.result.current.resultColor).toBe("red");
      });
    });

    it("clears spinning", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 5,
        winningColor: "red",
        totalBet: 10,
        totalWin: 0,
        balance: 490,
        result: "Loss",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);
      await act(async () => hook.result.current.handleSpin());
      await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

      await act(async () => {
        await hook.result.current.handleSpinStop();
      });

      expect(hook.result.current.spinning).toBe(false);
    });

    it("keeps winningNumber after stop", async () => {
      (rouletteApi.spin as jest.Mock).mockResolvedValue({
        winningNumber: 5,
        winningColor: "red",
        totalBet: 10,
        totalWin: 0,
        balance: 490,
        result: "Loss",
        roundId: "r1",
      });

      const { hook } = await setup(500, [makeCurrentBet()]);
      await act(async () => hook.result.current.handleSpin());
      await waitFor(() => expect(hook.result.current.winningNumber).toBe(5));

      await act(async () => {
        await hook.result.current.handleSpinStop();
      });

      expect(hook.result.current.winningNumber).toBe(5);
    });
  });

  describe("setters", () => {
    it("setBetType updates betType", async () => {
      const { hook } = await setup();
      await act(async () => {
        hook.result.current.setBetType("black");
      });
      expect(hook.result.current.betType).toBe("black");
    });

    it("setAmount updates amount", async () => {
      const { hook } = await setup();
      await act(async () => {
        hook.result.current.setAmount("99");
      });
      expect(hook.result.current.amount).toBe("99");
    });

    it("setValue updates value", async () => {
      const { hook } = await setup();
      await act(async () => {
        hook.result.current.setValue("7");
      });
      expect(hook.result.current.value).toBe("7");
    });

    it("setErrors updates errors", async () => {
      const { hook } = await setup();
      await act(async () => {
        hook.result.current.setErrors({ amount: "test" });
      });
      expect(hook.result.current.errors.amount).toBe("test");
    });

    it("setBetType to each valid type", async () => {
      const { hook } = await setup();
      const types = [
        "red",
        "black",
        "even",
        "odd",
        "low",
        "high",
        "dozen",
        "number",
      ] as const;

      for (const t of types) {
        await act(async () => {
          hook.result.current.setBetType(t);
        });
        expect(hook.result.current.betType).toBe(t);
      }
    });
  });

  describe("bets loading", () => {
    it("loads bets from API", async () => {
      const bets = [
        makeCurrentBet({ id: 1, type: "red", amount: 10 }),
        makeCurrentBet({ id: 2, type: "black", amount: 20 }),
      ];
      const { hook } = await setup(500, bets);

      await waitFor(() => expect(hook.result.current.bets?.length).toBe(2));
    });

    it("shows empty bets array when none", async () => {
      const { hook } = await setup();
      await waitFor(() => expect(hook.result.current.bets?.length).toBe(0));
    });

    it("bets is undefined while loading", async () => {
      (rouletteApi.getBets as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );
      (walletApi.get as jest.Mock).mockResolvedValue({
        balance: 500,
        currency: "EUR",
      });

      const qc = makeQc();
      const hook = await renderHookAsync(() => useRouletteScreen(), {
        wrapper: createWrapper(qc),
      });

      expect(hook.result.current.bets).toBeUndefined();
    });
  });
});
