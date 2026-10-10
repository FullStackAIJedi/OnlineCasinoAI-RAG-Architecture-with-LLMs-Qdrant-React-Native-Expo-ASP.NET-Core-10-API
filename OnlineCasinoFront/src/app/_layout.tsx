import { Stack } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { QueryClientProvider } from "@tanstack/react-query";

import { queryClient } from "../lib/queryClient";
import { ThemeProvider } from "../theme/ThemeProvider";
import { AuthGuard } from "../components/auth/AuthGuard";

/**
 * Root layout.
 *
 * Provides the QueryClient, theme, and auth guard around the app's
 * navigation stack.
 *
 * @returns The root layout tree.
 */
export default function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <StatusBar style="auto" />
        <AuthGuard>
          <Stack screenOptions={{ headerShown: false }} />
        </AuthGuard>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
