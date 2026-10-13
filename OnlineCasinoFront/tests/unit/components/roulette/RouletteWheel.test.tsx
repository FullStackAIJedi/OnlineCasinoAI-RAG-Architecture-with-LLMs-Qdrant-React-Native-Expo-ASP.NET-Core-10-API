import { render } from "@testing-library/react-native";
import { RouletteWheel } from "../../../../src/components/roulette/RouletteWheel";

jest.mock("../../../../src/hooks/useEvent", () => ({
  useEvent: (fn: any) => fn,
}));

describe("RouletteWheel", () => {
  it("renders without crashing", async () => {
    const { toJSON } = await render(
      <RouletteWheel winningNumber={null} spinning={false} />,
    );
    expect(toJSON()).toBeTruthy();
  });

  it("renders pointer", async () => {
    const { toJSON } = await render(
      <RouletteWheel winningNumber={5} spinning={false} />,
    );
    expect(toJSON()).toBeTruthy();
  });
});
