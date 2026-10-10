import React from "react";
import {
  ActivityIndicator,
  StyleSheet,
  Text,
  TextStyle,
  TouchableOpacity,
  ViewStyle,
} from "react-native";
import { useTheme } from "../../theme/ThemeProvider";

interface Props {
  title: string;
  onPress: () => void;
  loading?: boolean;
  disabled?: boolean;
  icon?: React.ReactNode;
  style?: ViewStyle;
  textStyle?: TextStyle;
}

export const Button = ({
  title,
  onPress,
  loading,
  disabled,
  icon,
  style,
  textStyle,
}: Props) => {
  const theme = useTheme();

  return (
    <TouchableOpacity
      style={[
        styles.button,
        {
          backgroundColor: theme.card,
          borderColor: theme.card,
          opacity: disabled || loading ? 0.5 : 1,
        },
        style,
      ]}
      onPress={onPress}
      disabled={disabled || loading}
      activeOpacity={0.7}
    >
      {loading ? (
        <ActivityIndicator color={theme.text} />
      ) : (
        <>
          {icon}
          <Text
            style={[
              styles.text,
              { color: theme.text, marginLeft: icon ? 8 : 0 },
              textStyle,   
            ]}
          >
            {title}
          </Text>
        </>
      )}
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  button: {
    paddingVertical: 16,
    paddingHorizontal: 16,
    borderRadius: 16,
    alignItems: "center",
    justifyContent: "center",
    flexDirection: "row",
    borderWidth: 1,
  },
  text: { fontSize: 11, fontWeight: "600" },
});
