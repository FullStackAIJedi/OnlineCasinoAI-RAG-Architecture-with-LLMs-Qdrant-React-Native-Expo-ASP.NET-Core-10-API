import { useState } from "react";
import { ImageBackground, ScrollView, StyleSheet, Text, View } from "react-native";

import { SafeAreaView } from "react-native-safe-area-context";

import type { BlackjackState } from "../../../src/api/types";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { Input } from "../../../src/components/ui/Input";
import { Button } from "../../../src/components/ui/Button";
import { PlayingCard } from "../../../src/components/blackjack/PlayingCard";

import { useStartBlackjack, useHit, useStand, useDouble } from "../../../src/hooks/useBlackjack";
import { useWallet } from "../../../src/hooks/useWallet";

type BlackjackAction = "hit" | "stand" | "double";

const MIN_BET = 5;
const MAX_BET = 500;
const DEFAULT_BET = "10";

/**
 * Blackjack game screen.
 *
 * Handles bet input, game state, and player actions (hit, stand, double).
 *
 * @returns The Blackjack screen.
 */
export default function BlackjackScreen() {
  const theme = useTheme();

  const [bet, setBet] = useState(DEFAULT_BET);
  const [state, setState] = useState<BlackjackState | null>(null);
  const [resultText, setResultText] = useState<string | null>(null);
  const [resultColor, setResultColor] = useState<string>("");

  const stand = useStand();
  const double = useDouble();
  const { data: wallet } = useWallet();
  const start = useStartBlackjack("blackjack");
  const hit = useHit();

  const handleStart = () => {
    const betAmount = parseFloat(bet);

    if (isNaN(betAmount) || betAmount <= 0) {
      setResultColor(theme.danger);
      setResultText("Invalid bet");
      return;
    }

    if (betAmount < MIN_BET) {
      setResultColor(theme.danger);
      setResultText(`Minimum bet is ${MIN_BET} EUR`);
      return;
    }

    if (betAmount > MAX_BET) {
      setResultColor(theme.danger);
      setResultText(`Maximum bet is ${MAX_BET} EUR`);
      return;
    }

    if (betAmount > (wallet?.balance ?? 0)) {
      setResultColor(theme.danger);
      setResultText("Insufficient balance");
      return;
    }

    setResultText(null);
    setResultColor("");

    start.mutate(
      { betAmount },
      {
        onSuccess: (data) => setState(data),
        onError: (err: any) => {
          setResultColor(theme.danger);
          setResultText(err.message || "Failed to start game");
        },
      }
    );
  };

  const handleAction = (action: BlackjackAction) => {
    if (!state) return;

    const mutation =
      action === "hit" ? hit : action === "stand" ? stand : double;

    mutation.mutate(state.sessionId, {
      onSuccess: (data) => {
        setState(data);
        if (data.result) {
          if (data.result === "Win") {
            setResultColor(theme.success);
            setResultText(`You won ${data.winAmount.toFixed(2)} EUR`);
          } else if (data.result === "Push") {
            setResultColor(theme.warning);
            setResultText(`Push ${data.betAmount.toFixed(2)} EUR returned`);
          } else {
            setResultColor(theme.danger);
            setResultText(`You lost ${data.betAmount.toFixed(2)} EUR`);
          }
        }
      },
      onError: (err: any) => console.log("Error", err.message),
    });
  };

  const isLoading = hit.isPending || stand.isPending || double.isPending;

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
            <Text style={{ color: theme.text, fontWeight: "800", fontSize: 18 }}>
              {wallet?.balance.toFixed(2) ?? "0.00"} {wallet?.currency ?? "EUR"}
            </Text>
          </View>

          {state && (
            <>
              <View>
                <Text style={[styles.handTitle, { color: theme.textSecondary }]}>
                  Dealer:{" "}
                  {state.status === "Finished" ? `${state.dealerScore}` : ""}
                </Text>
                <View style={styles.cards}>
                  {state.dealerHand.map((c, i) => (
                    <PlayingCard key={i} card={c} size="medium" />
                  ))}
                </View>
              </View>

              <View>
                <Text style={[styles.handTitle, { color: theme.textSecondary }]}>
                  You: {state.playerScore}
                </Text>
                <View style={styles.cards}>
                  {state.playerHand.map((c, i) => (
                    <PlayingCard key={i} card={c} size="medium" />
                  ))}
                </View>
              </View>
            </>
          )}

          {resultText && (
            <Text style={[styles.resultText, { color: resultColor }]}>
              {resultText}
            </Text>
          )}
        </ScrollView>

        <View style={styles.bottomBox}>
          {!state ? (
            <>
              <Input
                label="Bet"
                value={bet}
                onChangeText={setBet}
                keyboardType="decimal-pad"
              />

              <View style={{ alignItems: "flex-start", marginTop: 8 }}>
                <Button
                  title="Start"
                  onPress={handleStart}
                  loading={start.isPending}
                  style={{
                    borderColor: "#22C55E",
                    borderWidth: 2,
                    borderRadius: 14,
                    paddingHorizontal: 32,
                    paddingVertical: 10,
                  }}
                  textStyle={{
                    color: "#22C55E",
                    fontSize: 14,
                    fontWeight: "700",
                  }}
                />
              </View>
            </>
          ) : state.status === "PlayerTurn" ? (
            <View style={styles.actions}>
              <View style={{ flex: 1, marginRight: 6 }}>
                <Button
                  title="Hit"
                  onPress={() => handleAction("hit")}
                  loading={isLoading}
                  style={{
                    borderColor: "#22C55E",
                    borderWidth: 2,
                    borderRadius: 14,
                    paddingVertical: 14,
                  }}
                  textStyle={{
                    color: "#22C55E",
                    fontSize: 12,
                    fontWeight: "700",
                  }}
                />
              </View>
              <View style={{ flex: 1, marginHorizontal: 6 }}>
                <Button
                  title="Stand"
                  onPress={() => handleAction("stand")}
                  loading={isLoading}
                  style={{
                    borderColor: "orange",
                    borderWidth: 2,
                    borderRadius: 14,
                    paddingVertical: 14,
                  }}
                  textStyle={{
                    color: "orange",
                    fontSize: 12,
                    fontWeight: "700",
                  }}
                />
              </View>
              <View style={{ flex: 1, marginLeft: 6 }}>
                <Button
                  title="Double"
                  onPress={() => handleAction("double")}
                  loading={isLoading}
                  style={{
                    borderColor: "#e40a40",
                    borderWidth: 2,
                    borderRadius: 14,
                    paddingVertical: 14,
                  }}
                  textStyle={{
                    color: "#e40a40",
                    fontSize: 12,
                    fontWeight: "700",
                  }}
                />
              </View>
            </View>
          ) : (
            <Button
              title="New game"
              onPress={() => {
                setState(null);
                setResultText(null);
                setResultColor("");
              }}
              style={{
                borderColor: "#22C55E",
                borderWidth: 2,
                borderRadius: 14,
                paddingVertical: 8,
                paddingHorizontal: 12,
              }}
              textStyle={{
                color: theme.success,
                fontSize: 16,
                fontWeight: "700",
              }}
            />
          )}
        </View>
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
  container: { flex: 1, marginBottom: 16 },
  scroll: {
    padding: 16,
    paddingBottom: 20,
  },
  balanceRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    padding: 16,
    borderRadius: 16,
    borderWidth: 1,
    marginBottom: 16,
  },
  handTitle: {
    fontSize: 14,
    marginBottom: 8,
    fontWeight: "600",
  },
  cards: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 8,
    marginBottom: 16,
  },
  actions: {
    flexDirection: "row",
  },
  bottomBox: {
    padding: 16,
    paddingBottom: 20,
  },
  resultText: {
    fontSize: 18,
    fontWeight: "800",
    textAlign: "center",
    marginTop: 8,
    marginBottom: 8,
  },
});
