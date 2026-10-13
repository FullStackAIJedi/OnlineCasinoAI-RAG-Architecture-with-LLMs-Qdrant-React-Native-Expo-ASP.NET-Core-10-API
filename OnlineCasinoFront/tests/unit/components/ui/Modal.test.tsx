import { fireEvent, render } from "@testing-library/react-native";
import { Text } from "react-native";
import { Modal } from "../../../../src/components/ui/Modal";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({
    success: "#0f0",
    danger: "#f00",
    warning: "#fa0",
    border: "#ccc",
    card: "#111",
    overlay: "rgba(0,0,0,0.5)",
  }),
}));

describe("Modal", () => {
  it("renders children when visible", async () => {
    const { getByText } = await render(
      <Modal visible onClose={jest.fn()}>
        <Text>Content</Text>
      </Modal>,
    );
    expect(getByText("Content")).toBeTruthy();
  });

  it("renders title", async () => {
    const { getByText } = await render(
      <Modal visible onClose={jest.fn()} title="Title">
        <Text>X</Text>
      </Modal>,
    );
    expect(getByText("Title")).toBeTruthy();
  });

  it("does not render when not visible", async () => {
    const { queryByText } = await render(
      <Modal visible={false} onClose={jest.fn()} title="Title">
        <Text>Content</Text>
      </Modal>,
    );
    expect(queryByText("Content")).toBeNull();
  });
});
