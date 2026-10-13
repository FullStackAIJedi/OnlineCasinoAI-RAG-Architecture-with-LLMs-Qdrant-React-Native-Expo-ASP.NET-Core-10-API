// import { fireEvent, waitFor } from "@testing-library/react-native";

// import { authApi } from "@/api/auth";
// import RegisterScreen from "@/app/register";
// import { renderWithProviders } from "../../helpers";

// jest.mock("@/api/auth");

// const mockBack = jest.fn();
// jest.mock("expo-router", () => ({
//   useRouter: () => ({ back: mockBack, push: jest.fn() }),
// }));

// jest.mock("react-native-safe-area-context", () => {
//   const React = require("react");
//   const { View } = require("react-native");
//   return {
//     useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
//     SafeAreaProvider: ({ children }: any) =>
//       React.createElement(View, null, children),
//     SafeAreaView: ({ children, ...props }: any) =>
//       React.createElement(View, props, children),
//   };
// });

// describe("RegisterScreen", () => {
//   beforeEach(() => {
//     jest.clearAllMocks();
//     (authApi.register as jest.Mock).mockResolvedValue({ message: "OK" });
//   });

//   it("renders sign up title", async () => {
//     const { getAllByText } = await renderWithProviders(<RegisterScreen />);
//     const titles = getAllByText("Sign up");
//     expect(titles.length).toBeGreaterThanOrEqual(1);
//   });

//   it("renders all inputs", async () => {
//     const { getByPlaceholderText } = await renderWithProviders(
//       <RegisterScreen />,
//     );
//     expect(getByPlaceholderText("Email")).toBeTruthy();
//     expect(getByPlaceholderText("Nickname")).toBeTruthy();
//     expect(getByPlaceholderText("Password")).toBeTruthy();
//   });

//   it("shows error when fields empty", async () => {
//     const { getAllByText, getByText } = await renderWithProviders(
//       <RegisterScreen />,
//     );
//     const signUpButtons = getAllByText("Sign up");
//     fireEvent.press(signUpButtons[signUpButtons.length - 1]);
//     await waitFor(() =>
//       expect(getByText("Please fill in all fields.")).toBeTruthy(),
//     );
//   });

//   it("calls register on valid input", async () => {
//     const { getAllByText, getByPlaceholderText } = await renderWithProviders(
//       <RegisterScreen />,
//     );

//     await waitFor(() => getByPlaceholderText("Email"));

//     fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
//     fireEvent.changeText(getByPlaceholderText("Nickname"), "nick");
//     fireEvent.changeText(getByPlaceholderText("Password"), "pass");

//     const signUpButtons = getAllByText("Sign up");
//     fireEvent.press(signUpButtons[signUpButtons.length - 1]);

//     await waitFor(() =>
//       expect(authApi.register).toHaveBeenCalledWith({
//         email: "a@b.com",
//         nickName: "nick",
//         password: "pass",
//       }),
//     );
//   });

//   it("shows server error", async () => {
//     (authApi.register as jest.Mock).mockRejectedValue(new Error("Taken"));

//     const { getAllByText, getByText, getByPlaceholderText } =
//       await renderWithProviders(<RegisterScreen />);

//     await waitFor(() => getByPlaceholderText("Email"));

//     fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
//     fireEvent.changeText(getByPlaceholderText("Nickname"), "n");
//     fireEvent.changeText(getByPlaceholderText("Password"), "p");

//     const signUpButtons = getAllByText("Sign up");
//     fireEvent.press(signUpButtons[signUpButtons.length - 1]);

//     await waitFor(() => expect(getByText("Taken")).toBeTruthy());
//   });

//   it("navigates back", async () => {
//     const { getByText } = await renderWithProviders(<RegisterScreen />);
//     await waitFor(() => getByText("Have an account? Sign in"));
//     fireEvent.press(getByText("Have an account? Sign in"));
//     expect(mockBack).toHaveBeenCalled();
//   });
// });

import { cleanup, fireEvent, waitFor } from "@testing-library/react-native";

import { authApi } from "@/api/auth";
import { renderWithProviders } from "../../helpers";
import RegisterScreen from "@/app/register";

jest.mock("@/api/auth");

const mockBack = jest.fn();
jest.mock("expo-router", () => ({
  useRouter: () => ({ back: mockBack, push: jest.fn() }),
}));

jest.mock("react-native-safe-area-context", () => {
  const React = require("react");
  const { View } = require("react-native");
  return {
    useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
    SafeAreaProvider: ({ children }: { children: React.ReactNode }) =>
      React.createElement(View, null, children),
    SafeAreaView: ({
      children,
      ...props
    }: {
      children: React.ReactNode;
      [key: string]: unknown;
    }) => React.createElement(View, props, children),
  };
});

afterEach(() => {
  cleanup();
  jest.clearAllMocks();
});

describe("RegisterScreen", () => {
  beforeEach(() => {
    (authApi.register as jest.Mock).mockResolvedValue({ message: "OK" });
  });

  it("renders sign up title", async () => {
    const { getAllByText } = await renderWithProviders(<RegisterScreen />);
    const titles = getAllByText("Sign up");
    expect(titles.length).toBeGreaterThanOrEqual(1);
  });

  it("renders all inputs", async () => {
    const { getByPlaceholderText } = await renderWithProviders(
      <RegisterScreen />,
    );
    expect(getByPlaceholderText("Email")).toBeTruthy();
    expect(getByPlaceholderText("Nickname")).toBeTruthy();
    expect(getByPlaceholderText("Password")).toBeTruthy();
  });

  it("shows error when fields empty", async () => {
    const { getAllByText, getByText } = await renderWithProviders(
      <RegisterScreen />,
    );
    const signUpButtons = getAllByText("Sign up");
    await fireEvent.press(signUpButtons[signUpButtons.length - 1]);
    await waitFor(() =>
      expect(getByText("Please fill in all fields.")).toBeTruthy(),
    );
  });

  it("calls register on valid input", async () => {
    const { getAllByText, getByPlaceholderText } = await renderWithProviders(
      <RegisterScreen />,
    );

    await waitFor(() => getByPlaceholderText("Email"));

    await fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
    await fireEvent.changeText(getByPlaceholderText("Nickname"), "nick");
    await fireEvent.changeText(getByPlaceholderText("Password"), "pass");

    const signUpButtons = getAllByText("Sign up");
    await fireEvent.press(signUpButtons[signUpButtons.length - 1]);

    await waitFor(() =>
      expect(authApi.register).toHaveBeenCalledWith({
        email: "a@b.com",
        nickName: "nick",
        password: "pass",
      }),
    );
  });

  it("shows server error", async () => {
    (authApi.register as jest.Mock).mockRejectedValue(new Error("Taken"));

    const { getAllByText, getByText, getByPlaceholderText } =
      await renderWithProviders(<RegisterScreen />);

    await waitFor(() => getByPlaceholderText("Email"));

    await fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
    await fireEvent.changeText(getByPlaceholderText("Nickname"), "n");
    await fireEvent.changeText(getByPlaceholderText("Password"), "p");

    const signUpButtons = getAllByText("Sign up");
    await fireEvent.press(signUpButtons[signUpButtons.length - 1]);

    await waitFor(() => expect(getByText("Taken")).toBeTruthy());
  });

  it("navigates back", async () => {
    const { getByText } = await renderWithProviders(<RegisterScreen />);
    await waitFor(() => getByText("Have an account? Sign in"));
    await fireEvent.press(getByText("Have an account? Sign in"));
    expect(mockBack).toHaveBeenCalled();
  });
});
