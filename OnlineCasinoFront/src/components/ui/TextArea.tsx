import { StyleSheet, Text, TextInput, View } from "react-native";
import { useTheme } from "../../theme/ThemeProvider";

interface Props {
  label?: string;
  value: string;
  onChangeText: (t: string) => void;
  placeholder?: string;
  error?: string;
  numberOfLines?: number;
}

export const TextArea = ({ label, value, onChangeText, placeholder, error, numberOfLines = 4 }: Props) => {
  const theme = useTheme();

  return (
    <View style={styles.container}>
      {label && <Text style={[styles.label, { color: theme.textSecondary }]}>{label}</Text>}
      <TextInput
        style={[
          styles.input,
          {
            backgroundColor: theme.card,
            color: theme.text,
            borderColor: error ? theme.danger : theme.border,
            minHeight: numberOfLines * 24,
          },
        ]}
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        placeholderTextColor={theme.textSecondary}
        multiline
        numberOfLines={numberOfLines}
        textAlignVertical="top"
      />
      {error && <Text style={[styles.error, { color: theme.danger }]}>{error}</Text>}
    </View>
  );
};

const styles = StyleSheet.create({
  container: { marginBottom: 16 },
  label: { fontSize: 14, marginBottom: 6 },
  input: { borderWidth: 1, borderRadius: 10, padding: 12, fontSize: 16 },
  error: { fontSize: 12, marginTop: 4 },
});
