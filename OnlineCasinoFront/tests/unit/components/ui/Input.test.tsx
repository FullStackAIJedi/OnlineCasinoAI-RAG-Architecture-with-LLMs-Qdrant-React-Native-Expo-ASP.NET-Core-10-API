import { fireEvent, render } from "@testing-library/react-native";
import { Input } from "../../../../src/components/ui/Input";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({ card: "#111", text: "#fff", danger: "#f00" }),
}));

describe("Input", () => {
  it("renders placeholder", async () => {
    const { getByPlaceholderText } = await render(
      <Input value="" onChangeText={jest.fn()} placeholder="Email" />,
    );
    expect(getByPlaceholderText("Email")).toBeTruthy();
  });

  it("renders label", async () => {
    const { getByText } = await render(
      <Input value="" onChangeText={jest.fn()} label="Name" />,
    );
    expect(getByText("Name")).toBeTruthy();
  });

  it("calls onChangeText", async () => {
    const onChangeText = jest.fn();
    const { getByPlaceholderText } = await render(
      <Input value="" onChangeText={onChangeText} placeholder="Email" />,
    );
    fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
    expect(onChangeText).toHaveBeenCalledWith("a@b.com");
  });

  it("renders error", async () => {
    const { getByText } = await render(
      <Input value="" onChangeText={jest.fn()} error="Required" />,
    );
    expect(getByText("Required")).toBeTruthy();
  });

  it("respects secureTextEntry", async () => {
    const { getByPlaceholderText } = await render(
      <Input
        value=""
        onChangeText={jest.fn()}
        placeholder="Pwd"
        secureTextEntry
      />,
    );
    expect(getByPlaceholderText("Pwd").props.secureTextEntry).toBe(true);
  });

  it("renders placeholder", async () => {
    const { getByPlaceholderText } = await render(
      <Input value="" onChangeText={jest.fn()} placeholder="Enter name" />,
    );
    expect(getByPlaceholderText("Enter name")).toBeTruthy();
  });

  it("renders error message when error prop provided", async () => {
    const { getByText } = await render(
      <Input value="" onChangeText={jest.fn()} error="Required field" />,
    );
    expect(getByText("Required field")).toBeTruthy();
  });

  it("does not render error when no error prop", async () => {
    const { queryByText } = await render(
      <Input value="" onChangeText={jest.fn()} />,
    );
    expect(queryByText("Required field")).toBeNull();
  });

  it("calls onChangeText with typed value", async () => {
    const onChangeText = jest.fn();
    const { getByDisplayValue } = await render(
      <Input value="old" onChangeText={onChangeText} />,
    );
    fireEvent.changeText(getByDisplayValue("old"), "new");
    expect(onChangeText).toHaveBeenCalledWith("new");
  });
});
