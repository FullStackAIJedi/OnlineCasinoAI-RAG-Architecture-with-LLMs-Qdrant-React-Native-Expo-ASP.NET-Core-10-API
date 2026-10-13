// import { QueryClient } from "@tanstack/react-query";
// import { act, waitFor } from "@testing-library/react-native";

// import { slotsApi } from "../../../src/api/slots";
// import { walletApi } from "../../../src/api/wallet";
// import { useSlotsScreen } from "../../../src/hooks/useSlotsScreen";
// import {
//   createWrapper,
//   makeSpinResponse,
//   makeWallet,
//   renderHookAsync,
// } from "../../helpers";

// jest.mock("../../../src/api/slots");
// jest.mock("../../../src/api/wallet");
// jest.mock("../../../src/utils/uuid", () => ({
//   uuid: () => "test-uuid",
// }));

// jest.mock("expo-router", () => ({
//   useFocusEffect: (cb: () => void) => {
//     const React = require("react");
//     React.useEffect(cb, [cb]);
//   },
// }));

// const makeQc = () =>
//   new QueryClient({ defaultOptions: { queries: { retry: false } } });

// const setup = async (walletBalance = 500, gameCode = "slots") => {
//   (walletApi.get as jest.Mock).mockResolvedValue(
//     makeWallet({ balance: walletBalance }),
//   );

//   const qc = makeQc();
//   const hook = await renderHookAsync(() => useSlotsScreen({ gameCode }), {
//     wrapper: createWrapper(qc),
//   });

//   await waitFor(() =>
//     expect(hook.result.current.shownBalance).toBe(walletBalance),
//   );

//   return { hook, qc };
// };

// describe("useSlotsScreen", () => {
//   beforeEach(() => {
//     jest.clearAllMocks();
//     jest.useFakeTimers();
//   });

//   afterEach(() => {
//     jest.useRealTimers();
//   });

//   // ────────── Initial state ──────────

//   describe("initial state", () => {
//     it("defaults bet to 1", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.bet).toBe("1");
//     });

//     it("defaults error to undefined", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.error).toBeUndefined();
//     });

//     it("defaults spinning to false", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.spinning).toBe(false);
//     });

//     it("defaults finalSymbols to null", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.finalSymbols).toBeNull();
//     });

//     it("defaults matchCount to 0", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.matchCount).toBe(0);
//     });

//     it("defaults lastWin to 0", async () => {
//       const { hook } = await setup();
//       expect(hook.result.current.lastWin).toBe(0);
//     });

//     it("uses 3 reels for slots", async () => {
//       const { hook } = await setup(500, "slots");
//       expect(hook.result.current.reelsCount).toBe(3);
//     });

//     it("uses 4 reels for slots4", async () => {
//       const { hook } = await setup(500, "slots4");
//       expect(hook.result.current.reelsCount).toBe(4);
//     });

//     it("falls back to 3 reels for unknown code", async () => {
//       const { hook } = await setup(500, "unknown");
//       expect(hook.result.current.reelsCount).toBe(3);
//     });

//     it("uses currency from wallet", async () => {
//       (walletApi.get as jest.Mock).mockResolvedValue(
//         makeWallet({ currency: "USD" }),
//       );
//       const qc = makeQc();
//       const hook = await renderHookAsync(
//         () => useSlotsScreen({ gameCode: "slots" }),
//         { wrapper: createWrapper(qc) },
//       );
//       await waitFor(() => expect(hook.result.current.currency).toBe("USD"));
//     });

//     it("falls back to EUR", async () => {
//       (walletApi.get as jest.Mock).mockResolvedValue(undefined);
//       const qc = makeQc();
//       const hook = await renderHookAsync(
//         () => useSlotsScreen({ gameCode: "slots" }),
//         { wrapper: createWrapper(qc) },
//       );
//       expect(hook.result.current.currency).toBe("EUR");
//     });
//   });

//   // ────────── Validation ──────────

//   describe("validation", () => {
//     it("rejects empty bet", async () => {
//       const { hook } = await setup();

//       act(() => hook.result.current.handleBetChange(""));
//       act(() => hook.result.current.handleSpin());

//       await waitFor(() =>
//         expect(hook.result.current.error).toBe("Bet is required"),
//       );
//     });

//     it("rejects non-numeric bet", async () => {
//       const { hook } = await setup();

//       act(() => hook.result.current.handleBetChange("abc"));
//       act(() => hook.result.current.handleSpin());

//       await waitFor(() =>
//         expect(hook.result.current.error).toBe("Invalid bet"),
//       );
//     });

//     it("rejects zero bet", async () => {
//       const { hook } = await setup();

//       act(() => hook.result.current.handleBetChange("0"));
//       act(() => hook.result.current.handleSpin());

//       await waitFor(() =>
//         expect(hook.result.current.error).toBe("Invalid bet"),
//       );
//     });

//     it("rejects negative bet", async () => {
//       const { hook } = await setup();

//       act(() => hook.result.current.handleBetChange("-5"));
//       act(() => hook.result.current.handleSpin());

//       await waitFor(() =>
//         expect(hook.result.current.error).toBe("Invalid bet"),
//       );
//     });

//     it("rejects bet over balance", async () => {
//       const { hook } = await setup(100);

//       act(() => hook.result.current.handleBetChange("500"));
//       act(() => hook.result.current.handleSpin());

//       await waitFor(() =>
//         expect(hook.result.current.error).toBe("Insufficient balance"),
//       );
//     });

//     it("does not call API when invalid", async () => {
//       const { hook } = await setup();

//       act(() => hook.result.current.handleBetChange("abc"));
//       act(() => hook.result.current.handleSpin());

//       expect(slotsApi.spin).not.toHaveBeenCalled();
//     });
//   });

//   // ────────── handleSpin ──────────

//   describe("handleSpin", () => {
//     it("calls slotsApi.spin with gameCode + body", async () => {
//       (slotsApi.spin as jest.Mock).mockResolvedValue(
//         makeSpinResponse({ matchCount: 0 }),
//       );
//       const { hook } = await setup(500, "slots4");

//       act(() => hook.result.current.handleSpin());

//       await waitFor(() =>
//         expect(slotsApi.spin).toHaveBeenCalledWith("slots4", {
//           betAmount: 1,
//           clientRoundId: "test-uuid",
//         }),
//       );
//     });

//     it("applies optimistic balance", async () => {
//       (slotsApi.spin as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );
//       const { hook } = await setup(500);

//       act(() => hook.result.current.handleBetChange("50"));
//       act(() => hook.result.current.handleSpin());

//       await waitFor(() => expect(hook.result.current.shownBalance).toBe(450));
//     });

//     it("sets spinning=true", async () => {
//       (slotsApi.spin as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );
//       const { hook } = await setup();

//       act(() => hook.result.current.handleSpin());

//       await waitFor(() => expect(hook.result.current.spinning).toBe(true));
//     });

//     it("clears error on valid spin", async () => {
//       (slotsApi.spin as jest.Mock).mockResolvedValue(makeSpinResponse());
//       const { hook } = await setup();

//       act(() => hook.result.current.handleBetChange("abc"));
//       act(() => hook.result.current.handleSpin());
//       await waitFor(() =>
//         expect(hook.result.current.error).toBe("Invalid bet"),
//       );

//       act(() => hook.result.current.handleBetChange("10"));
//       act(() => hook.result.current.handleSpin());

//       await waitFor(() => expect(hook.result.current.error).toBeUndefined());
//     });

//     it("resets matchCount and lastWin", async () => {
//       (slotsApi.spin as jest.Mock).mockImplementation(
//         () => new Promise(() => {}),
//       );
//       const { hook } = await setup();

//       act(() => hook.result.current.handleSpin());

//       await waitFor(() => expect(hook.result.current.matchCount).toBe(0));
//     });

//     it("sets finalSymbols after min duration", async () => {
//       (slotsApi.spin as jest.Mock).mockResolvedValue(
//         makeSpinResponse({ reels: ["cherry", "cherry", "cherry"] }),
//       );
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.handleSpin();
//         // flush promise
//         await Promise.resolve();
//         // advance the min duration timer
//         jest.advanceTimersByTime(2000);
//         await Promise.resolve();
//       });

//       await waitFor(() =>
//         expect(hook.result.current.finalSymbols).toEqual([
//           "cherry",
//           "cherry",
//           "cherry",
//         ]),
//       );
//     });

//     it("stops spinning after response", async () => {
//       (slotsApi.spin as jest.Mock).mockResolvedValue(makeSpinResponse());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.handleSpin();
//         await Promise.resolve();
//         jest.advanceTimersByTime(2000);
//         await Promise.resolve();
//       });

//       await waitFor(() => expect(hook.result.current.spinning).toBe(false));
//     });

//     it("sets error on spin failure", async () => {
//       (slotsApi.spin as jest.Mock).mockRejectedValue(new Error("Network fail"));
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.error).toBe("Network fail");
//         expect(hook.result.current.spinning).toBe(false);
//       });
//     });

//     it("restores balance on failure", async () => {
//       (slotsApi.spin as jest.Mock).mockRejectedValue(new Error("fail"));
//       const { hook } = await setup(500);

//       act(() => hook.result.current.handleBetChange("50"));
//       await act(async () => {
//         hook.result.current.handleSpin();
//       });

//       await waitFor(() => expect(hook.result.current.shownBalance).toBe(500));
//     });
//   });

//   // ────────── handleReelsStop ──────────

//   describe("handleReelsStop", () => {
//     it("does nothing without pending result", async () => {
//       const { hook } = await setup();

//       await act(async () => {
//         await hook.result.current.handleReelsStop();
//       });

//       expect(hook.result.current.matchCount).toBe(0);
//     });

//     it("sets matchCount + lastWin from response", async () => {
//       (slotsApi.spin as jest.Mock).mockResolvedValue(
//         makeSpinResponse({ matchCount: 3, winAmount: 100 }),
//       );
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.handleSpin();
//         await Promise.resolve();
//         jest.advanceTimersByTime(2000);
//         await Promise.resolve();
//       });

//       await act(async () => {
//         await hook.result.current.handleReelsStop();
//       });

//       await waitFor(() => {
//         expect(hook.result.current.matchCount).toBe(3);
//         expect(hook.result.current.lastWin).toBe(100);
//       });
//     });

//     it("clears settling", async () => {
//       (slotsApi.spin as jest.Mock).mockResolvedValue(makeSpinResponse());
//       const { hook } = await setup();

//       await act(async () => {
//         hook.result.current.handleSpin();
//         await Promise.resolve();
//         jest.advanceTimersByTime(2000);
//         await Promise.resolve();
//       });

//       await act(async () => {
//         await hook.result.current.handleReelsStop();
//       });

//       expect(hook.result.current.settling).toBe(false);
//     });
//   });

//   // ────────── handleBetChange ──────────

//   describe("handleBetChange", () => {
//     it("updates bet", async () => {
//       const { hook } = await setup();
//       act(() => hook.result.current.handleBetChange("99"));
//       expect(hook.result.current.bet).toBe("99");
//     });

//     it("clears error on change", async () => {
//       const { hook } = await setup();

//       act(() => hook.result.current.handleBetChange("abc"));
//       act(() => hook.result.current.handleSpin());
//       await waitFor(() =>
//         expect(hook.result.current.error).toBe("Invalid bet"),
//       );

//       act(() => hook.result.current.handleBetChange("10"));
//       expect(hook.result.current.error).toBeUndefined();
//     });
//   });
// });

import { QueryClient } from "@tanstack/react-query";
import { act, waitFor } from "@testing-library/react-native";

import { slotsApi } from "../../../src/api/slots";
import { walletApi } from "../../../src/api/wallet";
import { useSlotsScreen } from "../../../src/hooks/useSlotsScreen";
import {
  createWrapper,
  makeSpinResponse,
  makeWallet,
  renderHookAsync,
} from "../../helpers";

jest.mock("../../../src/api/slots");
jest.mock("../../../src/api/wallet");
jest.mock("../../../src/utils/uuid", () => ({
  uuid: () => "test-uuid",
}));

jest.mock("expo-router", () => ({
  useFocusEffect: (cb: () => void) => {
    const React = require("react");
    React.useEffect(cb, [cb]);
  },
}));

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

const setup = async (walletBalance = 500, gameCode = "slots") => {
  (walletApi.get as jest.Mock).mockResolvedValue(
    makeWallet({ balance: walletBalance }),
  );

  const qc = makeQc();
  const hook = await renderHookAsync(() => useSlotsScreen({ gameCode }), {
    wrapper: createWrapper(qc),
  });

  await waitFor(() =>
    expect(hook.result.current.shownBalance).toBe(walletBalance),
  );

  return { hook, qc };
};

describe("useSlotsScreen", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  describe("initial state", () => {
    it("defaults bet to 1", async () => {
      const { hook } = await setup();
      expect(hook.result.current.bet).toBe("1");
    });

    it("defaults error to undefined", async () => {
      const { hook } = await setup();
      expect(hook.result.current.error).toBeUndefined();
    });

    it("defaults spinning to false", async () => {
      const { hook } = await setup();
      expect(hook.result.current.spinning).toBe(false);
    });

    it("defaults finalSymbols to null", async () => {
      const { hook } = await setup();
      expect(hook.result.current.finalSymbols).toBeNull();
    });

    it("defaults matchCount to 0", async () => {
      const { hook } = await setup();
      expect(hook.result.current.matchCount).toBe(0);
    });

    it("defaults lastWin to 0", async () => {
      const { hook } = await setup();
      expect(hook.result.current.lastWin).toBe(0);
    });

    it("uses 3 reels for slots", async () => {
      const { hook } = await setup(500, "slots");
      expect(hook.result.current.reelsCount).toBe(3);
    });

    it("uses 4 reels for slots4", async () => {
      const { hook } = await setup(500, "slots4");
      expect(hook.result.current.reelsCount).toBe(4);
    });

    it("falls back to 3 reels for unknown code", async () => {
      const { hook } = await setup(500, "unknown");
      expect(hook.result.current.reelsCount).toBe(3);
    });

    it("uses currency from wallet", async () => {
      (walletApi.get as jest.Mock).mockResolvedValue(
        makeWallet({ currency: "USD" }),
      );
      const qc = makeQc();
      const hook = await renderHookAsync(
        () => useSlotsScreen({ gameCode: "slots" }),
        { wrapper: createWrapper(qc) },
      );
      await waitFor(() => expect(hook.result.current.currency).toBe("USD"));
    });

    it("falls back to EUR", async () => {
      (walletApi.get as jest.Mock).mockResolvedValue(makeWallet());
      const qc = makeQc();
      const hook = await renderHookAsync(
        () => useSlotsScreen({ gameCode: "slots" }),
        { wrapper: createWrapper(qc) },
      );
      await waitFor(() => expect(hook.result.current.currency).toBe("EUR"));
    });
  });

  describe("validation", () => {
    it("rejects empty bet", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleBetChange("");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(hook.result.current.error).toBe("Bet is required"),
      );
    });

    it("rejects non-numeric bet", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleBetChange("abc");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(hook.result.current.error).toBe("Invalid bet"),
      );
    });

    it("rejects zero bet", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleBetChange("0");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(hook.result.current.error).toBe("Invalid bet"),
      );
    });

    it("rejects negative bet", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleBetChange("-5");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(hook.result.current.error).toBe("Invalid bet"),
      );
    });

    it("rejects bet over balance", async () => {
      const { hook } = await setup(100);

      await act(async () => {
        hook.result.current.handleBetChange("500");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(hook.result.current.error).toBe("Insufficient balance"),
      );
    });

    it("does not call API when invalid", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleBetChange("abc");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      expect(slotsApi.spin).not.toHaveBeenCalled();
    });
  });

  describe("handleSpin", () => {
    it("calls slotsApi.spin with gameCode + body", async () => {
      (slotsApi.spin as jest.Mock).mockResolvedValue(
        makeSpinResponse({ matchCount: 0 }),
      );
      const { hook } = await setup(500, "slots4");

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() =>
        expect(slotsApi.spin).toHaveBeenCalledWith("slots4", {
          betAmount: 1,
          clientRoundId: "test-uuid",
        }),
      );
    });

    it("applies optimistic balance", async () => {
      (slotsApi.spin as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );
      const { hook } = await setup(500);

      await act(async () => {
        hook.result.current.handleBetChange("50");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.shownBalance).toBe(450));
    });

    it("sets spinning=true", async () => {
      (slotsApi.spin as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.spinning).toBe(true));
    });

    it("clears error on valid spin", async () => {
      (slotsApi.spin as jest.Mock).mockResolvedValue(makeSpinResponse());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleBetChange("abc");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });
      await waitFor(() =>
        expect(hook.result.current.error).toBe("Invalid bet"),
      );

      await act(async () => {
        hook.result.current.handleBetChange("10");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.error).toBeUndefined());
    });

    it("resets matchCount and lastWin", async () => {
      (slotsApi.spin as jest.Mock).mockImplementation(
        () => new Promise(() => {}),
      );
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.matchCount).toBe(0));
    });

    it("sets finalSymbols after min duration", async () => {
      (slotsApi.spin as jest.Mock).mockResolvedValue(
        makeSpinResponse({ reels: ["cherry", "cherry", "cherry"] }),
      );
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
        await Promise.resolve();
        jest.advanceTimersByTime(2000);
        await Promise.resolve();
      });

      await waitFor(() =>
        expect(hook.result.current.finalSymbols).toEqual([
          "cherry",
          "cherry",
          "cherry",
        ]),
      );
    });

    it("stops spinning after response", async () => {
      (slotsApi.spin as jest.Mock).mockResolvedValue(makeSpinResponse());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
        await Promise.resolve();
        jest.advanceTimersByTime(2000);
        await Promise.resolve();
      });

      await waitFor(() => expect(hook.result.current.spinning).toBe(false));
    });

    it("sets error on spin failure", async () => {
      (slotsApi.spin as jest.Mock).mockRejectedValue(new Error("Network fail"));
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => {
        expect(hook.result.current.error).toBe("Network fail");
        expect(hook.result.current.spinning).toBe(false);
      });
    });

    it("restores balance on failure", async () => {
      (slotsApi.spin as jest.Mock).mockRejectedValue(new Error("fail"));
      const { hook } = await setup(500);

      await act(async () => {
        hook.result.current.handleBetChange("50");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });

      await waitFor(() => expect(hook.result.current.shownBalance).toBe(500));
    });
  });

  describe("handleReelsStop", () => {
    it("does nothing without pending result", async () => {
      const { hook } = await setup();

      await act(async () => {
        await hook.result.current.handleReelsStop();
      });

      expect(hook.result.current.matchCount).toBe(0);
    });

    it("sets matchCount + lastWin from response", async () => {
      (slotsApi.spin as jest.Mock).mockResolvedValue(
        makeSpinResponse({ matchCount: 3, winAmount: 100 }),
      );
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
        await Promise.resolve();
        jest.advanceTimersByTime(2000);
        await Promise.resolve();
      });

      await act(async () => {
        await hook.result.current.handleReelsStop();
      });

      await waitFor(() => {
        expect(hook.result.current.matchCount).toBe(3);
        expect(hook.result.current.lastWin).toBe(100);
      });
    });

    it("clears settling", async () => {
      (slotsApi.spin as jest.Mock).mockResolvedValue(makeSpinResponse());
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleSpin();
        await Promise.resolve();
        jest.advanceTimersByTime(2000);
        await Promise.resolve();
      });

      await act(async () => {
        await hook.result.current.handleReelsStop();
      });

      expect(hook.result.current.settling).toBe(false);
    });
  });

  describe("handleBetChange", () => {
    it("updates bet", async () => {
      const { hook } = await setup();
      await act(async () => {
        hook.result.current.handleBetChange("99");
      });
      expect(hook.result.current.bet).toBe("99");
    });

    it("clears error on change", async () => {
      const { hook } = await setup();

      await act(async () => {
        hook.result.current.handleBetChange("abc");
      });
      await act(async () => {
        hook.result.current.handleSpin();
      });
      await waitFor(() =>
        expect(hook.result.current.error).toBe("Invalid bet"),
      );

      await act(async () => {
        hook.result.current.handleBetChange("10");
      });
      expect(hook.result.current.error).toBeUndefined();
    });
  });
});
