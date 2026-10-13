import { renderHook } from "@testing-library/react-native";
import type { RenderHookOptions } from "@testing-library/react-native";
import type { ReactNode } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { SafeAreaProvider } from "react-native-safe-area-context";
import React from "react";

export const createWrapper = (qc: QueryClient) => {
  return ({ children }: { children: ReactNode }) =>
    React.createElement(
      SafeAreaProvider,
      {
        initialMetrics: {
          frame: { x: 0, y: 0, width: 0, height: 0 },
          insets: { top: 0, bottom: 0, left: 0, right: 0 },
        },
      },
      React.createElement(QueryClientProvider, { client: qc }, children),
    );
};

export async function renderHookAsync<T, P = unknown>(
  callback: (props: P) => T,
  options?: Omit<RenderHookOptions<P>, "wrapper"> & { wrapper?: unknown },
): Promise<{
  result: { current: T };
  rerender: (props?: P) => void;
  unmount: () => void;
}> {
  return (await renderHook(callback, options as never)) as unknown as {
    result: { current: T };
    rerender: (props?: P) => void;
    unmount: () => void;
  };
}
