// import { fireEvent, render } from "@testing-library/react-native";
// import { DatePicker } from "../../../../src/components/ui/DatePicker";

// jest.mock("../../../../src/theme/ThemeProvider", () => ({
//   useTheme: () => ({
//     text: "#fff",
//     textSecondary: "#aaa",
//     card: "#111",
//     border: "#ccc",
//   }),
// }));

// jest.mock("@react-native-community/datetimepicker", () => {
//   const React = require("react");
//   const { Text } = require("react-native");
//   return {
//     __esModule: true,
//     default: ({ onChange }: any) =>
//       React.createElement(
//         Text,
//         {
//           testID: "dtp",
//           onPress: () => onChange({}, new Date(2024, 0, 1)),
//         },
//         "Picker",
//       ),
//   };
// });

// describe("DatePicker", () => {
//   it("renders label", async () => {
//     const { getByText } = await render(
//       <DatePicker label="Birthday" value={new Date()} onChange={jest.fn()} />,
//     );
//     expect(getByText("Birthday")).toBeTruthy();
//   });

//   it("opens picker on press", async () => {
//     const { getByText } = await render(
//       <DatePicker label="B" value={new Date()} onChange={jest.fn()} />,
//     );
//     const trigger = getByText(new Date().toLocaleDateString());
//     fireEvent.press(trigger);
//     expect(getByText("Picker")).toBeTruthy();
//   });

//   it("calls onChange with new date", async () => {
//     const onChange = jest.fn();
//     const { getByText } = await render(
//       <DatePicker label="B" value={new Date()} onChange={onChange} />,
//     );
//     fireEvent.press(getByText(new Date().toLocaleDateString()));
//     fireEvent.press(getByText("Picker"));
//     expect(onChange).toHaveBeenCalled();
//   });
// });

import React from "react";
import {
  cleanup,
  fireEvent,
  render,
  waitFor,
} from "@testing-library/react-native";

import { DatePicker } from "../../../../src/components/ui/DatePicker";

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

jest.mock("@react-native-community/datetimepicker", () => {
  const React = require("react");
  const { Text, TouchableOpacity } = require("react-native");
  const DateTimePicker = (props: {
    onChange?: (event: unknown, date?: Date) => void;
  }) =>
    React.createElement(
      TouchableOpacity,
      {
        onPress: () => props.onChange?.({}, new Date(2026, 0, 15)),
      },
      React.createElement(Text, null, "Picker"),
    );
  return {
    __esModule: true,
    default: DateTimePicker,
    DateTimePickerAndroid: { open: jest.fn() },
  };
});

afterEach(() => {
  cleanup();
  jest.clearAllMocks();
});

describe("DatePicker", () => {
  it("opens picker on press", async () => {
    const { getByText } = await render(
      <DatePicker label="Date" value={new Date()} onChange={jest.fn()} />,
    );
    const trigger = getByText(new Date().toLocaleDateString());
    await fireEvent.press(trigger);
    await waitFor(() => expect(getByText("Picker")).toBeTruthy());
  });

  it("calls onChange with new date", async () => {
    const onChange = jest.fn();
    const { getByText } = await render(
      <DatePicker label="Date" value={new Date()} onChange={onChange} />,
    );
    await fireEvent.press(getByText(new Date().toLocaleDateString()));
    await waitFor(() => getByText("Picker"));
    await fireEvent.press(getByText("Picker"));
    expect(onChange).toHaveBeenCalled();
  });
});
