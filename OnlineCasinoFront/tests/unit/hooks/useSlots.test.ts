import { QueryClient } from "@tanstack/react-query";
import { act } from "@testing-library/react-native";

import { slotsApi } from "../../../src/api/slots";
import { useSpinSlots } from "../../../src/hooks/useSlots";
import { createWrapper, renderHookAsync } from "../../helpers";

jest.mock("../../../src/api/slots");

const makeQc = () =>
  new QueryClient({ defaultOptions: { queries: { retry: false } } });

describe("useSpinSlots", () => {
  it("calls slotsApi.spin with gameCode and data", async () => {
    (slotsApi.spin as jest.Mock).mockResolvedValue({});
    const qc = makeQc();
    const { result } = await renderHookAsync(() => useSpinSlots(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({
        gameCode: "slots",
        data: { betAmount: 10, clientRoundId: "u" },
      });
    });

    expect(slotsApi.spin).toHaveBeenCalledWith("slots", {
      betAmount: 10,
      clientRoundId: "u",
    });
  });

  it("invalidates wallet and history", async () => {
    (slotsApi.spin as jest.Mock).mockResolvedValue({});
    const qc = makeQc();
    const spy = jest.spyOn(qc, "invalidateQueries");
    const { result } = await renderHookAsync(() => useSpinSlots(), {
      wrapper: createWrapper(qc),
    });

    await act(async () => {
      await result.current.mutateAsync({
        gameCode: "slots",
        data: { betAmount: 10, clientRoundId: "u" },
      });
    });

    expect(spy).toHaveBeenCalledWith({ queryKey: ["wallet"] });
    expect(spy).toHaveBeenCalledWith({ queryKey: ["history"] });
  });
});
