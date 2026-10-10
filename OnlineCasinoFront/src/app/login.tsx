import { useState } from "react";
import {
  ImageBackground,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { useRouter } from "expo-router";
import { SafeAreaView } from "react-native-safe-area-context";

import { Input } from "../components/ui/Input";
import { Button } from "../components/ui/Button";
import { Modal } from "../components/ui/Modal";

import { useLogin } from "../hooks/useAuth";
import { useTheme } from "../theme/ThemeProvider";

/**
 * Login screen.
 *
 * On successful sign-in, auth state is invalidated and Expo Router
 * redirects the user to the main app via `Stack.Protected`.
 *
 * @returns The rendered login screen.
 */
export default function LoginScreen() {
  const theme = useTheme();
  const router = useRouter();
  const login = useLogin();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<{ email?: string; password?: string }>(
    {},
  );
  const [serverError, setServerError] = useState<string | null>(null);

  const validate = () => {
    const newErrors: typeof errors = {};
    if (!email.trim()) newErrors.email = "Email is required";
    else if (!/^\S+@\S+\.\S+$/.test(email)) newErrors.email = "Invalid email";
    if (!password) newErrors.password = "Password is required";
    else if (password.length < 6) newErrors.password = "Minimum 6 characters";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };
  const handleLogin = () => {
    setServerError(null);
    if (!validate()) {
      return;
    }

    login.mutate(
      { email, password },
      {
        onError: (err) => {
          setServerError(err.message || "Login failed");
        },
      },
    );
  };

  return (
    <ImageBackground
      source={require("../../assets/backgrounds/SignInGirl.jpg")}
      style={styles.background}
      resizeMode="cover"
    >
      <View style={styles.overlay} />

      <SafeAreaView style={styles.container}>
        <KeyboardAvoidingView
          behavior={Platform.OS === "ios" ? "padding" : "height"}
          style={{ flex: 1 }}
        >
          <ScrollView
            contentContainerStyle={styles.scroll}
            showsVerticalScrollIndicator={false}
          >
            <View style={styles.header}>
              <Text style={[styles.title, { color: "#fff" }]}>Casino</Text>
              <Text
                style={[styles.subtitle, { color: "rgba(255,255,255,0.85)" }]}
              >
                Sign in
              </Text>
            </View>

            <Input
              value={email}
              onChangeText={(t) => {
                setEmail(t);
                if (errors.email) setErrors({ ...errors, email: undefined });
              }}
              placeholder="Email"
              keyboardType="email-address"
              autoCapitalize="none"
              error={errors.email}
            />
            <Input
              value={password}
              onChangeText={(t) => {
                setPassword(t);
                if (errors.password)
                  setErrors({ ...errors, password: undefined });
              }}
              placeholder="Password"
              secureTextEntry
              error={errors.password}
            />

            <Button
              title="Sign in"
              onPress={handleLogin}
              loading={login.isPending}
              style={{ backgroundColor: "#080112", borderColor: "#080112" }}
            />

            <TouchableOpacity
              onPress={() => router.push("/register")}
              style={styles.link}
            >
              <Text style={[styles.linkText, { color: "#fff" }]}>
                No account? Sign up
              </Text>
            </TouchableOpacity>
          </ScrollView>
        </KeyboardAvoidingView>
      </SafeAreaView>

      <Modal
        visible={!!serverError}
        onClose={() => setServerError(null)}
        title="Login failed"
        variant="danger"
      >
        <Text style={[styles.modalText, { color: theme.text }]}>
          {serverError}
        </Text>
        <Button title="OK" onPress={() => setServerError(null)} />
      </Modal>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  background: { flex: 1, width: "100%", height: "100%" },
  overlay: {
    ...StyleSheet.absoluteFill,
    backgroundColor: "rgba(29, 28, 71, 0.55)",
  },
  container: { flex: 1 },
  scroll: { padding: 24, flexGrow: 1, justifyContent: "center" },
  header: { alignItems: "center", marginBottom: 110 },
  title: { fontSize: 44, fontWeight: "900", marginTop: 36 },
  subtitle: { fontSize: 26, marginTop: 1 },
  link: { marginTop: 20, alignItems: "center" },
  linkText: { fontSize: 15, fontWeight: "600" },
  modalText: { fontSize: 15, marginBottom: 20, lineHeight: 22 },
});
