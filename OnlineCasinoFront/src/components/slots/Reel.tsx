// import { useEffect, useRef, useState } from "react";
// import { Animated, Easing, StyleSheet, Text, View } from "react-native";
// import { useTheme } from "../../theme/ThemeProvider";
// import { ALL_SYMBOL_CODES, getSymbol } from "./symbols";

// interface ReelProps {
//   finalSymbol: string | null;
//   spinning: boolean;
//   duration?: number;
//   delay?: number;
//   onStop?: () => void;
//   reelsCount?: number;
// }

// export const Reel = ({
//   finalSymbol,
//   spinning,
//   duration = 1500,
//   delay = 0,
//   onStop,
//   reelsCount = 3,
// }: ReelProps) => {
//   const theme = useTheme();

//   const [displaySymbol, setDisplaySymbol] = useState(finalSymbol ?? "cherry");

//   const bounceAnim = useRef(new Animated.Value(1)).current;
//   const fadeAnim = useRef(new Animated.Value(1)).current;
//   const onStopRef = useRef(onStop);
//   onStopRef.current = onStop;

//   useEffect(() => {
//     if (spinning) {
//       const interval = setInterval(() => {
//         const random =
//           ALL_SYMBOL_CODES[
//             Math.floor(Math.random() * ALL_SYMBOL_CODES.length)
//           ];
//         setDisplaySymbol(random);
//       }, 80);

//       return () => clearInterval(interval);
//     }

//     if (!spinning && finalSymbol) {
//       const timer = setTimeout(() => {
//         setDisplaySymbol(finalSymbol);

//         Animated.sequence([
//           Animated.timing(bounceAnim, {
//             toValue: 1.15,
//             duration: 120,
//             easing: Easing.out(Easing.ease),
//             useNativeDriver: true,
//           }),
//           Animated.timing(bounceAnim, {
//             toValue: 1,
//             duration: 120,
//             easing: Easing.in(Easing.ease),
//             useNativeDriver: true,
//           }),
//         ]).start();

//         onStopRef.current?.();
//       }, delay);

//       return () => clearTimeout(timer);
//     }
//   }, [spinning, finalSymbol, delay, bounceAnim]);

//   const symbol = getSymbol(displaySymbol);
//   const isFourReels = reelsCount === 4;
//   const bgSize = isFourReels ? 80 : 108;
//   const emojiSize = isFourReels ? 32 : 45;

//   return (
//     <Animated.View
//       style={{
//         transform: [{ scale: bounceAnim }],
//         opacity: fadeAnim,
//       }}
//     >
//       <View
//         style={[
//           styles.symbolBg,
//           {
//             backgroundColor: theme.card,
//             width: bgSize,
//             height: bgSize,
//             borderRadius: bgSize / 2,
//           },
//         ]}
//       >
//         <Text style={[styles.emoji, { fontSize: emojiSize }]}>
//           {symbol.emoji}
//         </Text>
//       </View>
//     </Animated.View>
//   );
// };

// const styles = StyleSheet.create({
//   symbolBg: {
//     justifyContent: "center",
//     alignItems: "center",
//   },
//   emoji: {
//     textAlign: "center",
//   },
// });

import { useEffect, useRef, useState } from "react";
import { Animated, Easing, StyleSheet, Text, View } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import { useEvent } from "../../hooks/useEvent";
import { ALL_SYMBOL_CODES, getSymbol } from "./symbols";

const SYMBOL_CYCLE_MS = 80;
const BOUNCE_SCALE = 1.15;
const BOUNCE_DURATION_MS = 120;
const DEFAULT_SYMBOL = "cherry";
const FOUR_REELS_BG_SIZE = 80;
const THREE_REELS_BG_SIZE = 108;
const FOUR_REELS_EMOJI_SIZE = 32;
const THREE_REELS_EMOJI_SIZE = 45;
const FOUR_REELS_COUNT = 4;

interface Props {
  finalSymbol: string | null;
  spinning: boolean;
  delay?: number;
  onStop?: () => void;
  reelsCount?: number;
}

/**
 * Single slot reel.
 *
 * While `spinning`, rapidly cycles through random symbols. When the spin
 * ends, it settles on `finalSymbol` after `delay` milliseconds, plays a
 * bounce animation, and invokes `onStop`.
 *
 * @param props - Component props.
 * @param props.finalSymbol - Symbol to settle on, or null.
 * @param props.spinning - Whether the reel is currently spinning.
 * @param props.delay - Delay before settling, in milliseconds.
 * @param props.onStop - Called when this reel finishes settling.
 * @param props.reelsCount - Total number of reels in the machine.
 * @returns The rendered reel.
 */
export const Reel = ({
  finalSymbol,
  spinning,
  delay = 0,
  onStop,
  reelsCount = 3,
}: Props) => {
  const theme = useTheme();

  const [displaySymbol, setDisplaySymbol] = useState(
    finalSymbol ?? DEFAULT_SYMBOL
  );

  const bounceAnim = useRef(new Animated.Value(1)).current;
  const notifyStop = useEvent(() => onStop?.());

  useEffect(() => {
    if (spinning) {
      const interval = setInterval(() => {
        const random =
          ALL_SYMBOL_CODES[
            Math.floor(Math.random() * ALL_SYMBOL_CODES.length)
          ];
        setDisplaySymbol(random);
      }, SYMBOL_CYCLE_MS);

      return () => clearInterval(interval);
    }

    if (finalSymbol) {
      const timer = setTimeout(() => {
        setDisplaySymbol(finalSymbol);

        Animated.sequence([
          Animated.timing(bounceAnim, {
            toValue: BOUNCE_SCALE,
            duration: BOUNCE_DURATION_MS,
            easing: Easing.out(Easing.ease),
            useNativeDriver: true,
          }),
          Animated.timing(bounceAnim, {
            toValue: 1,
            duration: BOUNCE_DURATION_MS,
            easing: Easing.in(Easing.ease),
            useNativeDriver: true,
          }),
        ]).start();

        notifyStop();
      }, delay);

      return () => clearTimeout(timer);
    }
  }, [spinning, finalSymbol, delay, bounceAnim, notifyStop]);

  const symbol = getSymbol(displaySymbol);
  const isFourReels = reelsCount === FOUR_REELS_COUNT;
  const bgSize = isFourReels ? FOUR_REELS_BG_SIZE : THREE_REELS_BG_SIZE;
  const emojiSize = isFourReels
    ? FOUR_REELS_EMOJI_SIZE
    : THREE_REELS_EMOJI_SIZE;

  return (
    <Animated.View style={{ transform: [{ scale: bounceAnim }] }}>
      <View
        style={[
          styles.symbolBg,
          {
            backgroundColor: theme.card,
            width: bgSize,
            height: bgSize,
            borderRadius: bgSize / 2,
          },
        ]}
      >
        <Text style={[styles.emoji, { fontSize: emojiSize }]}>
          {symbol.emoji}
        </Text>
      </View>
    </Animated.View>
  );
};

const styles = StyleSheet.create({
  symbolBg: {
    justifyContent: "center",
    alignItems: "center",
  },
  emoji: {
    textAlign: "center",
  },
});
