// import { StyleSheet, Text, View } from "react-native";

// import { useTheme } from "../../theme/ThemeProvider";
// import { RouletteWheel } from "./RouletteWheel";
// import { RED_NUMBERS } from "../../../src/utils/constants";

// interface Props {
//   winningNumber: number | null;
//   spinning: boolean;
//   onStop?: () => void;
// }

// export const RouletteGame = ({ winningNumber, spinning, onStop }: Props) => {
//   const theme = useTheme();

//   // 37 = 00 рулетка
//   const displayNumber =
//     winningNumber === null
//       ? "—"
//       : winningNumber === 37
//       ? "00"
//       : String(winningNumber);

//   // 0 и 37 (00) са зелени
//   const color =
//     winningNumber === null
//       ? "—"
//       : winningNumber === 0 || winningNumber === 37
//       ? "Green"
//       : RED_NUMBERS.has(winningNumber)
//       ? "Red"
//       : "Black";

//   // 0 и 37 не са even/odd
//   const parity =
//     winningNumber === null || winningNumber === 0 || winningNumber === 37
//       ? "—"
//       : winningNumber % 2 === 0
//       ? "Even"
//       : "Odd";

//   const colorHex =
//     color === "Red" ? "#E63946" : color === "Black" ? "#1D1D1D" : "#27AE60";

//   return (
//     <View style={styles.container}>
//       <RouletteWheel
//         winningNumber={winningNumber}
//         spinning={spinning}
//         onStop={onStop}
//       />

//       {!spinning && winningNumber !== null && (
//         <View style={styles.resultBox}>
//           <Text style={styles.label}>Result</Text>
//           <Text style={[styles.number, { color: colorHex }]}>
//             {displayNumber}
//           </Text>
//           <Text style={styles.info}>
//             {color} • {parity}
//           </Text>
//         </View>
//       )}
//     </View>
//   );
// };

// const styles = StyleSheet.create({
//   container: {
//     width: "100%",
//     alignItems: "center",
//   },
//   resultBox: {
//     marginTop: 20,
//     padding: 16,
//     borderRadius: 27,
//     borderWidth: 1,
//     alignItems: "center",
//     minWidth: 220, 
//     backgroundColor: "#FFFFFF",
//     borderColor: "#E0E0E0",
//   },
//   label: {
//     fontSize: 12,
//     marginBottom: 4, 
//     color: "#666666",
//   },
//   number: {
//     fontSize: 48,
//     fontWeight: "900",
//   },
//   info: {
//     fontSize: 18,
//     fontWeight: "700",
//     marginTop: 4, 
//     color: "#1A1A1A",
//   },
// });

import { StyleSheet, Text, View } from "react-native";

import { RouletteWheel } from "./RouletteWheel";

const RED_NUMBERS = new Set([
  1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36,
]);

type RouletteColor = "Red" | "Black" | "Green" | "—";
type RouletteParity = "Even" | "Odd" | "—";

const COLOR_HEX: Record<RouletteColor, string> = {
  Red: "#E63946",
  Black: "#1D1D1D",
  Green: "#27AE60",
  "—": "#666666",
};

interface Props {
  winningNumber: number | null;
  spinning: boolean;
  onStop?: () => void;
}

/**
 * Formats a roulette number for display, mapping 37 to "00".
 *
 * @param winningNumber - The winning number, or null if none.
 * @returns The display string for the number.
 */
const formatDisplayNumber = (winningNumber: number | null): string => {
  if (winningNumber === null) return "—";
  if (winningNumber === 37) return "00";
  return String(winningNumber);
};

/**
 * Resolves the roulette color for a given number.
 *
 * @param winningNumber - The winning number, or null if none.
 * @returns The color name.
 */
const resolveColor = (winningNumber: number | null): RouletteColor => {
  if (winningNumber === null) return "—";
  if (winningNumber === 0 || winningNumber === 37) return "Green";
  return RED_NUMBERS.has(winningNumber) ? "Red" : "Black";
};

/**
 * Resolves the parity label for a given number.
 *
 * @param winningNumber - The winning number, or null if none.
 * @returns The parity label.
 */
const resolveParity = (winningNumber: number | null): RouletteParity => {
  if (winningNumber === null || winningNumber === 0 || winningNumber === 37) {
    return "—";
  }
  return winningNumber % 2 === 0 ? "Even" : "Odd";
};

/**
 * Roulette game display.
 *
 * Renders the spinning wheel and, once the spin has stopped,
 * the result number with its color and parity.
 *
 * @param props - Component props.
 * @param props.winningNumber - The winning number (37 represents 00), or null.
 * @param props.spinning - Whether the wheel is currently spinning.
 * @param props.onStop - Called when the wheel animation finishes.
 * @returns The rendered roulette game.
 */
export const RouletteGame = ({ winningNumber, spinning, onStop }: Props) => {
  const displayNumber = formatDisplayNumber(winningNumber);
  const color = resolveColor(winningNumber);
  const parity = resolveParity(winningNumber);

  return (
    <View style={styles.container}>
      <RouletteWheel
        winningNumber={winningNumber}
        spinning={spinning}
        onStop={onStop}
      />

      {!spinning && winningNumber !== null && (
        <View style={styles.resultBox}>
          <Text style={styles.label}>Result</Text>
          <Text style={[styles.number, { color: COLOR_HEX[color] }]}>
            {displayNumber}
          </Text>
          <Text style={styles.info}>
            {color} • {parity}
          </Text>
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    width: "100%",
    alignItems: "center",
  },
  resultBox: {
    marginTop: 20,
    padding: 16,
    borderRadius: 27,
    borderWidth: 1,
    alignItems: "center",
    minWidth: 220,
    backgroundColor: "#FFFFFF",
    borderColor: "#E0E0E0",
  },
  label: {
    fontSize: 12,
    marginBottom: 4,
    color: "#666666",
  },
  number: {
    fontSize: 48,
    fontWeight: "900",
  },
  info: {
    fontSize: 18,
    fontWeight: "700",
    marginTop: 4,
    color: "#1A1A1A",
  },
});
