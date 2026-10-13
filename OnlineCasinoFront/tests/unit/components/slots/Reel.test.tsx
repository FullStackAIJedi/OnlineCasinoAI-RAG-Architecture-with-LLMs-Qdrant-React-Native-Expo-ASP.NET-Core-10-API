import { render } from "@testing-library/react-native";
import { Reel } from "../../../../src/components/slots/Reel";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({ card: "#111" }),
}));

jest.mock("@expo/vector-icons", () => ({ Ionicons: () => null }));

describe("Reel", () => {
  it("renders without crashing", async () => {
    const { toJSON } = await render(
      <Reel finalSymbol={null} spinning={false} />,
    );
    expect(toJSON()).toBeTruthy();
  });

  it("renders with finalSymbol", async () => {
    const { toJSON } = await render(
      <Reel finalSymbol="cherry" spinning={false} />,
    );
    expect(toJSON()).toBeTruthy();
  });

  it("accepts reelsCount", async () => {
    const { toJSON } = await render(
      <Reel finalSymbol="cherry" spinning={false} reelsCount={4} />,
    );
    expect(toJSON()).toBeTruthy();
  });
});
