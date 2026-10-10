import { useState } from "react";
import { ImageBackground, KeyboardAvoidingView, Platform,
  ScrollView, StyleSheet, Text, TouchableOpacity, View } from "react-native";
import { useRouter } from "expo-router";
import { SafeAreaView } from "react-native-safe-area-context";

import { Ionicons } from "@expo/vector-icons";
import { Input } from "../../src/components/ui/Input";
import { Button } from "../../src/components/ui/Button";
import { Modal } from "../../src/components/ui/Modal";

import { useRegister } from "../../src/hooks/useAuth";
import { useTheme } from "../../src/theme/ThemeProvider";

export default function RegisterScreen() {
  const theme = useTheme();
  const router = useRouter();

  const [email, setEmail] = useState("");
  const [nickName, setNickName] = useState("");
  const [password, setPassword] = useState("");

  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successVisible, setSuccessVisible] = useState(false);

  const register = useRegister();

  const handleRegister = () => {
    if (!email || !nickName || !password) {
      setErrorMessage("Please fill in all fields.");
      return;
    }

    register.mutate(
      { email, nickName, password },
      {
        onSuccess: () => setSuccessVisible(true),
        onError: (err) => setErrorMessage(err.message || "Registration failed"),
      }
    );
  };

  const handleSuccessClose = () => {
    setSuccessVisible(false);
    router.replace("/(tabs)");
  };

  return (
    <ImageBackground
      source={require("../../assets/backgrounds/SignUpGirl.jpg")}
      style={styles.background}
      resizeMode="cover"
    >
      <View style={styles.overlay} />

      <SafeAreaView style={styles.container}>
        <KeyboardAvoidingView
          behavior={Platform.OS === "ios" ? "padding" : "height"}
          style={{ flex: 1 }}
        >
          <ScrollView contentContainerStyle={styles.scroll} showsVerticalScrollIndicator={false}>
            <TouchableOpacity onPress={() => router.back()} style={styles.back}>
              <Ionicons name="arrow-back" size={24} color="#fff" />
            </TouchableOpacity>

            <View style={styles.header}>
              <Text style={[styles.title, { color: "#fff" }]}>Sign up</Text>
            </View>

            <Input
              value={email}
              onChangeText={setEmail}
              placeholder="Email"
              keyboardType="email-address"
              autoCapitalize="none"
            />
            <Input value={nickName} onChangeText={setNickName} placeholder="Nickname" />
            <Input
              value={password}
              onChangeText={setPassword}
              placeholder="Password"
              secureTextEntry
            />

            <Button
              title="Sign up"
              onPress={handleRegister}
              loading={register.isPending}
            />

            <TouchableOpacity onPress={() => router.back()} style={styles.link}>
              <Text style={[styles.linkText, { color: "#fff" }]}>
                Have an account? Sign in
              </Text>
            </TouchableOpacity>
          </ScrollView>
        </KeyboardAvoidingView>
      </SafeAreaView>

      <Modal
        visible={!!errorMessage}
        onClose={() => setErrorMessage(null)}
        title="Error"
        variant="danger"
      >
        <Text style={[styles.modalText, { color: theme.text }]}>
          {errorMessage}
        </Text>
        <Button title="OK" onPress={() => setErrorMessage(null)} />
      </Modal>

      <Modal
        visible={successVisible}
        onClose={handleSuccessClose}
        title="Welcome!"
        variant="success"
      >
        <Text style={[styles.modalText, { color: theme.text }]}>
          Your account has been created successfully.
        </Text>
        <Button title="Continue" onPress={handleSuccessClose} />
      </Modal>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  background: { flex: 1, width: "100%", height: "100%" },
  overlay: {
    ...StyleSheet.absoluteFill,
    backgroundColor: "rgba(41, 40, 90, 0.55)",
  },
  container: { flex: 1 },
  scroll: { padding: 24, flexGrow: 1, justifyContent: "center" },
  back: { position: "absolute", top: 8, left: 8, padding: 8, zIndex: 10 },
  header: { alignItems: "center", marginBottom: 40 },
  title: { fontSize: 32, fontWeight: "900", marginBottom: 20 },
  link: { marginTop: 20, alignItems: "center" },
  linkText: { fontSize: 15, fontWeight: "600" },
  modalText: { fontSize: 15, marginBottom: 20, lineHeight: 22 },
});
