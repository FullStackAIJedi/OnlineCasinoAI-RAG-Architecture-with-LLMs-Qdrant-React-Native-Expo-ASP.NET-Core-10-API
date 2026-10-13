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

  it("renders label text", async () => {
    const { getByText } = await render(
      <Radio label="Option A" selected={false} onSelect={jest.fn()} />,
    );
    expect(getByText("Option A")).toBeTruthy();
  });

  it("calls onSelect when pressed", async () => {
    const onSelect = jest.fn();
    const { getByText } = await render(
      <Radio label="Option A" selected={false} onSelect={onSelect} />,
    );
    fireEvent.press(getByText("Option A"));
    expect(onSelect).toHaveBeenCalledTimes(1);
  });

  it("does not call onSelect when already selected", async () => {
    const onSelect = jest.fn();
    const { getByText } = await render(
      <Radio label="Option A" selected={true} onSelect={onSelect} />,
    );
    fireEvent.press(getByText("Option A"));
    expect(onSelect).toHaveBeenCalledTimes(1);
  });
});
