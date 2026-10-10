import { useState } from "react";
import {
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { Ionicons } from "@expo/vector-icons";

import { useTheme } from "../../theme/ThemeProvider";

const DEFAULT_PLACEHOLDER = "Choose...";

interface Option<T extends string> {
  label: string;
  value: T;
}

interface Props<T extends string> {
  label: string;
  options: Option<T>[];
  value: T;
  onChange: (value: T) => void;
  placeholder?: string;
}

/**
 * Select input with a dropdown list.
 *
 * @param props - Component props.
 * @param props.label - Label displayed above the trigger.
 * @param props.options - Available options.
 * @param props.value - Currently selected value.
 * @param props.onChange - Called with the new value when an option is picked.
 * @param props.placeholder - Text shown when no value is selected. Defaults to "Choose...".
 * @returns The rendered dropdown.
 */
export const Dropdown = <T extends string>({
  label,
  options,
  value,
  onChange,
  placeholder = DEFAULT_PLACEHOLDER,
}: Props<T>) => {
  const theme = useTheme();
  const [open, setOpen] = useState(false);
  const selected = options.find((o) => o.value === value);

  const handleToggle = () => setOpen((prev) => !prev);

  const handleSelect = (next: T) => {
    onChange(next);
    setOpen(false);
  };

  return (
    <View style={styles.container}>
      <Text style={[styles.label, { color: theme.card }]}>{label}</Text>

      <TouchableOpacity
        style={[
          styles.trigger,
          { backgroundColor: theme.card, borderColor: theme.border },
        ]}
        onPress={handleToggle}
      >
        <Text style={{ color: theme.text }}>
          {selected?.label ?? placeholder}
        </Text>
        <Ionicons
          name={open ? "chevron-up" : "chevron-down"}
          size={20}
          color={theme.text}
        />
      </TouchableOpacity>

      {open && (
        <View
          style={[
            styles.list,
            { backgroundColor: theme.card, borderColor: theme.border },
          ]}
        >
          <ScrollView style={styles.scroll}>
            {options.map((option) => (
              <TouchableOpacity
                key={option.value}
                style={[styles.item, { borderBottomColor: theme.border }]}
                onPress={() => handleSelect(option.value)}
              >
                <Text style={{ color: theme.text }}>{option.label}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: { marginBottom: 16 },
  label: { fontSize: 14, marginBottom: 6, fontWeight: "600" },
  trigger: {
    borderWidth: 1,
    borderRadius: 16,
    paddingVertical: 17,
    paddingHorizontal: 12,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  list: { borderWidth: 1, borderRadius: 10, marginTop: 4, overflow: "hidden" },
  scroll: { maxHeight: 200 },
  item: { padding: 12, borderBottomWidth: 1 },
});
