import { KeyboardTypeOptions, StyleSheet, Text, TextInput, View } from "react-native";
import { useTheme } from "../../theme/ThemeProvider";

interface Props {
  label?: string;
  value: string;
  onChangeText: (t: string) => void;
  placeholder?: string;
  secureTextEntry?: boolean;
  error?: string;
  keyboardType?: KeyboardTypeOptions;
  autoCapitalize?: "none" | "sentences" | "words" | "characters";
  labelColor?: string;
  borderColor?: string;
  backgroundColor?: string;
  placeholderColor?: string; 
}

export const Input = ({
  label,
  value,
  onChangeText,
  placeholder, 
  labelColor = "#ffffff",                     
  secureTextEntry,
  error,
  keyboardType,
  autoCapitalize = "none",
  borderColor = "#000000",
  backgroundColor,                         
  placeholderColor = "rgba(213, 203, 224, 0.5)",
}: Props) => {
  const theme = useTheme();
  return (
    <View style={styles.container}>
      {label && (
                <Text style={[styles.label, { color: labelColor }]}>  
          {label}
        </Text>
      )}
      <TextInput
        style={[
          styles.input,
          {
            borderColor: error ? theme.danger : borderColor,
            backgroundColor: backgroundColor ?? theme.card,     
            color: theme.text,                                     
          },
        ]}
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        placeholderTextColor={placeholderColor}
        secureTextEntry={secureTextEntry}
        keyboardType={keyboardType}
        autoCapitalize={autoCapitalize}
      />
      {error && (
        <Text style={{ color: theme.danger }}>
          {error}
        </Text>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: { marginBottom: 16 },
  label: {
    fontSize: 14,
    marginBottom: 6,
    fontWeight: "600",
  },
  input: {
    borderWidth: 1.5,
    borderRadius: 16,
    paddingHorizontal: 16,
    paddingVertical: 14,
    fontSize: 16,
  },
  error: {
    fontSize: 12,
    marginTop: 4,
  },
});
