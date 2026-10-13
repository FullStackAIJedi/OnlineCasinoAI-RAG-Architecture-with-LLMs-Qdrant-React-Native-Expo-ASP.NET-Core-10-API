import { fireEvent, render } from "@testing-library/react-native";
import { TextArea } from "../../../../src/components/ui/TextArea";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({
    card: "#111",
    text: "#fff",
    danger: "#f00",
    border: "#ccc",
    textSecondary: "#aaa",
  }),
}));

describe("TextArea", () => {
  it("renders label and placeholder", async () => {
    const { getByText, getByPlaceholderText } = await render(
      <TextArea
        value=""
        onChangeText={jest.fn()}
        label="Bio"
        placeholder="..."
      />,
    );
    expect(getByText("Bio")).toBeTruthy();
    expect(getByPlaceholderText("...")).toBeTruthy();
  });

  it("calls onChangeText", async () => {
    const onChangeText = jest.fn();
    const { getByPlaceholderText } = await render(
      <TextArea value="" onChangeText={onChangeText} placeholder="t" />,
    );
    fireEvent.changeText(getByPlaceholderText("t"), "hi");
    expect(onChangeText).toHaveBeenCalledWith("hi");
  });

  it("renders error", async () => {
    const { getByText } = await render(
      <TextArea value="" onChangeText={jest.fn()} error="Too short" />,
    );
    expect(getByText("Too short")).toBeTruthy();
  });
});
