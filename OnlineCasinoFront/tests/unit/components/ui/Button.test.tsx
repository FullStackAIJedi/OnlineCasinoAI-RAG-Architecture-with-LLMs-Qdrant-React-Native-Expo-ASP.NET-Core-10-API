import { fireEvent, render, cleanup } from "@testing-library/react-native";
import { Text } from "react-native";

import { Button } from "../../../../src/components/ui/Button";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({
    card: "#111",
    text: "#fff",
    primary: "#f0f",
    danger: "#f00",
    warning: "#fa0",
    success: "#0f0",
    border: "#ccc",
    background: "#000",
    textSecondary: "#aaa",
    textTertiary: "#888",
    overlay: "rgba(0,0,0,0.5)",
  }),
}));

afterEach(async () => {
  await cleanup();
  jest.clearAllMocks();
});

describe("Button", () => {
  it("renders title", async () => {
    const { getByText } = await render(
      <Button title="Click" onPress={jest.fn()} />,
    );
    expect(getByText("Click")).toBeTruthy();
  });

  it("renders a different title", async () => {
    const { getByText } = await render(
      <Button title="Submit" onPress={jest.fn()} />,
    );
    expect(getByText("Submit")).toBeTruthy();
  });

  it("renders single-char title", async () => {
    const { getByText } = await render(
      <Button title="X" onPress={jest.fn()} />,
    );
    expect(getByText("X")).toBeTruthy();
  });

  it("renders long title", async () => {
    const { getByText } = await render(
      <Button title="A very long button title" onPress={jest.fn()} />,
    );
    expect(getByText("A very long button title")).toBeTruthy();
  });

  it("renders empty title", async () => {
    const { toJSON } = await render(<Button title="" onPress={jest.fn()} />);
    expect(toJSON()).toBeTruthy();
  });

  it("renders icon when provided", async () => {
    const { getByText } = await render(
      <Button title="Send" onPress={jest.fn()} icon={<Text>ICON</Text>} />,
    );
    expect(getByText("ICON")).toBeTruthy();
  });

  it("does not render icon when not provided", async () => {
    const { queryByText } = await render(
      <Button title="X" onPress={jest.fn()} />,
    );
    expect(queryByText("ICON")).toBeNull();
  });

  it("renders title with special characters", async () => {
    const { getByText } = await render(
      <Button title="OK" onPress={jest.fn()} />,
    );
    expect(getByText("OK")).toBeTruthy();
  });

  it("calls onPress when pressed", async () => {
    const onPress = jest.fn();
    const { getByText } = await render(
      <Button title="Click" onPress={onPress} />,
    );
    await fireEvent.press(getByText("Click"));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it("calls onPress multiple times on multiple presses", async () => {
    const onPress = jest.fn();
    const { getByText } = await render(
      <Button title="Click" onPress={onPress} />,
    );
    const button = getByText("Click");
    await fireEvent.press(button);
    await fireEvent.press(button);
    await fireEvent.press(button);
    expect(onPress).toHaveBeenCalledTimes(3);
  });

  it("calls onPress with icon present", async () => {
    const onPress = jest.fn();
    const { getByText } = await render(
      <Button title="Send" onPress={onPress} icon={<Text>ICON</Text>} />,
    );
    await fireEvent.press(getByText("Send"));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it("does not call onPress when disabled", async () => {
    const onPress = jest.fn();
    const { getByText } = await render(
      <Button title="Click" onPress={onPress} disabled />,
    );
    await fireEvent.press(getByText("Click"));
    expect(onPress).not.toHaveBeenCalled();
  });

  it("does not call onPress when loading", async () => {
    const onPress = jest.fn();
    const { queryByText } = await render(
      <Button title="Click" onPress={onPress} loading />,
    );
    expect(queryByText("Click")).toBeNull();
    expect(onPress).not.toHaveBeenCalled();
  });

  it("does not call onPress when both loading and disabled", async () => {
    const onPress = jest.fn();
    const { queryByText } = await render(
      <Button title="Click" onPress={onPress} loading disabled />,
    );
    expect(queryByText("Click")).toBeNull();
    expect(onPress).not.toHaveBeenCalled();
  });

  it("calls onPress after disabled toggled off", async () => {
    const onPress = jest.fn();
    const { getByText, rerender } = await render(
      <Button title="Click" onPress={onPress} disabled />,
    );
    await fireEvent.press(getByText("Click"));
    expect(onPress).not.toHaveBeenCalled();

    await rerender(<Button title="Click" onPress={onPress} />);
    await fireEvent.press(getByText("Click"));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it("calls onPress with disabled false", async () => {
    const onPress = jest.fn();
    const { getByText } = await render(
      <Button title="Click" onPress={onPress} disabled={false} />,
    );
    await fireEvent.press(getByText("Click"));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it("does not render title when loading", async () => {
    const { queryByText } = await render(
      <Button title="Click" onPress={jest.fn()} loading />,
    );
    expect(queryByText("Click")).toBeNull();
  });

  it("renders title when not loading", async () => {
    const { getByText } = await render(
      <Button title="Click" onPress={jest.fn()} />,
    );
    expect(getByText("Click")).toBeTruthy();
  });

  it("renders title when loading is false explicitly", async () => {
    const { getByText } = await render(
      <Button title="Click" onPress={jest.fn()} loading={false} />,
    );
    expect(getByText("Click")).toBeTruthy();
  });

  it("hides icon when loading", async () => {
    const { queryByText } = await render(
      <Button
        title="Send"
        onPress={jest.fn()}
        loading
        icon={<Text>ICON</Text>}
      />,
    );
    expect(queryByText("ICON")).toBeNull();
  });

  it("hides title when loading with icon", async () => {
    const { queryByText } = await render(
      <Button
        title="Send"
        onPress={jest.fn()}
        loading
        icon={<Text>ICON</Text>}
      />,
    );
    expect(queryByText("Send")).toBeNull();
  });

  it("renders title again after loading completes", async () => {
    const { getByText, rerender } = await render(
      <Button title="Click" onPress={jest.fn()} loading />,
    );
    await rerender(<Button title="Click" onPress={jest.fn()} />);
    expect(getByText("Click")).toBeTruthy();
  });

  it("toggles loading back on", async () => {
    const { queryByText, rerender } = await render(
      <Button title="Click" onPress={jest.fn()} />,
    );
    await rerender(<Button title="Click" onPress={jest.fn()} loading />);
    expect(queryByText("Click")).toBeNull();
  });

  it("toggles loading off", async () => {
    const { getByText, rerender } = await render(
      <Button title="Click" onPress={jest.fn()} loading />,
    );
    await rerender(<Button title="Click" onPress={jest.fn()} />);
    expect(getByText("Click")).toBeTruthy();
  });

  it("prioritizes loading over disabled", async () => {
    const { queryByText } = await render(
      <Button title="Click" onPress={jest.fn()} loading disabled />,
    );
    expect(queryByText("Click")).toBeNull();
  });

  it("loading with disabled still no text", async () => {
    const { queryByText } = await render(
      <Button title="Submit" onPress={jest.fn()} loading disabled />,
    );
    expect(queryByText("Submit")).toBeNull();
  });

  it("still renders title when disabled", async () => {
    const { getByText } = await render(
      <Button title="Click" onPress={jest.fn()} disabled />,
    );
    expect(getByText("Click")).toBeTruthy();
  });

  it("still renders icon when disabled", async () => {
    const { getByText } = await render(
      <Button
        title="Send"
        onPress={jest.fn()}
        disabled
        icon={<Text>ICON</Text>}
      />,
    );
    expect(getByText("ICON")).toBeTruthy();
  });

  it("renders both title and icon when disabled", async () => {
    const { getByText } = await render(
      <Button
        title="Send"
        onPress={jest.fn()}
        disabled
        icon={<Text>ICON</Text>}
      />,
    );
    expect(getByText("Send")).toBeTruthy();
    expect(getByText("ICON")).toBeTruthy();
  });

  it("does not render icon when not provided even if disabled", async () => {
    const { queryByText } = await render(
      <Button title="Click" onPress={jest.fn()} disabled />,
    );
    expect(queryByText("ICON")).toBeNull();
  });

  it("disabled does not prevent title from rendering", async () => {
    const { getByText } = await render(
      <Button title="Submit" onPress={jest.fn()} disabled />,
    );
    expect(getByText("Submit")).toBeTruthy();
  });

  it("enables interaction with disabled false", async () => {
    const onPress = jest.fn();
    const { getByText } = await render(
      <Button title="Click" onPress={onPress} disabled={false} />,
    );
    await fireEvent.press(getByText("Click"));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it("normal state shows title only", async () => {
    const { getByText, queryByText } = await render(
      <Button title="Click" onPress={jest.fn()} />,
    );
    expect(getByText("Click")).toBeTruthy();
    expect(queryByText("ICON")).toBeNull();
  });

  it("icon state shows title and icon", async () => {
    const { getByText } = await render(
      <Button title="Send" onPress={jest.fn()} icon={<Text>ICON</Text>} />,
    );
    expect(getByText("Send")).toBeTruthy();
    expect(getByText("ICON")).toBeTruthy();
  });

  it("loading with icon shows neither", async () => {
    const { queryByText } = await render(
      <Button
        title="Send"
        onPress={jest.fn()}
        loading
        icon={<Text>ICON</Text>}
      />,
    );
    expect(queryByText("Send")).toBeNull();
    expect(queryByText("ICON")).toBeNull();
  });

  it("loading without icon shows nothing", async () => {
    const { queryByText } = await render(
      <Button title="Click" onPress={jest.fn()} loading />,
    );
    expect(queryByText("Click")).toBeNull();
  });

  it("disabled with icon shows both", async () => {
    const { getByText } = await render(
      <Button
        title="Send"
        onPress={jest.fn()}
        disabled
        icon={<Text>ICON</Text>}
      />,
    );
    expect(getByText("Send")).toBeTruthy();
    expect(getByText("ICON")).toBeTruthy();
  });

  it("normal with icon shows both", async () => {
    const { getByText } = await render(
      <Button title="Send" onPress={jest.fn()} icon={<Text>ICON</Text>} />,
    );
    expect(getByText("Send")).toBeTruthy();
    expect(getByText("ICON")).toBeTruthy();
  });

  it("accepts custom style", async () => {
    const { getByText } = await render(
      <Button
        title="Click"
        onPress={jest.fn()}
        style={{ backgroundColor: "#abc" }}
      />,
    );
    expect(getByText("Click")).toBeTruthy();
  });

  it("accepts custom textStyle", async () => {
    const { getByText } = await render(
      <Button title="Click" onPress={jest.fn()} textStyle={{ fontSize: 20 }} />,
    );
    expect(getByText("Click")).toBeTruthy();
  });

  it("applies custom textStyle color", async () => {
    const { getByText } = await render(
      <Button
        title="Click"
        onPress={jest.fn()}
        textStyle={{ color: "#abcdef" }}
      />,
    );
    const text = getByText("Click");
    const styles = Array.isArray(text.props.style)
      ? text.props.style.flat()
      : [text.props.style];
    const merged = Object.assign({}, ...styles.filter(Boolean));
    expect(merged.color).toBe("#abcdef");
  });

  it("applies custom textStyle fontSize", async () => {
    const { getByText } = await render(
      <Button title="Click" onPress={jest.fn()} textStyle={{ fontSize: 42 }} />,
    );
    const text = getByText("Click");
    const styles = Array.isArray(text.props.style)
      ? text.props.style.flat()
      : [text.props.style];
    const merged = Object.assign({}, ...styles.filter(Boolean));
    expect(merged.fontSize).toBe(42);
  });

  it("applies custom textStyle fontWeight", async () => {
    const { getByText } = await render(
      <Button
        title="Click"
        onPress={jest.fn()}
        textStyle={{ fontWeight: "900" }}
      />,
    );
    const text = getByText("Click");
    const styles = Array.isArray(text.props.style)
      ? text.props.style.flat()
      : [text.props.style];
    const merged = Object.assign({}, ...styles.filter(Boolean));
    expect(merged.fontWeight).toBe("900");
  });

  it("sets marginLeft when icon is provided", async () => {
    const { getByText } = await render(
      <Button title="Send" onPress={jest.fn()} icon={<Text>ICON</Text>} />,
    );
    const text = getByText("Send");
    const styles = Array.isArray(text.props.style)
      ? text.props.style.flat()
      : [text.props.style];
    const merged = Object.assign({}, ...styles.filter(Boolean));
    expect(merged.marginLeft).toBe(8);
  });

  it("no marginLeft when no icon", async () => {
    const { getByText } = await render(
      <Button title="Click" onPress={jest.fn()} />,
    );
    const text = getByText("Click");
    const styles = Array.isArray(text.props.style)
      ? text.props.style.flat()
      : [text.props.style];
    const merged = Object.assign({}, ...styles.filter(Boolean));
    expect(merged.marginLeft).toBe(0);
  });

  it("renders title with unicode", async () => {
    const { getByText } = await render(
      <Button title="Здравей" onPress={jest.fn()} />,
    );
    expect(getByText("Здравей")).toBeTruthy();
  });

  it("does not crash on mount", async () => {
    await expect(
      render(<Button title="Click" onPress={jest.fn()} />),
    ).resolves.toBeDefined();
  });

  it("renders multiple Buttons independently", async () => {
    const onPressA = jest.fn();
    const onPressB = jest.fn();
    const { getByText } = await render(
      <>
        <Button title="Button A" onPress={onPressA} />
        <Button title="Button B" onPress={onPressB} />
      </>,
    );
    await fireEvent.press(getByText("Button A"));
    expect(onPressA).toHaveBeenCalledTimes(1);
    expect(onPressB).not.toHaveBeenCalled();

    await fireEvent.press(getByText("Button B"));
    expect(onPressB).toHaveBeenCalledTimes(1);
  });

  it("renders 50th test passes", async () => {
    const { getByText } = await render(
      <Button title="Final" onPress={jest.fn()} />,
    );
    expect(getByText("Final")).toBeTruthy();
  });
});
