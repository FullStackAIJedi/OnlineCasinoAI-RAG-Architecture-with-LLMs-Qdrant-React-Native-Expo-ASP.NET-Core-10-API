import { useState } from "react";
import { FlatList, RefreshControl, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useTheme } from "../../../src/theme/ThemeProvider";
import { Button } from "../../../src/components/ui/Button";
import { Input } from "../../../src/components/ui/Input";
import { Modal } from "../../../src/components/ui/Modal";

import {
  useDeposit,
  useTransactions,
  useWallet,
  useWithdraw,
} from "../../../src/hooks/useWallet";

/**
 * Wallet screen with balance, deposit/withdraw actions, and transaction list.
 *
 * @returns The rendered wallet screen.
 */
export default function WalletScreen() {
  const theme = useTheme();

  const [modal, setModal] = useState<"deposit" | "withdraw" | null>(null);
  const [amount, setAmount] = useState("");
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const deposit = useDeposit();
  const withdraw = useWithdraw();
  const { data: wallet, refetch: refetchWallet } = useWallet();
  const { data: tx, refetch: refetchTx, isRefetching } = useTransactions();

  const handleAction = () => {
    setErrorMessage(null);
    const value = parseFloat(amount);
    if (isNaN(value) || value <= 0) {
      setErrorMessage("Invalid amount");
      return;
    }

    const mutation = modal === "deposit" ? deposit : withdraw;
    mutation.mutate(value, {
      onSuccess: () => {
        setModal(null);
        setAmount("");
        setSuccessMessage(
          modal === "deposit" ? "Deposit successful" : "Withdrawal successful",
        );
      },
      onError: (err: any) =>
        setErrorMessage(err.message || "Something went wrong"),
    });
  };

  return (
    <SafeAreaView
      style={[styles.container, { backgroundColor: theme.background }]}
      edges={["top"]}
    >
      <View
        style={[
          styles.balanceCard,
          { backgroundColor: theme.card, borderColor: theme.border },
        ]}
      >
        <Text style={[styles.balance, { color: theme.text }]}>
          {wallet?.balance.toFixed(2) ?? "0.00"} {wallet?.currency ?? "EUR"}
        </Text>
        <View style={styles.actions}>
          <View>
            <Button
              style={{
                borderColor: theme.warning,
                borderWidth: 2,
                paddingHorizontal: 16,
                alignSelf: "flex-start",
              }}
              textStyle={{
                color: theme.warning,
                fontSize: 14,
                fontWeight: "600",
              }}
              title="Deposit"
              onPress={() => setModal("deposit")}
            />
          </View>
          <View>
            <Button
              style={{
                borderColor: theme.danger,
                borderWidth: 2,
                paddingHorizontal: 16,
                alignSelf: "flex-start",
              }}
              textStyle={{
                color: theme.danger,
                fontSize: 14,
                fontWeight: "600",
              }}
              title="Withdrawal"
              onPress={() => setModal("withdraw")}
            />
          </View>
        </View>
      </View>

      <Text style={[styles.sectionTitle, { color: theme.text }]}>
        Transactions
      </Text>

      <FlatList
        data={tx?.items ?? []}
        keyExtractor={(item) => String(item.id)}
        refreshControl={
          <RefreshControl
            refreshing={isRefetching}
            onRefresh={() => {
              refetchWallet();
              refetchTx();
            }}
            tintColor={theme.primary}
          />
        }
        renderItem={({ item }) => (
          <View
            style={[
              styles.txItem,
              { backgroundColor: theme.card, borderColor: theme.border },
            ]}
          >
            <View>
              <Text style={[styles.txType, { color: theme.text }]}>
                {item.type}
              </Text>
              <Text style={[styles.txDate, { color: theme.textSecondary }]}>
                {new Date(item.createdAt).toLocaleString()}
              </Text>
            </View>
            <Text
              style={[
                styles.txAmount,
                { color: item.amount >= 0 ? theme.success : theme.danger },
              ]}
            >
              {item.amount >= 0 ? "+" : ""}
              {item.amount.toFixed(2)}
            </Text>
          </View>
        )}
        ListEmptyComponent={
          <Text style={[styles.empty, { color: theme.textSecondary }]}>
            No transactions
          </Text>
        }
      />

      <Modal
        variant={modal === "deposit" ? "warning" : "danger"}
        backdropColor="rgba(12, 1, 21, 0.85)"
        background={theme.card}
        visible={modal !== null}
        onClose={() => setModal(null)}
        title={modal === "deposit" ? "Deposit" : "Withdrawal"}
      >
        <Input
          borderColor={theme.text}
          label="Amount"
          labelColor={theme.text}
          value={amount}
          onChangeText={setAmount}
          keyboardType="decimal-pad"
          placeholder="10.00"
        />
        <Button
          title="ОК"
          onPress={handleAction}
          loading={deposit.isPending || withdraw.isPending}
          style={{
            borderColor: theme.text,
            borderWidth: 2,
            paddingHorizontal: 23,
            paddingVertical: 12,
            alignSelf: "flex-start",
          }}
          textStyle={{
            color: theme.text,
            fontSize: 16,
            fontWeight: "900",
          }}
        />
      </Modal>

      <Modal
        visible={!!successMessage}
        onClose={() => setSuccessMessage(null)}
        title="Success"
        variant="success"
      >
        <Text
          style={{
            color: theme.textTertiary,
            fontSize: 15,
            marginBottom: 20,
            lineHeight: 22,
          }}
        >
          {successMessage}
        </Text>
        <Button title="OK" onPress={() => setSuccessMessage(null)} />
      </Modal>

      <Modal
        visible={!!errorMessage}
        onClose={() => setErrorMessage(null)}
        title="Error"
        variant="danger"
      >
        <Text
          style={{
            color: theme.danger,
            fontSize: 15,
            marginBottom: 20,
            lineHeight: 22,
          }}
        >
          {errorMessage}
        </Text>
        <Button title="OK" onPress={() => setErrorMessage(null)} />
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 16 },
  balanceCard: {
    borderRadius: 16,
    padding: 20,
    borderWidth: 1,
    marginBottom: 24,
  },
  balanceLabel: { fontSize: 14, marginBottom: 8 },
  balance: { fontSize: 21, fontWeight: "900", marginBottom: 16 },
  actions: {
    flexDirection: "row",
    gap: 21,
    alignItems: "center",
    justifyContent: "center",
  },
  sectionTitle: { fontSize: 20, fontWeight: "800", marginBottom: 12 },
  txItem: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 16,
    borderRadius: 12,
    borderWidth: 1,
    marginBottom: 8,
  },
  txType: { fontSize: 16, fontWeight: "600" },
  txDate: { fontSize: 10, marginTop: 4 },
  txAmount: { fontSize: 18, fontWeight: "800" },
  empty: { textAlign: "center", marginTop: 32 },
});
