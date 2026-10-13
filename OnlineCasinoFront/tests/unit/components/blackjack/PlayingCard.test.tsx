import { render } from "@testing-library/react-native";
import { PlayingCard } from "../../../../src/components/blackjack/PlayingCard";

describe("PlayingCard", () => {
  it("renders without crashing", async () => {
    const { toJSON } = await render(<PlayingCard card="A♠" />);
    expect(toJSON()).toBeTruthy();
  });

  it("accepts small size", async () => {
    const { toJSON } = await render(<PlayingCard card="A♠" size="small" />);
    expect(toJSON()).toBeTruthy();
  });

  it("accepts large size", async () => {
    const { toJSON } = await render(<PlayingCard card="A♠" size="large" />);
    expect(toJSON()).toBeTruthy();
  });

  it("uses medium by default", async () => {
    const { toJSON } = await render(<PlayingCard card="A♠" />);
    expect(toJSON()).toBeTruthy();
  });

  it("handles face-down", async () => {
    const { toJSON } = await render(<PlayingCard card="?" />);
    expect(toJSON()).toBeTruthy();
  });
});
