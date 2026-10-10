import { StyleSheet, Text, TouchableOpacity } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useTheme } from "../../theme/ThemeProvider";

interface Props {
  checked: boolean;
  onToggle: () => void;
  label: string;
}

export const Checkbox = ({ checked, onToggle, label }: Props) => {
  const theme = useTheme();

  return (
    <TouchableOpacity style={styles.container} onPress={onToggle}>
      <Ionicons
        name={checked ? "checkbox" : "square-outline"}
        size={24}
        color={checked ? theme.primary : theme.border}
      />
      <Text style={[styles.label, { color: theme.text }]}>{label}</Text>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: { flexDirection: "row", alignItems: "center", marginBottom: 12 },
  label: { marginLeft: 8, fontSize: 16 },
});
