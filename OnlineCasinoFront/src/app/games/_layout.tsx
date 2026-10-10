import { Stack } from "expo-router";

/**
 * Games group layout.
 *
 * Access control is handled by `Stack.Protected` in the root layout,
 * so no additional guard is needed here.
 *
 * @returns The rendered games layout.
 */
export default function GamesLayout() {
  return <Stack screenOptions={{ headerShown: false }} />;
}
