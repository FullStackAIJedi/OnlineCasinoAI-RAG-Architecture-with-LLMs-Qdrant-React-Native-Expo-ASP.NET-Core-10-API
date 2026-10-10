import { Tabs, useRouter } from "expo-router";
import { Ionicons } from "@expo/vector-icons";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { useTheme } from "../../../src/theme/ThemeProvider";

export default function TabsLayout() {
const theme = useTheme();
const insets = useSafeAreaInsets();

  return (
    <Tabs
      screenOptions={{
        headerShown: true,
        headerStyle: { backgroundColor: theme.card },
        headerTintColor: theme.text,
        tabBarStyle: {
          position: "absolute",
          left: 0,
          right: 0,
          bottom: 0,
          height: 64 + insets.bottom,   
          backgroundColor: "rgba(12, 1, 21, 0.85)",  
          borderTopWidth: 0,
          elevation: 0,
          shadowOpacity: 0,
          paddingBottom: insets.bottom + 10,
          paddingTop: 8,
        },
        tabBarActiveTintColor: theme.warning,
        tabBarInactiveTintColor: theme.clean,
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
            title: "Games",
            tabBarIcon: ({ color, size }) => <Ionicons name="game-controller" color={color} size={size} />,
        }}
        />
        <Tabs.Screen
        name="wallet"
        options={{
            title: "Wallet",
            tabBarIcon: ({ color, size }) => <Ionicons name="wallet" color={color} size={size} />,
        }}
        />
        <Tabs.Screen
        name="history"
        options={{
            title: "History",
            tabBarIcon: ({ color, size }) => <Ionicons name="time" color={color} size={size} />,
        }}
        />
        <Tabs.Screen
        name="profile"
        options={{
            title: "Profile",
            tabBarIcon: ({ color, size }) => <Ionicons name="person" color={color} size={size} />,
        }}
        />
    </Tabs>
  );
}
