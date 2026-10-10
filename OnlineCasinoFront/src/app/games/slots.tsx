import {
  ImageBackground,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useLocalSearchParams } from "expo-router";

import { useTheme } from "../../../src/theme/ThemeProvider";
import { Input } from "../../../src/components/ui/Input";
import { Button } from "../../../src/components/ui/Button";
import { SlotMachine } from "../../../src/components/slots/SlotMachine";
import { useSlotsScreen } from "../../../src/hooks/useSlotsScreen";

/**
 * Slots game screen.
 *
 * Handles bet input, spin flow, reel animation, and result display.
 *
 * @returns The rendered slots screen.
 */
export default function SlotsScreen() {
  const theme = useTheme();
  const params = useLocalSearchParams<{ code?: string }>();

  const {
    bet,
    handleBetChange,
    error,
    spinning,
    finalSymbols,
    matchCount,
    lastWin,
    reelsCount,
    currency,
    shownBalance,
    handleSpin,
    handleReelsStop,
  } = useSlotsScreen({ gameCode: params.code });

  const resultText = (() => {
    if (spinning || matchCount === 0) return null;
    if (lastWin > 0) {
      return matchCount === reelsCount
        ? `JACKPOT You won ${lastWin.toFixed(2)} ${currency}`
        : `You won ${lastWin.toFixed(2)} ${currency}`;
    }
    return "Better luck next time";
  })();

  const resultColor = lastWin > 0 ? theme.success : theme.danger;

  return (
    <ImageBackground
      source={require("../../../assets/backgrounds/profile.jpg")}
      style={styles.background}
      resizeMode="cover"
    >
      <View style={styles.overlay} />

      <SafeAreaView style={styles.container}>
        <ScrollView contentContainerStyle={styles.scroll}>
          <View
            style={[
              styles.balanceRow,
              { backgroundColor: theme.card, borderColor: theme.border },
            ]}
          >
            <Text style={[styles.balanceText, { color: theme.text }]}>
              {shownBalance.toFixed(2)} {currency}
            </Text>
          </View>

          <View style={styles.machineBox}>
            <SlotMachine
              reelsCount={reelsCount}
              finalSymbols={finalSymbols}
              spinning={spinning}
              onStop={handleReelsStop}
            />

            {!spinning && matchCount > 0 && (
              <View style={styles.resultBox}>
                <Text style={[styles.resultText, { color: resultColor }]}>
                  {resultText}
                </Text>
                {lastWin > 0 && (
                  <Text style={[styles.matchText, { color: theme.card }]}>
                    {matchCount} matched
                  </Text>
                )}
              </View>
            )}
          </View>

          <View style={styles.bottomBox}>
            <Input
              label="Bet"
              value={bet}
              onChangeText={handleBetChange}
              keyboardType="decimal-pad"
              error={error}
            />

            <Button
              title={spinning ? "Spinning..." : "Spin"}
              onPress={handleSpin}
              loading={spinning}
              disabled={spinning}
            />
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
  scroll: {
    padding: 16,
    flexGrow: 1,
    paddingBottom: 20,
  },
  balanceRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    padding: 16,
    borderRadius: 16,
    borderWidth: 1,
    marginBottom: 24,
  },
  balanceText: { fontWeight: "800", fontSize: 18 },
  machineBox: {
    height: 240,
    alignItems: "center",
  },
  resultBox: {
    alignItems: "center",
    marginTop: 24,
    paddingVertical: 8,
  },
  resultText: {
    fontSize: 20,
    fontWeight: "900",
    textAlign: "center",
  },
  matchText: {
    fontSize: 14,
    marginTop: 4,
    fontWeight: "600",
  },
  bottomBox: {
    marginTop: "auto",
    paddingTop: 24,
  },
});
