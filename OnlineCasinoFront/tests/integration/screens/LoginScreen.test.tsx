import { cleanup, fireEvent, waitFor } from "@testing-library/react-native";

import { authApi } from "@/api/auth";
import { renderWithProviders } from "../../helpers";
import LoginScreen from "@/app/login";

jest.mock("@/api/auth");

const mockPush = jest.fn();
jest.mock("expo-router", () => {
  const React = require("react");
  const { Text, View } = require("react-native");

  return {
    useRouter: () => ({
      push: mockPush,
      replace: jest.fn(),
      back: jest.fn(),
    }),
    useLocalSearchParams: () => ({}),
    useFocusEffect: (cb: () => void) => {
      React.useEffect(cb, [cb]);
    },
    Redirect: ({ href }: { href: string }) =>
      React.createElement(Text, null, href),
    Link: ({ children }: { children: React.ReactNode }) =>
      React.createElement(View, null, children),
    Stack: ({ children }: { children: React.ReactNode }) =>
      React.createElement(View, null, children),
    Tabs: ({ children }: { children: React.ReactNode }) =>
      React.createElement(View, null, children),
  };
});

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

describe("LoginScreen", () => {
  beforeEach(() => {
    (authApi.login as jest.Mock).mockResolvedValue({ message: "OK" });
  });

  it("renders title", async () => {
    const { getByText } = await renderWithProviders(<LoginScreen />);
    expect(getByText("Casino")).toBeTruthy();
  });

  it("renders email + password inputs", async () => {
    const { getByPlaceholderText } = await renderWithProviders(<LoginScreen />);
    expect(getByPlaceholderText("Email")).toBeTruthy();
    expect(getByPlaceholderText("Password")).toBeTruthy();
  });

  it("renders sign in button", async () => {
    const { getAllByText } = await renderWithProviders(<LoginScreen />);
    const signInElements = getAllByText("Sign in");
    expect(signInElements.length).toBeGreaterThanOrEqual(1);
  });

  it("shows error when email empty", async () => {
    const { getAllByText, getByText } = await renderWithProviders(
      <LoginScreen />,
    );
    const signInButtons = getAllByText("Sign in");
    await fireEvent.press(signInButtons[signInButtons.length - 1]);
    await waitFor(() => expect(getByText("Email is required")).toBeTruthy());
  });

  it("shows error when email invalid", async () => {
    const { getAllByText, getByText, getByPlaceholderText } =
      await renderWithProviders(<LoginScreen />);

    await fireEvent.changeText(getByPlaceholderText("Email"), "invalid");

    const signInButtons = getAllByText("Sign in");
    await fireEvent.press(signInButtons[signInButtons.length - 1]);

    await waitFor(() => expect(getByText("Invalid email")).toBeTruthy());
  });

  it("shows error when password too short", async () => {
    const { getAllByText, getByText, getByPlaceholderText } =
      await renderWithProviders(<LoginScreen />);

    await fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
    await fireEvent.changeText(getByPlaceholderText("Password"), "123");

    const signInButtons = getAllByText("Sign in");
    await fireEvent.press(signInButtons[signInButtons.length - 1]);

    await waitFor(() => expect(getByText("Minimum 6 characters")).toBeTruthy());
  });

  it("calls login on valid input", async () => {
    const { getAllByText, getByPlaceholderText } = await renderWithProviders(
      <LoginScreen />,
    );

    await fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
    await fireEvent.changeText(getByPlaceholderText("Password"), "123456");

    const signInButtons = getAllByText("Sign in");
    await fireEvent.press(signInButtons[signInButtons.length - 1]);

    await waitFor(() =>
      expect(authApi.login).toHaveBeenCalledWith({
        email: "a@b.com",
        password: "123456",
      }),
    );
  });

  it("clears field error on change", async () => {
    const { getAllByText, getByText, getByPlaceholderText, queryByText } =
      await renderWithProviders(<LoginScreen />);

    const signInButtons = getAllByText("Sign in");
    await fireEvent.press(signInButtons[signInButtons.length - 1]);

    await waitFor(() => expect(getByText("Email is required")).toBeTruthy());

    await fireEvent.changeText(getByPlaceholderText("Email"), "a");
    await waitFor(() => expect(queryByText("Email is required")).toBeNull());
  });

  it("shows server error on failure", async () => {
    (authApi.login as jest.Mock).mockRejectedValue(new Error("Bad creds"));

    const { getAllByText, getByText, getByPlaceholderText } =
      await renderWithProviders(<LoginScreen />);

    await fireEvent.changeText(getByPlaceholderText("Email"), "a@b.com");
    await fireEvent.changeText(getByPlaceholderText("Password"), "123456");

    const signInButtons = getAllByText("Sign in");
    await fireEvent.press(signInButtons[signInButtons.length - 1]);

    await waitFor(() => expect(getByText("Bad creds")).toBeTruthy());
  });

  it("navigates to register", async () => {
    const { getByText } = await renderWithProviders(<LoginScreen />);
    await fireEvent.press(getByText("No account? Sign up"));
    expect(mockPush).toHaveBeenCalledWith("/register");
  });
});
