import { useState } from "react";
import {
  Platform,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import DateTimePicker from "@react-native-community/datetimepicker";
import { Ionicons } from "@expo/vector-icons";
import { useTheme } from "../../theme/ThemeProvider";

interface Props {
  label: string;
  value: Date;
  onChange: (date: Date) => void;
}

export const DatePicker = ({ label, value, onChange }: Props) => {
  const theme = useTheme();
  const [show, setShow] = useState(false);

  return (
    <View style={styles.container}>
      <Text style={[styles.label, { color: theme.textSecondary }]}>
        {label}
      </Text>
      <TouchableOpacity
        style={[
          styles.trigger,
          { backgroundColor: theme.card, borderColor: theme.border },
        ]}
        onPress={() => setShow(true)}
      >
        <Text style={{ color: theme.text }}>{value.toLocaleDateString()}</Text>
        <Ionicons name="calendar-outline" size={20} color={theme.text} />
      </TouchableOpacity>

      {show && (
        <DateTimePicker
          value={value}
          mode="date"
          display={Platform.OS === "ios" ? "spinner" : "default"}
          onChange={(_, date) => {
            setShow(false);
            if (date) onChange(date);
          }}
        />
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: { marginBottom: 16 },
  label: { fontSize: 14, marginBottom: 6 },
  trigger: {
    borderWidth: 1,
    borderRadius: 10,
    padding: 12,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
});
