import React from "react";
import { Modal as RNModal, StyleSheet, Text, TouchableOpacity } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";

interface Props {
  visible: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  variant?: "default" | "success" | "danger" | "warning";

  backdropColor?: string;

  background?: string;

  borderColor?: string;
}

export const Modal = ({
  visible,
  onClose,
  title,
  children,
  variant = "default",
  background,
  borderColor,
  backdropColor,
}: Props) => {
  const theme = useTheme();

  // Цвят на бордера според variant (може да се override-не)
  const resolvedBorderColor =
    borderColor ??
    (variant === "success"
      ? theme.success
      : variant === "danger"
      ? theme.danger
      : variant === "warning"
      ? theme.warning
      : theme.border);

  // Цвят на фона – override или tinted според variant
  const resolvedBackground =
    background ??
    (variant === "success"
      ? theme.success + "11"
      : variant === "danger"
      ? theme.danger + "11"
      : variant === "warning"
      ? theme.warning + "11"
      : theme.card);

  // Цвят на backdrop-а – override или от темата
  const resolvedBackdropColor = backdropColor ?? theme.overlay;

  return (
    <RNModal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
      <TouchableOpacity
        style={[styles.backdrop, { backgroundColor: resolvedBackdropColor }]}
        activeOpacity={1}
        onPress={onClose}
      >
        <TouchableOpacity
          activeOpacity={1}
          style={[
            styles.content,
            {
              backgroundColor: resolvedBackground,
              borderColor: resolvedBorderColor,
            },
          ]}
        >
          {title && (
            <Text style={[styles.title, { color: resolvedBorderColor }]}>{title}</Text>
          )}
          {children}
        </TouchableOpacity>
      </TouchableOpacity>
    </RNModal>
  );
};

const styles = StyleSheet.create({
  backdrop: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 20,
  },
  content: {
    width: "100%",
    maxWidth: 400,
    borderRadius: 30,
    borderWidth: 4,
    padding: 20,
  },
  title: {
    fontSize: 30,
    fontWeight: "900",
    marginBottom: 16,
  },
});
