import { fireEvent, render } from "@testing-library/react-native";
import { Radio } from "../../../../src/components/ui/Radio";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({ primary: "#0f0", border: "#ccc", text: "#fff" }),
}));

describe("Radio", () => {
  it("renders label", async () => {
    const { getByText } = await render(
      <Radio selected={false} onSelect={jest.fn()} label="A" />,
    );
    expect(getByText("A")).toBeTruthy();
  });

  it("calls onSelect", async () => {
    const onSelect = jest.fn();
    const { getByText } = await render(
      <Radio selected={false} onSelect={onSelect} label="A" />,
    );
    fireEvent.press(getByText("A"));
    expect(onSelect).toHaveBeenCalled();
  });
});
