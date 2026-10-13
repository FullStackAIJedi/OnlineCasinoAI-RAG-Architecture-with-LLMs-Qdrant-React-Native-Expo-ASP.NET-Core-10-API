import { fireEvent, render } from "@testing-library/react-native";
import { GameCard } from "../../../src/components/GameCard";

describe("GameCard", () => {
  const defaultProps = {
    code: "slots",
    name: "Slots",
    image: 1,
    onPress: jest.fn(),
  };

  it("renders name", async () => {
    const { getByText } = await render(<GameCard {...defaultProps} />);
    expect(getByText("Slots")).toBeTruthy();
  });

  it("calls onPress", async () => {
    const onPress = jest.fn();
    const { getByText } = await render(
      <GameCard {...defaultProps} onPress={onPress} />,
    );
    fireEvent.press(getByText("Slots"));
    expect(onPress).toHaveBeenCalled();
  });

  it("renders different names", async () => {
    const { getByText } = await render(
      <GameCard {...defaultProps} name="Roulette" />,
    );
    expect(getByText("Roulette")).toBeTruthy();
  });
});
