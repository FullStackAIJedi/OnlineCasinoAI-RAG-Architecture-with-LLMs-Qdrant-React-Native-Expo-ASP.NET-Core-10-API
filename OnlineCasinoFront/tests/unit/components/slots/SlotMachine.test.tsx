import { render } from "@testing-library/react-native";
import { SlotMachine } from "../../../../src/components/slots/SlotMachine";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({ card: "#111" }),
}));

jest.mock("@expo/vector-icons", () => ({ Ionicons: () => null }));

describe("SlotMachine", () => {
  it("renders 3 reels", async () => {
    const { toJSON } = await render(
      <SlotMachine reelsCount={3} finalSymbols={null} spinning={false} />,
    );
    expect(toJSON()).toBeTruthy();
  });

  it("renders 4 reels", async () => {
    const { toJSON } = await render(
      <SlotMachine reelsCount={4} finalSymbols={null} spinning={false} />,
    );
    expect(toJSON()).toBeTruthy();
  });

  it("renders with finalSymbols", async () => {
    const { toJSON } = await render(
      <SlotMachine
        reelsCount={3}
        finalSymbols={["cherry", "cherry", "cherry"]}
        spinning={false}
      />,
    );
    expect(toJSON()).toBeTruthy();
  });
});
