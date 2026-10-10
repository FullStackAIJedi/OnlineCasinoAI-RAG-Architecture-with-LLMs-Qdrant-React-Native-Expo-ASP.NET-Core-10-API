import React, { createContext, useContext, useEffect, useState } from "react";
import { useColorScheme } from "react-native";
import * as SecureStore from "expo-secure-store";

import { darkColors, lightColors, Theme } from "./colors";

type ThemeMode = "light" | "dark";

interface ThemeContextValue {
  theme: Theme;
  mode: ThemeMode;
  setMode: (mode: ThemeMode) => void;
}

const ThemeContext = createContext<ThemeContextValue>({
  theme: lightColors,
  mode: "light",
  setMode: () => {},
});

const STORAGE_KEY = "theme_mode";

/**
 * Returns the current theme object.
 *
 * @returns The active theme (colors, spacing, etc.).
 */
export const useTheme = () => useContext(ThemeContext).theme;

/**
 * Returns the current theme mode and a setter.
 *
 * @returns An object with `mode` ("light" | "dark") and `setMode`.
 */
export const useThemeMode = () => {
  const { mode, setMode } = useContext(ThemeContext);
  return { mode, setMode };
};

/**
 * Provides the current theme and theme mode to the app.
 *
 * @param props - Component props.
 * @param props.children - The subtree that consumes the theme.
 * @returns The rendered theme provider.
 */
export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const systemScheme = useColorScheme();
  const [mode, setModeState] = useState<ThemeMode>(
    systemScheme === "dark" ? "dark" : "light",
  );

  useEffect(() => {
    (async () => {
      try {
        const saved = await SecureStore.getItemAsync(STORAGE_KEY);
        if (saved === "light" || saved === "dark") {
          setModeState(saved);
        }
      } catch {}
    })();
  }, []);

  const setMode = async (newMode: ThemeMode) => {
    setModeState(newMode);
    try {
      await SecureStore.setItemAsync(STORAGE_KEY, newMode);
    } catch {}
  };

  const theme = mode === "dark" ? darkColors : lightColors;

  return (
    <ThemeContext.Provider value={{ theme, mode, setMode }}>
      {children}
    </ThemeContext.Provider>
  );
}
