// import { useEffect, useRef, useState } from "react";
// import { StyleSheet, View } from "react-native";

// import { Reel } from "./Reel";

// interface SlotMachineProps {
//   reelsCount: number;
//   finalSymbols: string[] | null;
//   spinning: boolean;
//   onStop?: () => void;
// }

// export const SlotMachine = ({
//   reelsCount,
//   finalSymbols,
//   spinning,
//   onStop,
// }: SlotMachineProps) => {
//   const [stoppedReels, setStoppedReels] = useState<number>(0);
//   const prevSpinning = useRef(spinning);

//   useEffect(() => { 
//     if (spinning && !prevSpinning.current) {
//       setStoppedReels(0);
//     }
//     prevSpinning.current = spinning;
 
//     if (
//       !spinning &&
//       finalSymbols !== null &&
//       stoppedReels === reelsCount &&
//       onStop
//     ) {
//       onStop();
//     }
//   }, [spinning, finalSymbols, stoppedReels, reelsCount, onStop]);

//   return (
//     <View style={styles.container}>
//       {Array.from({ length: reelsCount }).map((_, i) => (
//         <Reel
//           key={i}
//           finalSymbol={finalSymbols?.[i] ?? null}
//           spinning={spinning}
//           delay={i * 250}
//           onStop={() => setStoppedReels((n) => n + 1)}
//           reelsCount={reelsCount}
//         />
//       ))}
//     </View>
//   );
// };

// const styles = StyleSheet.create({
//   container: {
//     flexDirection: "row",
//     justifyContent: "center",
//     alignItems: "center",
//     gap: 1,
//   },
// });
 
import { useEffect, useRef, useState } from "react";
import { StyleSheet, View } from "react-native";

import { useEvent } from "../../hooks/useEvent";
import { Reel } from "./Reel";

const REEL_STAGGER_MS = 250;

interface Props {
  reelsCount: number;
  finalSymbols: string[] | null;
  spinning: boolean;
  onStop?: () => void;
}

/**
 * Slot machine with multiple reels.
 *
 * Each reel is staggered by a fixed delay so they stop sequentially.
 * `onStop` is invoked once every reel has finished settling.
 *
 * @param props - Component props.
 * @param props.reelsCount - Number of reels to render.
 * @param props.finalSymbols - Symbols to settle on, or null.
 * @param props.spinning - Whether the machine is currently spinning.
 * @param props.onStop - Called when all reels have stopped.
 * @returns The rendered slot machine.
 */
export const SlotMachine = ({
  reelsCount,
  finalSymbols,
  spinning,
  onStop,
}: Props) => {
  const [stoppedReels, setStoppedReels] = useState(0);
  const prevSpinning = useRef(spinning);
  const notifyStop = useEvent(() => onStop?.());

  useEffect(() => {
    if (spinning) {
      if (!prevSpinning.current) {
        setStoppedReels(0);
      }
      prevSpinning.current = true;
      return;
    }

    prevSpinning.current = false;

    if (finalSymbols !== null && stoppedReels === reelsCount) {
      notifyStop();
    }
  }, [spinning, finalSymbols, stoppedReels, reelsCount, notifyStop]);

  return (
    <View style={styles.container}>
      {Array.from({ length: reelsCount }).map((_, i) => (
        <Reel
          key={i}
          finalSymbol={finalSymbols?.[i] ?? null}
          spinning={spinning}
          delay={i * REEL_STAGGER_MS}
          onStop={() => setStoppedReels((n) => n + 1)}
          reelsCount={reelsCount}
        />
      ))}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: "row",
    justifyContent: "center",
    alignItems: "center",
    gap: 1,
  },
});
