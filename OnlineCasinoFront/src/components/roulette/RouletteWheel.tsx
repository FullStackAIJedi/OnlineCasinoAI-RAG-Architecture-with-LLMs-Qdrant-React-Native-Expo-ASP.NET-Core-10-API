// import { useEffect, useRef, useState } from "react";
// import { Animated, Easing, LayoutChangeEvent, StyleSheet, Text, View } from "react-native";

// import { WHEEL_ORDER, IMAGE_ROTATION, ANGLE_OFFSET, BALL_ANGLE_OFFSET } from "../../../src/utils/constants";

// const WHEEL = require("../../../assets/roulette/wheel.png");
// const SLOT_ANGLE = 360 / WHEEL_ORDER.length;  // 360 / 38 = 9.47°

// const getNumberAngle = (num: number): number => {
//   const idx = WHEEL_ORDER.indexOf(num);
//   if (idx === -1) return 0;
//   return idx * SLOT_ANGLE + ANGLE_OFFSET;
// };

// interface Props {
//   winningNumber: number | null;
//   spinning: boolean;
//   onStop?: () => void;
// }

// export const RouletteWheel = ({ winningNumber, spinning, onStop }: Props) => {
//   const [parentSize, setParentSize] = useState(0);

//   const wheelRotation = useRef(new Animated.Value(0)).current;
//   const ballRotation = useRef(new Animated.Value(0)).current;

//   const size = parentSize * 0.95; 
//   const ballOrbitRadius = size * 0.36;
//   const ballSize = size * 0.025;  

//   useEffect(() => {
//     if (!spinning || winningNumber === null) return;

//     const targetNumber = winningNumber === 37 ? 999 : winningNumber;
//     const targetWheelAngle = (360 - getNumberAngle(targetNumber)) % 360;
//     const wheelFullSpins = 6 + Math.floor(Math.random() * 3);
//     const wheelTotal = wheelFullSpins * 360 + targetWheelAngle;

//     const ballJitter = (Math.random() - 0.5) * 1;
//     const ballFullSpins = 10 + Math.floor(Math.random() * 4);
//     const ballTotal = -(ballFullSpins * 360) + ballJitter + BALL_ANGLE_OFFSET;

//     Animated.parallel([
//       Animated.timing(wheelRotation, {
//         toValue: wheelTotal,
//         duration: 4500,
//         easing: Easing.out(Easing.cubic),
//         useNativeDriver: true,
//       }),
//       Animated.timing(ballRotation, {
//         toValue: ballTotal,
//         duration: 4500,
//         easing: Easing.out(Easing.cubic),
//         useNativeDriver: true,
//       }),
//     ]).start(() => {
//       wheelRotation.setValue(wheelTotal % 360);
//       ballRotation.setValue(ballTotal % 360);
//       onStop?.();
//     });
//   }, [spinning, winningNumber]);

//   const wheelRotate = wheelRotation.interpolate({
//     inputRange: [-360, 360],
//     outputRange: ["-360deg", "360deg"],
//   });

//   const ballAngle = ballRotation.interpolate({
//     inputRange: [-360, 360],
//     outputRange: ["-360deg", "360deg"],
//   });

//   const handleLayout = (e: LayoutChangeEvent) => {
//     const { width } = e.nativeEvent.layout;
//     setParentSize(width);
//   };

//   return (
//     <View style={styles.container} onLayout={handleLayout}>
//       {size > 0 && (
//         <View
//           style={{
//             width: size,
//             height: size,
//             alignItems: "center",
//             justifyContent: "center",
//             alignSelf: "center",
//           }}
//         >
//           <Animated.Image
//             source={WHEEL}
//             style={{
//               width: size,
//               height: size,
//               transform: [
//                 { rotate: wheelRotate },
//                 { rotate: IMAGE_ROTATION },
//               ],
//             }}
//             resizeMode="contain"
//           />
//           <Animated.View
//             style={[
//               styles.ballOrbit,
//               {
//                 width: ballOrbitRadius * 2,
//                 height: ballOrbitRadius * 2,
//                 transform: [{ rotate: ballAngle }],
//               },
//             ]}
//             pointerEvents="none"
//           >
//             <View
//               style={[
//                 styles.ball,
//                 {
//                   width: ballSize,
//                   height: ballSize,
//                   borderRadius: ballSize / 2,
//                   top: -ballSize / 2,
//                   left: ballOrbitRadius - ballSize / 2,
//                 },
//               ]}
//             />
//           </Animated.View>

//           <View style={[styles.pointer, { top: -size * 0.10 }]}>
//             <Text style={{ fontSize: size * 0.08, color: "#E63946" }}>▼</Text>
//           </View>
//         </View>
//       )}
//     </View>
//   );
// };

// const styles = StyleSheet.create({
//   container: {
//     width: "100%",
//     alignItems: "center",
//     justifyContent: "center",
//     alignSelf: "center",
//   },
//   ballOrbit: {
//     position: "absolute",
//     alignItems: "center",
//     justifyContent: "center",
//   },
//   ball: {
//     position: "absolute",
//     backgroundColor: "#FFFFFF",
//     borderWidth: 1,
//     borderColor: "#CCCCCC",
//     shadowColor: "#000",
//     shadowOffset: { width: 0, height: 2 },
//     shadowOpacity: 0.4,
//     shadowRadius: 3,
//     elevation: 5,
//   },
//   pointer: {
//     position: "absolute",
//     zIndex: 10,
//     alignSelf: "center",
//   },
// });

import { useEffect, useRef, useState } from "react";
import {
  Animated,
  Easing,
  LayoutChangeEvent,
  StyleSheet,
  Text,
  View,
} from "react-native";

import { useEvent } from "../../hooks/useEvent";

const WHEEL = require("../../../assets/roulette/wheel.png");

const ZERO_ZERO_SENTINEL = 999;
const ZERO_ZERO_API = 37;

/**
 * Physical order of numbers on the roulette wheel (clockwise).
 *
 * Differs from the numeric order (0-36) because the wheel layout is
 * intentionally non-sequential. `ZERO_ZERO_SENTINEL` marks the "00" pocket.
 */
const WHEEL_ORDER = [
  0, 2, 14, 35, 23, 4, 16, 33, 21, 6, 18, 31, 19, 8, 12, 29, 25, 10, 27,
  ZERO_ZERO_SENTINEL,
  1, 13, 36, 24, 3, 15, 34, 22, 5, 17, 32, 20, 7, 11, 30, 26, 9, 28,
];

/**
 * Rotation applied to the wheel image so the "0" pocket aligns with the
 * pointer at the top. Specific to the wheel asset.
 */
const IMAGE_ROTATION = "28deg";

/**
 * Angular correction added to the calculated wheel angle to compensate
 * for small misalignments between the mathematical model and the image.
 */
const ANGLE_OFFSET = 0;

/**
 * Angular correction applied to the ball's final position to compensate
 * for small misalignments between the mathematical model and the image.
 */
const BALL_ANGLE_OFFSET = 0;

const SLOT_ANGLE = 360 / WHEEL_ORDER.length;
const SPIN_DURATION_MS = 4500;
const WHEEL_FULL_SPINS_MIN = 6;
const WHEEL_FULL_SPINS_EXTRA = 3;
const BALL_FULL_SPINS_MIN = 10;
const BALL_FULL_SPINS_EXTRA = 4;
const BALL_JITTER_RANGE = 1;
const WHEEL_SIZE_RATIO = 0.95;
const BALL_ORBIT_RATIO = 0.36;
const BALL_SIZE_RATIO = 0.025;
const POINTER_TOP_RATIO = 0.10;
const POINTER_FONT_RATIO = 0.08;

interface Props {
  winningNumber: number | null;
  spinning: boolean;
  onStop?: () => void;
}

/**
 * Resolves the wheel angle at which the given number is positioned.
 *
 * @param num - The roulette number (sentinel maps to the "00" pocket).
 * @returns The angle in degrees, or 0 if the number is not on the wheel.
 */
const getNumberAngle = (num: number): number => {
  const idx = WHEEL_ORDER.indexOf(num);
  if (idx === -1) return 0;
  return idx * SLOT_ANGLE + ANGLE_OFFSET;
};

/**
 * Computes the wheel and ball rotation targets for a winning number.
 *
 * @param winningNumber - The winning number (37 represents 00).
 * @returns The target rotation values for the wheel and ball.
 */
const computeSpinTargets = (
  winningNumber: number,
): { wheel: number; ball: number } => {
  const targetNumber =
    winningNumber === ZERO_ZERO_API ? ZERO_ZERO_SENTINEL : winningNumber;

  const targetWheelAngle = (360 - getNumberAngle(targetNumber)) % 360;
  const wheelFullSpins =
    WHEEL_FULL_SPINS_MIN + Math.floor(Math.random() * WHEEL_FULL_SPINS_EXTRA);
  const wheel = wheelFullSpins * 360 + targetWheelAngle;

  const ballJitter = (Math.random() - 0.5) * BALL_JITTER_RANGE;
  const ballFullSpins =
    BALL_FULL_SPINS_MIN + Math.floor(Math.random() * BALL_FULL_SPINS_EXTRA);
  const ball = -(ballFullSpins * 360) + ballJitter + BALL_ANGLE_OFFSET;

  return { wheel, ball };
};

/**
 * Animated roulette wheel with a rotating ball.
 *
 * When `spinning` becomes true and `winningNumber` is set, animates the
 * wheel and ball to the winning number and calls `onStop` when finished.
 *
 * @param props - Component props.
 * @param props.winningNumber - The winning number (37 represents 00), or null.
 * @param props.spinning - Whether the wheel is currently spinning.
 * @param props.onStop - Called when the spin animation finishes.
 * @returns The rendered animated wheel.
 */
export const RouletteWheel = ({ winningNumber, spinning, onStop }: Props) => {
  const [parentSize, setParentSize] = useState(0);

  const wheelRotation = useRef(new Animated.Value(0)).current;
  const ballRotation = useRef(new Animated.Value(0)).current;
  const notifyStop = useEvent(() => onStop?.());

  const size = parentSize * WHEEL_SIZE_RATIO;
  const ballOrbitRadius = size * BALL_ORBIT_RATIO;
  const ballSize = size * BALL_SIZE_RATIO;

  useEffect(() => {
    if (!spinning || winningNumber === null) return;

    const { wheel, ball } = computeSpinTargets(winningNumber);

    const animation = Animated.parallel([
      Animated.timing(wheelRotation, {
        toValue: wheel,
        duration: SPIN_DURATION_MS,
        easing: Easing.out(Easing.cubic),
        useNativeDriver: true,
      }),
      Animated.timing(ballRotation, {
        toValue: ball,
        duration: SPIN_DURATION_MS,
        easing: Easing.out(Easing.cubic),
        useNativeDriver: true,
      }),
    ]);

    animation.start(({ finished }) => {
      if (!finished) return;
      wheelRotation.setValue(wheel % 360);
      ballRotation.setValue(ball % 360);
      notifyStop();
    });

    return () => animation.stop();
  }, [spinning, winningNumber, wheelRotation, ballRotation, notifyStop]);

  const wheelRotate = wheelRotation.interpolate({
    inputRange: [-360, 360],
    outputRange: ["-360deg", "360deg"],
  });

  const ballAngle = ballRotation.interpolate({
    inputRange: [-360, 360],
    outputRange: ["-360deg", "360deg"],
  });

  const handleLayout = (e: LayoutChangeEvent) => {
    setParentSize(e.nativeEvent.layout.width);
  };

  return (
    <View style={styles.container} onLayout={handleLayout}>
      {size > 0 && (
        <View
          style={{
            width: size,
            height: size,
            alignItems: "center",
            justifyContent: "center",
            alignSelf: "center",
          }}
        >
          <Animated.Image
            source={WHEEL}
            style={{
              width: size,
              height: size,
              transform: [{ rotate: wheelRotate }, { rotate: IMAGE_ROTATION }],
            }}
            resizeMode="contain"
          />

          <Animated.View
            style={[
              styles.ballOrbit,
              {
                width: ballOrbitRadius * 2,
                height: ballOrbitRadius * 2,
                transform: [{ rotate: ballAngle }],
              },
            ]}
            pointerEvents="none"
          >
            <View
              style={[
                styles.ball,
                {
                  width: ballSize,
                  height: ballSize,
                  borderRadius: ballSize / 2,
                  top: -ballSize / 2,
                  left: ballOrbitRadius - ballSize / 2,
                },
              ]}
            />
          </Animated.View>

          <View style={[styles.pointer, { top: -size * POINTER_TOP_RATIO }]}>
            <Text
              style={{ fontSize: size * POINTER_FONT_RATIO, color: "#E63946" }}
            >
              ▼
            </Text>
          </View>
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    width: "100%",
    alignItems: "center",
    justifyContent: "center",
    alignSelf: "center",
  },
  ballOrbit: {
    position: "absolute",
    alignItems: "center",
    justifyContent: "center",
  },
  ball: {
    position: "absolute",
    backgroundColor: "#FFFFFF",
    borderWidth: 1,
    borderColor: "#CCCCCC",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.4,
    shadowRadius: 3,
    elevation: 5,
  },
  pointer: {
    position: "absolute",
    zIndex: 10,
    alignSelf: "center",
  },
});
