import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderOptions } from "@testing-library/react-native";
import type { ReactElement, ReactNode } from "react";
import { SafeAreaProvider } from "react-native-safe-area-context";

export const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: Infinity, staleTime: Infinity },
      mutations: { retry: false },
    },
  });

interface Options extends Omit<RenderOptions, "wrapper"> {
  queryClient?: QueryClient;
}

export const renderWithProviders = async (
  ui: ReactElement,
  { queryClient = createTestQueryClient(), ...options }: Options = {},
) => {
  const Wrapper = ({ children }: { children: ReactNode }) => (
    <SafeAreaProvider
      initialMetrics={{
        frame: { x: 0, y: 0, width: 0, height: 0 },
        insets: { top: 0, left: 0, right: 0, bottom: 0 },
      }}
    >
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </SafeAreaProvider>
  );

  const result = await render(ui, { wrapper: Wrapper, ...options });
  return { ...result, queryClient };
};
