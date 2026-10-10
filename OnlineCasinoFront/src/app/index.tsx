import { Redirect } from "expo-router";
import { useIsAuthenticated } from "../hooks/useAuth";

/**
 * Entry point.
 *
 * Redirects to the main app when authenticated, otherwise to login.
 *
 * @returns The rendered redirect.
 */
export default function Index() {
  const { data: isAuth, isPending } = useIsAuthenticated();

  if (isPending) return null;

  return <Redirect href={isAuth ? "/(tabs)" : "/login"} />;
}
