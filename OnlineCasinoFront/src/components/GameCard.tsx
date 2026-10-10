import { ImageBackground, StyleSheet, Text, TouchableOpacity, View } from "react-native";

interface Props {
  code: string;
  name: string;
  image: any;             
  onPress: () => void;
}

export const GameCard = ({ code, name, image, onPress }: Props) => {
  return (
    <TouchableOpacity
      style={styles.card}
      onPress={onPress}
      activeOpacity={0.85}
    >
      <ImageBackground
        source={image}
        style={styles.image}
        imageStyle={styles.imageInner}
        resizeMode="cover"
      > 
        <View style={styles.overlay} /> 
        <View style={styles.content}>
          <Text style={styles.title}>{name}</Text>
        </View>
      </ImageBackground>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  card: {
    width: "48%",
    aspectRatio: 1,
    borderRadius: 20,
    marginBottom: 16,
    overflow: "hidden",   
  },
  image: {
    flex: 1,
    justifyContent: "flex-end", 
  },
  imageInner: {
    borderRadius: 20,     
  },
  overlay: {
    position: "absolute",
    top: 0, left: 0, right: 0, bottom: 0,
    backgroundColor: "rgba(0, 0, 0, 0.35)",    
  },
  content: {
    padding: 14,
  },
  title: {
    color: "#FFF",
    fontSize: 14,
    fontWeight: "900",
    textAlign: "center",
    textShadowColor: "rgba(0, 0, 0, 0.8)",   
    textShadowOffset: { width: 0, height: 2 },
    textShadowRadius: 4,
  },
});
