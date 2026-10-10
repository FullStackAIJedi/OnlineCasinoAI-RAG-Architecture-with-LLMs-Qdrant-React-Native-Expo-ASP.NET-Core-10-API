import { StyleSheet, Text, TouchableOpacity } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useTheme } from "../../theme/ThemeProvider";

interface Props {
  selected: boolean;
  onSelect: () => void;
  label: string;
}

export const Radio = ({ selected, onSelect, label }: Props) => {
  const theme = useTheme();

  return (
    <TouchableOpacity style={styles.container} onPress={onSelect}>
      <Ionicons
        name={selected ? "radio-button-on" : "radio-button-off"}
        size={24}
        color={selected ? theme.primary : theme.border}
      />
      <Text style={[styles.label, { color: theme.text }]}>{label}</Text>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: { flexDirection: "row", alignItems: "center", marginBottom: 12 },
  label: { marginLeft: 8, fontSize: 16 },
});
