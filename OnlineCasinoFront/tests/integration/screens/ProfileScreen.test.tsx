import { fireEvent, waitFor } from "@testing-library/react-native";

import { authApi } from "../../../src/api/auth";
import { usersApi } from "../../../src/api/users";
import ProfileScreen from "@/app/(tabs)/profile";
import { makeUserProfile, renderWithProviders } from "../../helpers";

jest.mock("../../../src/api/users");
jest.mock("../../../src/api/auth");

describe("ProfileScreen", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (usersApi.getMe as jest.Mock).mockResolvedValue(
      makeUserProfile({ nickName: "Alice", email: "a@b.com" }),
    );
    (authApi.logout as jest.Mock).mockResolvedValue({});
  });

  it("shows nickname", async () => {
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => expect(getByText("Alice")).toBeTruthy());
  });

  it("shows email", async () => {
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => expect(getByText("a@b.com")).toBeTruthy());
  });

  it("renders Personal data section", async () => {
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => expect(getByText("Personal data")).toBeTruthy());
  });

  it("renders Security section", async () => {
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => expect(getByText("Security")).toBeTruthy());
  });

  it("renders Settings section", async () => {
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => expect(getByText("Settings")).toBeTruthy());
  });

  it("opens change password modal", async () => {
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => getByText("Change password"));
    fireEvent.press(getByText("Change password"));
    await waitFor(() => expect(getByText("Current password")).toBeTruthy());
  });

  it("opens logout modal", async () => {
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => getByText("Logout"));
    fireEvent.press(getByText("Logout"));
    await waitFor(() => expect(getByText("Ready to log out?")).toBeTruthy());
  });

  it("saves profile", async () => {
    (usersApi.updateMe as jest.Mock).mockResolvedValue(makeUserProfile());
    const { getByText } = await renderWithProviders(<ProfileScreen />);
    await waitFor(() => getByText("Save"));
    fireEvent.press(getByText("Save"));
    await waitFor(() => expect(usersApi.updateMe).toHaveBeenCalled());
  });
});
