import { Stack } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { QueryClientProvider } from "@tanstack/react-query";

import { queryClient } from "../lib/queryClient";
import { ThemeProvider } from "../theme/ThemeProvider";
import { useUnauthorizedInvalidation } from "../hooks/useUnauthorizedInvalidation";
import { useIsAuthenticated } from "../hooks/useAuth";

/**
 * Navigation stack with auth-protected routes.
 *
 * Delegates access control to `expo-router` via `Stack.Protected`,
 * so route groups are only reachable when their guard condition is met.
 *
 * @returns The rendered app stack.
 */
function AppStack() {
  useUnauthorizedInvalidation();
  const { data: isAuth, isPending } = useIsAuthenticated();

  if (isPending) return null;

  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Protected guard={!!isAuth}>
        <Stack.Screen name="(tabs)" />
        <Stack.Screen name="games" />
      </Stack.Protected>

      <Stack.Protected guard={!isAuth}>
        <Stack.Screen name="login" />
        <Stack.Screen name="register" />
      </Stack.Protected>
    </Stack>
  );
}

/**
 * Root layout.
 *
 * Provides the QueryClient and theme around the navigation stack.
 *
 * @returns The root layout tree.
 */
export default function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <StatusBar style="auto" />
        <AppStack />
      </ThemeProvider>
    </QueryClientProvider>
  );
}
