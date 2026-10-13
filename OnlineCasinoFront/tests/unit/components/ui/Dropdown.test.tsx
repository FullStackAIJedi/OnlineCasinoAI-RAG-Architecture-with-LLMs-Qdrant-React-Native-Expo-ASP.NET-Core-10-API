// import { fireEvent, render } from "@testing-library/react-native";

// import { Dropdown } from "../../../../src/components/ui/Dropdown";

// jest.mock("../../../../src/theme/ThemeProvider", () => ({
//   useTheme: () => ({ card: "#111", text: "#fff", border: "#ccc" }),
// }));

// jest.mock("@expo/vector-icons", () => {
//   const React = require("react");
//   const { Text } = require("react-native");
//   return {
//     Ionicons: (props: any) => React.createElement(Text, props, "[Ionicons]"),
//   };
// });

// const options = [
//   { label: "Red", value: "red" },
//   { label: "Black", value: "black" },
// ];

// describe("Dropdown", () => {
//   it("shows placeholder when no selection", async () => {
//     const { getByText } = await render(
//       <Dropdown
//         label="Color"
//         options={options}
//         value=""
//         onChange={jest.fn()}
//       />,
//     );
//     expect(getByText("Choose...")).toBeTruthy();
//   });

//   it("shows selected label", async () => {
//     const { getByText } = await render(
//       <Dropdown
//         label="Color"
//         options={options}
//         value="red"
//         onChange={jest.fn()}
//       />,
//     );
//     expect(getByText("Red")).toBeTruthy();
//   });

//   it("renders label", async () => {
//     const { getByText } = await render(
//       <Dropdown
//         label="Color"
//         options={options}
//         value=""
//         onChange={jest.fn()}
//       />,
//     );
//     expect(getByText("Color")).toBeTruthy();
//   });

//   it("opens list on press", async () => {
//     const { getByText, getAllByText } = await render(
//       <Dropdown
//         label="Color"
//         options={options}
//         value="red"
//         onChange={jest.fn()}
//       />,
//     );
//     fireEvent.press(getByText("Red"));
//     expect(getAllByText("Black").length).toBeGreaterThan(0);
//   });

//   it("calls onChange when option picked", async () => {
//     const onChange = jest.fn();
//     const { getByText, getAllByText } = await render(
//       <Dropdown
//         label="Color"
//         options={options}
//         value="red"
//         onChange={onChange}
//       />,
//     );
//     fireEvent.press(getByText("Red"));
//     const items = getAllByText("Black");
//     fireEvent.press(items[items.length - 1]);
//     expect(onChange).toHaveBeenCalledWith("black");
//   });

//   it("uses custom placeholder", async () => {
//     const { getByText } = await render(
//       <Dropdown
//         label="Color"
//         options={options}
//         value=""
//         onChange={jest.fn()}
//         placeholder="Pick one"
//       />,
//     );
//     expect(getByText("Pick one")).toBeTruthy();
//   });

//   it("does not show list initially", async () => {
//     const { queryByText } = await render(
//       <Dropdown
//         label="Color"
//         options={options}
//         value="red"
//         onChange={jest.fn()}
//       />,
//     );
//     expect(queryByText("Black")).toBeNull();
//   });
// });

import {
  cleanup,
  fireEvent,
  render,
  waitFor,
} from "@testing-library/react-native";

import { Dropdown } from "../../../../src/components/ui/Dropdown";

jest.mock("../../../../src/theme/ThemeProvider", () => ({
  useTheme: () => ({ card: "#111", text: "#fff", border: "#ccc" }),
}));

jest.mock("@expo/vector-icons", () => {
  const React = require("react");
  const { Text } = require("react-native");
  return {
    Ionicons: (props: Record<string, unknown>) =>
      React.createElement(Text, props, "[Ionicons]"),
  };
});

const options = [
  { label: "Red", value: "red" },
  { label: "Black", value: "black" },
];

afterEach(() => {
  cleanup();
  jest.clearAllMocks();
});

describe("Dropdown", () => {
  it("shows placeholder when no selection", async () => {
    const { getByText } = await render(
      <Dropdown
        label="Color"
        options={options}
        value=""
        onChange={jest.fn()}
      />,
    );
    expect(getByText("Choose...")).toBeTruthy();
  });

  it("shows selected label", async () => {
    const { getByText } = await render(
      <Dropdown
        label="Color"
        options={options}
        value="red"
        onChange={jest.fn()}
      />,
    );
    expect(getByText("Red")).toBeTruthy();
  });

  it("renders label", async () => {
    const { getByText } = await render(
      <Dropdown
        label="Color"
        options={options}
        value=""
        onChange={jest.fn()}
      />,
    );
    expect(getByText("Color")).toBeTruthy();
  });

  it("opens list on press", async () => {
    const { getByText, getAllByText } = await render(
      <Dropdown
        label="Color"
        options={options}
        value="red"
        onChange={jest.fn()}
      />,
    );
    await fireEvent.press(getByText("Red"));
    await waitFor(() =>
      expect(getAllByText("Black").length).toBeGreaterThan(0),
    );
  });

  it("calls onChange when option picked", async () => {
    const onChange = jest.fn();
    const { getByText, getAllByText } = await render(
      <Dropdown
        label="Color"
        options={options}
        value="red"
        onChange={onChange}
      />,
    );
    await fireEvent.press(getByText("Red"));
    await waitFor(() => getAllByText("Black"));
    const items = getAllByText("Black");
    await fireEvent.press(items[items.length - 1]);
    expect(onChange).toHaveBeenCalledWith("black");
  });

  it("uses custom placeholder", async () => {
    const { getByText } = await render(
      <Dropdown
        label="Color"
        options={options}
        value=""
        onChange={jest.fn()}
        placeholder="Pick one"
      />,
    );
    expect(getByText("Pick one")).toBeTruthy();
  });

  it("does not show list initially", async () => {
    const { queryByText } = await render(
      <Dropdown
        label="Color"
        options={options}
        value="red"
        onChange={jest.fn()}
      />,
    );
    expect(queryByText("Black")).toBeNull();
  });
});
