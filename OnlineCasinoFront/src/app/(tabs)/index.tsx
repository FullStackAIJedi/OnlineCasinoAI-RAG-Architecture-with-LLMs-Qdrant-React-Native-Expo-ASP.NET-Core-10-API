import { ActivityIndicator, ImageBackground, ScrollView, StyleSheet, Text, View } from "react-native";
import { useRouter } from "expo-router";
import { SafeAreaView } from "react-native-safe-area-context";

import { useTheme } from "../../../src/theme/ThemeProvider";
import { GameCard } from "../../../src/components/GameCard";
import { GAMES } from "../../../src//utils/constants";

import { useWallet } from "../../../src/hooks/useWallet";

export default function HomeScreen() {
  const theme = useTheme();
  const router = useRouter();
  
  const { data: wallet, isLoading } = useWallet();

  return (
    <ImageBackground
      source={require("../../../assets/backgrounds/profile.jpg")}
      style={styles.background}
      resizeMode="cover"
    >
      <View style={styles.overlay} />

      <SafeAreaView style={styles.container} edges={["top"]}>
        <ScrollView contentContainerStyle={styles.scroll}>
          <View style={[ styles.balanceCard, { backgroundColor: theme.card, borderColor: theme.border }, ]} >
            {isLoading ? (
              <ActivityIndicator color={theme.primary} />
            ) : (
              <Text style={[styles.balance, { color: theme.text }]}>
                {wallet?.balance.toFixed(2) ?? "0.00"} {wallet?.currency ?? "EUR"}
              </Text>
            )}
          </View>

          <View style={styles.grid}>
            {GAMES.map((g) => (
              <GameCard
                key={g.code}
                code={g.code}
                name={g.name}
                image={g.image}
                onPress={() => router.push(g.route as any)}
              />
            ))}
          </View>
        </ScrollView>
      </SafeAreaView>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  background: { flex: 1, width: "100%", height: "100%" },
  overlay: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,   
  },
  container: { flex: 1 },
  scroll: { padding: 16, paddingBottom: 100 },
  balanceCard: {
    borderRadius: 19,
    padding: 20,
    marginBottom: 24,
    borderWidth: 3,
    alignItems: "center",
  },
  balanceLabel: { fontSize: 14, marginBottom: 8 },
  balance: { fontSize: 30, fontWeight: "900" },
  sectionTitle: { fontSize: 24, fontWeight: "800", marginBottom: 16 },
  grid: {
    flexDirection: "row",
    flexWrap: "wrap",
    justifyContent: "space-between",
  },
});
