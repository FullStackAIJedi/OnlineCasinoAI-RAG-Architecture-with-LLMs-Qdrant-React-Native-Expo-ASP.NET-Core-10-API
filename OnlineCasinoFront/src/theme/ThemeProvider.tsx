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

export const useTheme = () => useContext(ThemeContext).theme;

export const useThemeMode = () => {
  const { mode, setMode } = useContext(ThemeContext);
  return { mode, setMode };
};

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const systemScheme = useColorScheme();
  const [mode, setModeState] = useState<ThemeMode>(
    systemScheme === "dark" ? "dark" : "light"
  );

  useEffect(() => {
    (async () => {
      try {
        const saved = await SecureStore.getItemAsync(STORAGE_KEY);
        if (saved === "light" || saved === "dark") {
          setModeState(saved);
        }
      } catch { 
      }
    })();
  }, []);

  const setMode = async (newMode: ThemeMode) => {
    setModeState(newMode);
    try {
      await SecureStore.setItemAsync(STORAGE_KEY, newMode);
    } catch { 
    }
  };

  const theme = mode === "dark" ? darkColors : lightColors;

  return (
    <ThemeContext.Provider value={{ theme, mode, setMode }}>
      {children}
    </ThemeContext.Provider>
  );
}
