// import { useState, useCallback, useEffect } from "react";
// import { ImageBackground, ScrollView, StyleSheet, Text, View } from "react-native";
// import { SafeAreaView } from "react-native-safe-area-context";
// import { useLocalSearchParams, useFocusEffect } from "expo-router";

// import { useTheme } from "../../../src/theme/ThemeProvider";
// import { Input } from "../../../src/components/ui/Input";
// import { Button } from "../../../src/components/ui/Button";
// import { SlotMachine } from "../../../src/components/slots/SlotMachine";

// import { useSpinSlots } from "../../../src/hooks/useSlots";
// import { useWallet } from "../../../src/hooks/useWallet";
// import { uuid } from "../../../src/utils/uuid";

// const REELS_COUNT: Record<string, number> = {
//   slots: 3,
//   slots4: 4,
// };

// const MIN_SPIN_DURATION = 2000;

// export default function SlotsScreen() {
//   const theme = useTheme();

//   const params = useLocalSearchParams<{ code?: string }>();
//   const gameCode = params.code ?? "slots";
//   const reelsCount = REELS_COUNT[gameCode] ?? 3;

//   const [bet, setBet] = useState("1");
//   const [error, setError] = useState<string | undefined>();
//   const [spinning, setSpinning] = useState(false);
//   const [finalSymbols, setFinalSymbols] = useState<string[] | null>(null);
//   const [matchCount, setMatchCount] = useState(0);
//   const [lastWin, setLastWin] = useState<number>(0);

//   const [pendingResult, setPendingResult] = useState<any>(null);
//   const [displayBalance, setDisplayBalance] = useState<number | null>(null); 

//   const spin = useSpinSlots();
//   const { data: wallet, refetch: refetchWallet } = useWallet();

//   useFocusEffect(
//     useCallback(() => {
//       (async () => {
//         const res = await refetchWallet();
//         if (res.data?.balance !== undefined) {
//           setDisplayBalance(res.data.balance);
//         }
//       })();
//     }, [refetchWallet])
//   );

//   useEffect(() => {
//     if (wallet?.balance !== undefined && displayBalance === null) {
//       setDisplayBalance(wallet.balance);
//     }
//   }, [wallet?.balance, displayBalance]);

//   const currency = wallet?.currency ?? "EUR";
//   const shownBalance = displayBalance ?? wallet?.balance ?? 0;

//   const handleSpin = () => {
//     const betAmount = parseFloat(bet);

//     if (!bet.trim()) { setError("Bet is required"); return; }
//     if (isNaN(betAmount) || betAmount <= 0) { setError("Invalid bet"); return; }
//     if (betAmount > shownBalance) { setError("Insufficient balance"); return; }

//     setError(undefined);
//     setFinalSymbols(null);
//     setMatchCount(0);
//     setLastWin(0);
//     setPendingResult(null);
//     setSpinning(true);

//     setDisplayBalance((prev) => (prev ?? shownBalance) - betAmount);

//     const startTime = Date.now();

//     spin.mutate(
//       {
//         gameCode,
//         data: { betAmount, clientRoundId: uuid() },
//       },
//       {
//         onSuccess: (data) => {
//           const elapsed = Date.now() - startTime;
//           const remaining = Math.max(0, MIN_SPIN_DURATION - elapsed);

//           setTimeout(() => {
//             setPendingResult(data);
//             setFinalSymbols(data.reels);
//             setSpinning(false);
//           }, remaining);
//         },
//         onError: async (err: any) => {
//           setSpinning(false);
//           setPendingResult(null);
//           const res = await refetchWallet();
//           if (res.data?.balance !== undefined) {
//             setDisplayBalance(res.data.balance);
//           }
//         },
//       }
//     );
//   };

//   const handleReelsStop = async () => {
//     const data = pendingResult;
//     if (!data) return;

//     setMatchCount(data.matchCount);
//     setLastWin(data.winAmount);

//     const res = await refetchWallet();
//     if (res.data?.balance !== undefined) {
//       setDisplayBalance(res.data.balance);
//     } else {
//       setDisplayBalance(data.balance);
//     } 
//     setPendingResult(null);
//   };

//   const getResultText = () => {
//     if (spinning || matchCount === 0) return null;

//     if (lastWin > 0) {
//       return matchCount === reelsCount
//         ? `JACKPOT You won ${lastWin.toFixed(2)} ${currency}`
//         : `You won ${lastWin.toFixed(2)} ${currency}`;
//     }

//     return "Better luck next time";
//   };

//   const getResultColor = () => {
//     if (lastWin > 0) return theme.success;
//     return theme.danger;
//   };

//   return (
//     <ImageBackground
//       source={require("../../../assets/backgrounds/profile.jpg")}
//       style={styles.background}
//       resizeMode="cover"
//     >
//       <View style={styles.overlay} />

//       <SafeAreaView style={styles.container}>
//         <ScrollView contentContainerStyle={styles.scroll}>
//           <View style={[styles.balanceRow, { backgroundColor: theme.card, borderColor: theme.border }]}> 
//             <Text style={{ color: theme.text, fontWeight: "800", fontSize: 18 }}>
//               {shownBalance.toFixed(2)} {currency}
//             </Text>
//           </View>

//           <View style={styles.machineBox}>
//             <SlotMachine
//               reelsCount={reelsCount}
//               finalSymbols={finalSymbols}
//               spinning={spinning}
//               onStop={handleReelsStop}
//             />

//             {!spinning && matchCount > 0 && (
//               <View style={styles.resultBox}>
//                 <Text style={[styles.resultText, { color: getResultColor() }]}>
//                   {getResultText()}
//                 </Text>
//                 {lastWin > 0 && (
//                   <Text style={[styles.matchText, { color: theme.card }]}>
//                     {matchCount} matched
//                   </Text>
//                 )}
//               </View>
//             )}
//           </View>
 
//           <View style={styles.bottomBox}>
//             <Input
//               label="Bet"
//               value={bet}
//               onChangeText={(t) => {
//                 setBet(t);
//                 if (error) setError(undefined);
//               }}
//               keyboardType="decimal-pad"
//               error={error}
//             />

//             <Button
//               title={spinning ? "Spinning..." : "Spin"}
//               onPress={handleSpin}
//               loading={spinning}
//               disabled={spinning}
//             />
//           </View>
//         </ScrollView> 
 
//       </SafeAreaView>
//     </ImageBackground>
//   );
// }

// const styles = StyleSheet.create({
//   background: { flex: 1, width: "100%", height: "100%" },
//   overlay: {
//     position: "absolute",
//     top: 0,
//     left: 0,
//     right: 0,
//     bottom: 0,
//   },
//   container: { flex: 1 },
//   scroll: {
//     padding: 16,
//     flexGrow: 1,
//     paddingBottom: 20,
//   },
//   balanceRow: {
//     flexDirection: "row",
//     justifyContent: "space-between",
//     padding: 16,
//     borderRadius: 16,
//     borderWidth: 1,
//     marginBottom: 24,
//   },
//   machineBox: {
//     height: 240,
//     alignItems: "center",
//   },
//   resultBox: {
//     alignItems: "center",
//     marginTop: 24,
//     paddingVertical: 8,
//   },
//   resultText: {
//     fontSize: 20,
//     fontWeight: "900",
//     textAlign: "center",
//   },
//   matchText: {
//     fontSize: 14,
//     marginTop: 4,
//     fontWeight: "600",
//   },
//   bottomBox: {
//     marginTop: "auto",
//     paddingTop: 24,
//   },
// });

import { ImageBackground, ScrollView, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useLocalSearchParams } from "expo-router";

import { useTheme } from "../../../src/theme/ThemeProvider";
import { Input } from "../../../src/components/ui/Input";
import { Button } from "../../../src/components/ui/Button";
import { SlotMachine } from "../../../src/components/slots/SlotMachine";
import { useSlotsScreen } from "../../../src/hooks/useSlotsScreen";
 
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
