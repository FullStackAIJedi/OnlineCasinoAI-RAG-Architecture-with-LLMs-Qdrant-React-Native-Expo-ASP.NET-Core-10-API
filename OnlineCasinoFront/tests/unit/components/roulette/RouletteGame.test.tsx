import { render } from "@testing-library/react-native";
import { RouletteGame } from "../../../../src/components/roulette/RouletteGame";

jest.mock("../../../../src/components/roulette/RouletteWheel", () => {
  const React = require("react");
  const { View } = require("react-native");
  return {
    RouletteWheel: () => React.createElement(View, { testID: "wheel" }),
  };
});

describe("RouletteGame", () => {
  it("does not show result when spinning", async () => {
    const { queryByText } = await render(
      <RouletteGame winningNumber={null} spinning />,
    );
    expect(queryByText("Result")).toBeNull();
  });

  it("shows red for red number", async () => {
    const { getByText } = await render(
      <RouletteGame winningNumber={1} spinning={false} />,
    );
    expect(getByText("1")).toBeTruthy();
    expect(getByText(/Red/)).toBeTruthy();
  });

  it("shows black for black number", async () => {
    const { getByText } = await render(
      <RouletteGame winningNumber={2} spinning={false} />,
    );
    expect(getByText("2")).toBeTruthy();
    expect(getByText(/Black/)).toBeTruthy();
  });

  it("shows green for 0", async () => {
    const { getByText } = await render(
      <RouletteGame winningNumber={0} spinning={false} />,
    );
    expect(getByText(/Green/)).toBeTruthy();
  });

  it("shows 00 for 37", async () => {
    const { getByText } = await render(
      <RouletteGame winningNumber={37} spinning={false} />,
    );
    expect(getByText("00")).toBeTruthy();
    expect(getByText(/Green/)).toBeTruthy();
  });

  it("shows Even for even number", async () => {
    const { getByText } = await render(
      <RouletteGame winningNumber={4} spinning={false} />,
    );
    expect(getByText(/Even/)).toBeTruthy();
  });

  it("shows Odd for odd number", async () => {
    const { getByText } = await render(
      <RouletteGame winningNumber={3} spinning={false} />,
    );
    expect(getByText(/Odd/)).toBeTruthy();
  });

  it("shows dash parity for 0", async () => {
    const { getAllByText } = await render(
      <RouletteGame winningNumber={0} spinning={false} />,
    );
    expect(getAllByText(/—/).length).toBeGreaterThan(0);
  });
});
