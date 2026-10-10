import { useEffect } from "react";
import { useRouter, useSegments } from "expo-router";
import { useIsAuthenticated } from "./useAuth";

type AuthRoute = "/" | "/(tabs)" | null;

/**
 * Resolves the target route a user should be redirected to,
 * based on auth state and current route segments.
 *
 * @param isAuth - Whether the user is authenticated.
 * @param segments - The current route segments from expo-router.
 * @returns The redirect target, or null if no redirect is needed.
 */
function resolveAuthRedirect(
  isAuth: boolean,
  segments: readonly string[],
): AuthRoute {
  const root = segments[0];
  const inTabs = root === "(tabs)";
  const inGames = root === "games";
  const inAuthScreen = !root || root === "register";
  const inProtected = inTabs || inGames;

  if (!isAuth && inProtected) return "/";
  if (isAuth && inAuthScreen) return "/(tabs)";
  return null;
}

/**
 * Keeps navigation in sync with auth state:
 * - unauthenticated users in protected routes are redirected to home
 * - authenticated users on auth screens are redirected to tabs
 *
 * @returns An object with the current auth loading state.
 */
export function useAuthRedirect() {
  const { data: isAuth, isPending, isFetching } = useIsAuthenticated();
  const segments = useSegments();
  const router = useRouter();

  useEffect(() => {
    if (isPending || isFetching) return;

    const target = resolveAuthRedirect(!!isAuth, segments);
    if (target) router.replace(target);
  }, [isAuth, isPending, isFetching, segments, router]);

  return { isPending };
}
