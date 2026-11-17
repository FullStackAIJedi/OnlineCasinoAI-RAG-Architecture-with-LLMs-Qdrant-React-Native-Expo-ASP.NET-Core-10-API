import { useState } from "react";
import { FlatList, RefreshControl, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useTheme } from "../../../src/theme/ThemeProvider";
import { Dropdown } from "../../../src/components/ui/Dropdown";
import { GAME_OPTIONS } from "../../../src//utils/constants";

import { useHistory } from "../../../src/hooks/useHistory";

/**
 * Game history screen with a filterable list of past rounds.
 *
 * @returns The rendered history screen.
 */
export default function HistoryScreen() {
  const theme = useTheme();

  const [game, setGame] = useState("");
  const { data, refetch, isRefetching, isLoading } = useHistory(
    game || undefined,
  );

  return (
    <SafeAreaView
      style={[styles.container, { backgroundColor: theme.background }]}
      edges={["top"]}
    >
      <View style={{ paddingHorizontal: 16, paddingTop: 16 }}>
        <Dropdown
          label="Game"
          options={GAME_OPTIONS}
          value={game}
          onChange={setGame}
        />
      </View>

      <FlatList
        contentContainerStyle={{ padding: 16 }}
        data={data?.items ?? []}
        keyExtractor={(item) => item.roundId}
        refreshControl={
          <RefreshControl
            refreshing={isRefetching}
            onRefresh={async () => {
              await refetch();
            }}
            tintColor={theme.primary}
          />
        }
        renderItem={({ item }) => (
          <View
            style={[
              styles.card,
              { backgroundColor: theme.card, borderColor: theme.border },
            ]}
          >
            <View style={styles.row}>
              <Text style={[styles.game, { color: theme.text }]}>
                {item.game.toUpperCase()}
              </Text>
              <View
                style={[
                  styles.badge,
                  {
                    backgroundColor:
                      item.result === "Win"
                        ? theme.success
                        : item.result === "Push"
                          ? theme.warning
                          : theme.danger,
                  },
                ]}
              >
                <Text style={styles.badgeText}>{item.result ?? "-"}</Text>
              </View>
            </View>
            <View style={styles.row}>
              <Text style={[styles.label, { color: theme.textSecondary }]}>
                Bet: {item.betAmount.toFixed(2)}
              </Text>
              <Text
                style={[
                  styles.win,
                  {
                    color:
                      item.result === "Win"
                        ? theme.success
                        : item.result === "Push"
                          ? theme.warning
                          : theme.danger,
                  },
                ]}
              >
                {item.result === "Win"
                  ? `Win: ${item.winAmount.toFixed(2)}`
                  : item.result === "Push"
                    ? `Push: ${item.betAmount.toFixed(2)}`
                    : `Lose: ${item.betAmount.toFixed(2)}`}
              </Text>
            </View>
            <Text style={[styles.date, { color: theme.textSecondary }]}>
              {new Date(item.createdAt).toLocaleString()}
            </Text>
          </View>
        )}
        ListEmptyComponent={
          !isLoading ? (
            <Text style={[styles.empty, { color: theme.textSecondary }]}>
              No rounds played yet
            </Text>
          ) : undefined
        }
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  card: { borderRadius: 12, borderWidth: 1, padding: 16, marginBottom: 12 },
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 8,
  },
  game: { fontSize: 16, fontWeight: "800", letterSpacing: 1 },
  badge: { paddingHorizontal: 10, paddingVertical: 4, borderRadius: 12 },
  badgeText: { color: "#FFF", fontSize: 12, fontWeight: "700" },
  label: { fontSize: 13 },
  win: { fontSize: 15, fontWeight: "700" },
  date: { fontSize: 9, marginTop: 4 },
  empty: { textAlign: "center", marginTop: 32 },
});
