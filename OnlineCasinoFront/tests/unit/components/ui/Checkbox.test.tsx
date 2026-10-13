import { fireEvent, render } from "@testing-library/react-native";
import { Checkbox } from "../../../../src/components/ui/Checkbox";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({ primary: "#0f0", border: "#ccc", text: "#fff" }),
}));

describe("Checkbox", () => {
  it("renders label", async () => {
    const { getByText } = await render(
      <Checkbox checked={false} onToggle={jest.fn()} label="Agree" />,
    );
    expect(getByText("Agree")).toBeTruthy();
  });

  it("calls onToggle", async () => {
    const onToggle = jest.fn();
    const { getByText } = await render(
      <Checkbox checked={false} onToggle={onToggle} label="Agree" />,
    );
    fireEvent.press(getByText("Agree"));
    expect(onToggle).toHaveBeenCalled();
  });
});
