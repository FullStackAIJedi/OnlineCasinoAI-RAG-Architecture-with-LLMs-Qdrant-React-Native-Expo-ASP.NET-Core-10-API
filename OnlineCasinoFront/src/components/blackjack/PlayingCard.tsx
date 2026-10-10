// import { Image, StyleSheet, View } from "react-native";
// import { getCardImage } from "./cardImages";

// interface PlayingCardProps { 
//   card: string; 
//   size?: "small" | "medium" | "large";
// }

// const SIZES = {
//   small:  { width: 50,  height: 70  },
//   medium: { width: 70,  height: 98  },
//   large:  { width: 100, height: 140 },
// };

// export const PlayingCard = ({ card, size = "medium" }: PlayingCardProps) => {
//   const { width, height } = SIZES[size];

//   return (
//     <View style={[styles.card, { width, height }]}>
//       <Image
//         source={getCardImage(card)}
//         style={styles.image}
//         resizeMode="contain"
//       />
//     </View>
//   );
// };

// const styles = StyleSheet.create({
//   card: {
//     borderRadius: 6,
//     overflow: "hidden",
//     marginRight: 4,
//     backgroundColor: "#FFF",
//     shadowColor: "#000",
//     shadowOffset: { width: 0, height: 2 },
//     shadowOpacity: 0.2,
//     shadowRadius: 3,
//     elevation: 3,
//   },
//   image: {
//     width: "100%",
//     height: "100%",
//   },
// });

import { Image, StyleSheet, View } from "react-native"; 
import { getCardImage } from "./cardImages";

export type PlayingCardSize = "small" | "medium" | "large";

interface PlayingCardProps {
  card: string;
  size?: PlayingCardSize;
}

const SIZES: Record<PlayingCardSize, { width: number; height: number }> = {
  small: { width: 50, height: 70 },
  medium: { width: 70, height: 98 },
  large: { width: 100, height: 140 },
};

/**
 * Renders a single playing card image at a given size.
 *
 * @param props - Component props.
 * @param props.card - Card code (e.g. "A♠"). Falls back to the card back if unknown.
 * @param props.size - Visual size of the card. Defaults to "medium".
 * @returns The rendered card.
 */
export const PlayingCard = ({ card, size = "medium" }: PlayingCardProps) => {
  const { width, height } = SIZES[size];

  return (
    <View style={[styles.card, { width, height }]}>
      <Image
        source={getCardImage(card)}
        style={styles.image}
        resizeMode="contain"
      />
    </View>
  );
};

const styles = StyleSheet.create({
  card: {
    borderRadius: 6,
    overflow: "hidden",
    marginRight: 4,
    backgroundColor: "#FFF",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.2,
    shadowRadius: 3,
    elevation: 3,
  },
  image: {
    width: "100%",
    height: "100%",
  },
});
