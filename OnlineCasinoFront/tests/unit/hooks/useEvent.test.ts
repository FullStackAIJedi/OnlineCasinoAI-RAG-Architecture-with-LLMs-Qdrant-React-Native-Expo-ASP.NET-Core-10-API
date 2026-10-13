import { act } from "@testing-library/react-native";
import { useEvent } from "../../../src/hooks/useEvent";
import { renderHookAsync } from "../../helpers";

describe("useEvent", () => {
  it("returns a function", async () => {
    const { result } = await renderHookAsync(() => useEvent(() => 42));
    expect(typeof result.current).toBe("function");
  });

  it("invokes latest callback", async () => {
    let count = 0;
    const { result, rerender } = await renderHookAsync(
      ({ n }: { n: number }) => useEvent(() => n),
      { initialProps: { n: 1 } },
    );

    expect(result.current()).toBe(1);

    await act(async () => {
      rerender({ n: 2 });
    });

    expect(result.current()).toBe(2);
  });

  it("returns stable reference across renders", async () => {
    const { result, rerender } = await renderHookAsync(() => useEvent(() => 0));
    const first = result.current;
    await act(async () => rerender({}));
    expect(result.current).toBe(first);
  });

  it("forwards arguments", async () => {
    const { result } = await renderHookAsync(() =>
      useEvent((a: number, b: number) => a + b),
    );
    expect(result.current(2, 3)).toBe(5);
  });
});
