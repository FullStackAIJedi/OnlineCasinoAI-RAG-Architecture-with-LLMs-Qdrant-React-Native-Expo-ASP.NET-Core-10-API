import {
  ScrollView,
  StyleSheet,
  Text,
  View,
  ImageBackground,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useTheme } from "../../../src/theme/ThemeProvider";
import { Input } from "../../../src/components/ui/Input";
import { Button } from "../../../src/components/ui/Button";
import { Dropdown } from "../../../src/components/ui/Dropdown";
import { RouletteGame } from "../../../src/components/roulette/RouletteGame";
import { useRouletteScreen } from "../../../src/hooks/useRouletteScreen";
import type { RouletteBetType } from "../../../src/hooks/useRouletteScreen";
import type { CurrentBet } from "../../../src/api/types";

const BET_TYPES: { label: string; value: RouletteBetType }[] = [
  { label: "Red", value: "red" },
  { label: "Black", value: "black" },
  { label: "Even", value: "even" },
  { label: "Odd", value: "odd" },
  { label: "Low (1-18)", value: "low" },
  { label: "High (19-36)", value: "high" },
  { label: "Dozen (1-3)", value: "dozen" },
  { label: "Number (0-36 or 00)", value: "number" },
];

const VALUE_BET_TYPES: RouletteBetType[] = ["number", "dozen"];

/**
 * Formats a roulette bet into a human-readable label.
 *
 * @param bet - The bet to format as `CurrentBet`.
 * @returns The formatted label.
 */
const formatBetLabel = (bet: CurrentBet): string => {
  const { type, value } = bet;
  switch (type) {
    case "red":
      return "Red";
    case "black":
      return "Black";
    case "even":
      return "Even";
    case "odd":
      return "Odd";
    case "low":
      return "Low (1-18)";
    case "high":
      return "High (19-36)";
    case "dozen":
      return value != null ? `Dozen ${value}` : "Dozen";
    case "number":
      if (value === 37) return "Number 00";
      return value != null ? `Number ${value}` : "Number";
    default:
      return type;
  }
};

/**
 * Roulette game screen.
 *
 * Handles bet placement, wheel spinning, wallet balance sync, and result display.
 *
 * @returns The rendered roulette screen.
 */
export default function RouletteScreen() {
  const theme = useTheme();
  const {
    betType,
    setBetType,
    value,
    setValue,
    amount,
    setAmount,
    errors,
    setErrors,
    spinning,
    winningNumber,
    bets,
    currency,
    shownBalance,
    resultText,
    resultColor,
    placeBet,
    handlePlaceBet,
    handleSpin,
    handleSpinStop,
  } = useRouletteScreen();

  const needsValue = VALUE_BET_TYPES.includes(betType);

  const handleValueChange = (t: string) => {
    setValue(t);
    if (errors.value) setErrors({ ...errors, value: undefined });
  };

  const handleAmountChange = (t: string) => {
    setAmount(t);
    if (errors.amount) setErrors({ ...errors, amount: undefined });
  };

  return (
    <ImageBackground
      source={require("../../../assets/backgrounds/profile.jpg")}
      style={styles.background}
      resizeMode="cover"
    >
      <View style={styles.overlay} />

      <SafeAreaView style={styles.container}>
        <ScrollView contentContainerStyle={styles.scroll}>
          <View style={styles.horizontalPadding}>
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
          </View>

          <RouletteGame
            winningNumber={winningNumber}
            spinning={spinning}
            onStop={handleSpinStop}
          />

          <View style={styles.formContainer}>
            {resultText && (
              <Text style={[styles.resultText, { color: resultColor }]}>
                {resultText}
              </Text>
            )}

            <Text style={[styles.sectionTitle, { color: theme.card }]}>
              Place a bet
            </Text>

            <Dropdown
              label="Bet type"
              options={BET_TYPES}
              value={betType}
              onChange={setBetType}
            />

            {needsValue && (
              <Input
                label={
                  betType === "number" ? "Value (0-36 or 00)" : "Value (1-3)"
                }
                value={value}
                onChangeText={handleValueChange}
                keyboardType={betType === "number" ? "default" : "number-pad"}
                error={errors.value}
              />
            )}

            <Input
              label="Amount"
              value={amount}
              onChangeText={handleAmountChange}
              keyboardType="decimal-pad"
              error={errors.amount}
            />

            <View style={styles.placeBetRow}>
              <Button
                title="Place Bet"
                onPress={handlePlaceBet}
                loading={placeBet.isPending}
                disabled={spinning}
                style={{
                  ...styles.placeBetButton,
                  borderColor: theme.success,
                }}
                textStyle={{
                  ...styles.placeBetText,
                  color: theme.success,
                }}
              />
            </View>

            <Text
              style={[
                styles.sectionTitle,
                styles.sectionTitleSpaced,
                { color: theme.card },
              ]}
            >
              Current bets {bets?.length ?? 0}
            </Text>

            {bets?.map((b) => (
              <View
                key={b.id}
                style={[
                  styles.betItem,
                  { backgroundColor: theme.card, borderColor: theme.border },
                ]}
              >
                <Text style={[styles.betItemLabel, { color: theme.text }]}>
                  {formatBetLabel(b)}
                </Text>
                <Text style={{ color: theme.text }}>{b.amount.toFixed(2)}</Text>
              </View>
            ))}

            <View style={styles.spinButtonWrapper}>
              <Button
                title={spinning ? "Spinning..." : "Spin the Wheel"}
                onPress={handleSpin}
                loading={spinning}
                disabled={spinning}
              />
            </View>
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
  scroll: { paddingBottom: 24 },
  horizontalPadding: { paddingHorizontal: 16 },
  formContainer: { paddingHorizontal: 16, paddingVertical: 16 },
  balanceRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    padding: 16,
    borderRadius: 16,
    borderWidth: 1,
    marginTop: 16,
    marginBottom: 16,
  },
  balanceText: { fontWeight: "800", fontSize: 18 },
  sectionTitle: { fontSize: 18, fontWeight: "700", marginBottom: 12 },
  sectionTitleSpaced: { marginTop: 24 },
  placeBetRow: { alignItems: "flex-start", marginTop: 8 },
  placeBetButton: {
    borderWidth: 2,
    paddingHorizontal: 20,
    paddingVertical: 10,
  },
  placeBetText: { fontSize: 14, fontWeight: "700" },
  betItem: {
    flexDirection: "row",
    justifyContent: "space-between",
    padding: 12,
    borderRadius: 10,
    borderWidth: 1,
    marginBottom: 8,
  },
  betItemLabel: { fontWeight: "600" },
  spinButtonWrapper: { marginTop: 16 },
  resultText: {
    fontSize: 21,
    fontWeight: "800",
    textAlign: "center",
    marginBottom: 16,
  },
});
