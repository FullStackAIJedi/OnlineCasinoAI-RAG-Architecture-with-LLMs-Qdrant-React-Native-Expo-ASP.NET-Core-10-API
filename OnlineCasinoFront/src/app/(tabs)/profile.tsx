// import { useEffect, useState } from "react";
// import { useRouter } from "expo-router";
// import { Image, ImageBackground, ScrollView, StyleSheet, Switch, Text, View } from "react-native";
// import { SafeAreaView } from "react-native-safe-area-context";
 
// import { Ionicons } from "@expo/vector-icons";

// import { useTheme, useThemeMode } from "../../../src/theme/ThemeProvider";
// import { Input } from "../../../src/components/ui/Input";
// import { Button } from "../../../src/components/ui/Button";
// import { Modal } from "../../../src/components/ui/Modal";

// import { useProfile, useUpdateProfile } from "../../../src/hooks/useUsers";
// import { useLogout, useChangePassword } from "../../../src/hooks/useAuth";

// export default function ProfileScreen() {
//   const theme = useTheme();
//   const router = useRouter();

//   const [firstName, setFirstName] = useState("");
//   const [lastName, setLastName] = useState("");
//   const [pofileImage, setPofileImage] = useState("");
//   const [pwdModal, setPwdModal] = useState(false);
//   const [currentPwd, setCurrentPwd] = useState("");
//   const [newPwd, setNewPwd] = useState("");

//   const [successMessage, setSuccessMessage] = useState<string | null>(null);
//   const [errorMessage, setErrorMessage] = useState<string | null>(null);
//   const [logoutModal, setLogoutModal] = useState(false);

//   const { mode, setMode } = useThemeMode();
//   const { data: profile } = useProfile();
//   const updateProfile = useUpdateProfile();
//   const logout = useLogout();
//   const changePassword = useChangePassword();

//   useEffect(() => {
//     if (profile) {
//       setFirstName(profile.firstName ?? "");
//       setLastName(profile.lastName ?? "");
//       setPofileImage(profile.pofileImage ?? "");
//     }
//   }, [profile]);

//   const handleSave = () => {
//     updateProfile.mutate(
//       { firstName, lastName, pofileImage },
//       {
//         onSuccess: () => setSuccessMessage("Profile updated"),
//         onError: (err: any) => setErrorMessage(err.message || "Update failed"),
//       }
//     );
//   };

//   const handleChangePwd = () => {
//     changePassword.mutate(
//       { currentPassword: currentPwd, newPassword: newPwd },
//       {
//         onSuccess: () => {
//           setPwdModal(false);
//           setCurrentPwd("");
//           setNewPwd("");
//           setSuccessMessage("Password changed");
//         },
//         onError: (err: any) => setErrorMessage(err.message || "Change failed"),
//       }
//     );
//   };

//   const handleLogout = () => {
//     setLogoutModal(true);
//   };

//   const confirmLogout = async () => {
//     setLogoutModal(false);
//     await logout.mutateAsync();
//     router.replace("/");
//   };

//   return (
//     <ImageBackground
//       source={require("../../../assets/backgrounds/profile.jpg")}
//       style={styles.background}
//       resizeMode="cover"
//     > 
//       <View style={styles.overlay} />

//       <SafeAreaView style={styles.container} edges={["top"]}>
//         <ScrollView contentContainerStyle={{ padding: 16 }}>
//          <View style={styles.container}> 
//             {pofileImage ? (
//               <Image source={{ uri: pofileImage }} style={styles.avatar} />
//             ) : (
//               <Ionicons
//                 name="person-circle"
//                 size={80}
//                 color={theme.primary}
//                 style={{ alignSelf: "center" }}
//               />
//             )}

//             <Text style={[styles.name, { color: theme.text }]}>
//               {profile?.nickName ?? "-"}
//             </Text>
//             <Text style={[styles.email, { color: theme.card }]}>
//               {profile?.email ?? "-"}
//             </Text>
//             <View style={[styles.roleBadge, { backgroundColor: theme.card, }]}>
//               <Text style={[styles.roleText, { color: theme.text }]}>{profile?.role ?? "User"}</Text>
//             </View>
//           </View>

//           <Text style={[styles.sectionTitle, { color: theme.text }]}>
//             Personal data
//           </Text>
//           <Input
//             label="First name"
//             value={firstName}
//             onChangeText={setFirstName}
//             placeholder="Johnnie"
//             autoCapitalize="words"
//           />
//           <Input
//             label="Last name" 
//             value={lastName}
//             onChangeText={setLastName}
//             placeholder="Walker"
//             autoCapitalize="words"
//           />
//           <Input
//             label="Profile image URL"
//             value={pofileImage}
//             onChangeText={setPofileImage}
//             placeholder="Profile image url .."
//             autoCapitalize="none"
//             keyboardType="url"
//           />
//             <View style={{ alignItems: "flex-start", marginTop: 8 }}>
//               <Button
//                 title="Save"
//                 onPress={handleSave}
//                 loading={updateProfile.isPending}
//                 style={{
//                   borderColor: theme.success,
//                   borderWidth: 2,
//                   paddingHorizontal: 23,
//                   paddingVertical: 12,
//                   alignSelf: "flex-start",
//                 }}
//                 textStyle={{
//                   color: theme.success,
//                   fontSize: 16,
//                   fontWeight: "900",
//                 }}
//               />
//             </View>

//             <Text 
//               style={[styles.sectionTitle, { color: theme.text, marginTop: 60 }]}
//               >
//               Security
//             </Text>
//             <Button
//               title="Change password" 
//               onPress={() => setPwdModal(true)}
//               style={{
//                   borderColor: theme.warning, 
//                   alignSelf: "flex-start",
//                   paddingHorizontal: 23, 
//                 }}
//                 textStyle={{
//                   color: theme.warning,
//                   fontSize: 16,
//                   fontWeight: "700",
//                 }}
//             />

//             <Text
//               style={[styles.sectionTitle, { color: theme.text, marginTop: 60 }]}
//             >
//               Settings
//             </Text>

//             <View
//               style={[
//                 styles.settingRow,
//                 { backgroundColor: theme.card, borderColor: theme.border },
//               ]}
//             >
//               <Ionicons
//                 name={mode === "dark" ? "moon" : "sunny"}
//                 size={22}
//                 color={theme.text}
//               />
//               <Text style={[styles.settingText, { color: theme.text }]}>
//                 Dark theme
//               </Text>
//               <Switch
//                 value={mode === "dark"}
//                 onValueChange={(value) => setMode(value ? "dark" : "light")}
//               />
//             </View>

//             <View style={{ marginTop: 24 }}>
//               <Button
//                 title="Logout" 
//                 onPress={handleLogout}
//                 loading={logout.isPending}
//                 style={{
//                   borderColor: theme.danger,
//                   borderWidth: 2,
//                   paddingHorizontal: 23, 
//                   alignSelf: "flex-start",
//                 }}
//                 textStyle={{
//                   color: theme.danger,
//                   fontSize: 14,
//                   fontWeight: "700",
//                 }}
//               />
//             </View>
//         </ScrollView>

//         <Modal
//           visible={pwdModal}
//           onClose={() => setPwdModal(false)}
//           title="Change password"
//           variant="warning"
//         >
//           <Input
//             label="Current password"
//             value={currentPwd}
//             onChangeText={setCurrentPwd}
//             secureTextEntry
//             labelColor={theme.warning}
//             borderColor={theme.warning}
//             backgroundColor={theme.warning}
//             placeholderColor="rgba(255, 255, 255, 0.4)"
//           />
//           <Input
//             label="New password"
//             value={newPwd}
//             onChangeText={setNewPwd}
//             secureTextEntry
//             labelColor={theme.warning}
//             borderColor={theme.warning}
//             backgroundColor={theme.warning}
//             placeholderColor="rgba(255, 255, 255, 0.4)"
//           />
//           <Button
//             title="Change"
//             onPress={handleChangePwd}
//             loading={changePassword.isPending}
//             style={{
//               borderRadius: 16,
//               paddingHorizontal: 25, 
//             }}
//           />
//         </Modal>

//         <Modal
//           visible={!!successMessage}
//           onClose={() => setSuccessMessage(null)}
//           title="Success"
//           variant="success"
//         >
//           <Text style={{ color: theme.card, fontSize: 16, marginBottom: 20, lineHeight: 22 }}>
//             {successMessage}
//           </Text>
//           <Button title="OK" onPress={() => setSuccessMessage(null)} />
//         </Modal>

//         <Modal
//           visible={!!errorMessage}
//           onClose={() => setErrorMessage(null)}
//           title="Error"
//           variant="danger"
//         >
//           <Text style={{ color: theme.card, fontSize: 16, marginBottom: 20, lineHeight: 22 }}>
//             {errorMessage}
//           </Text>
//           <Button title="OK" onPress={() => setErrorMessage(null)} />
//         </Modal>

//         <Modal
//           visible={logoutModal}
//           onClose={() => setLogoutModal(false)}
//           title="Logout"
//           variant="danger"
//         >
//           <Text style={{ color: theme.card, fontSize: 16, marginBottom: 20, lineHeight: 22 }}>
//             Ready to log out?
//           </Text>
//           <View style={{ flexDirection: "row", gap: 12, justifyContent: "flex-end" }}>
//             <Button
//               title="Cancel"
//               onPress={() => setLogoutModal(false)}
//               style={{ borderColor: theme.border, borderWidth: 2 }}
//               textStyle={{ color: theme.text }}
//             />
//             <Button
//               title="Logout"
//               onPress={confirmLogout}
//               loading={logout.isPending}
//               style={{ borderColor: theme.danger, borderWidth: 2, }}
//               textStyle={{ color: theme.danger }}
//             />
//           </View>
//         </Modal>
//       </SafeAreaView>
//     </ImageBackground>
//   );
// }

// const styles = StyleSheet.create({
//   background: { flex: 1, width: "100%", height: "100%" },
//   overlay: {
//     position: "absolute",
//     top: 0,
//     left: 0,
//     right: 0,
//     bottom: 0, 
//   },
//   container: { flex: 1, marginBottom: 50, }, 
//   avatar: {
//     width: 280,
//     height: 280,
//     borderRadius: 140,
//     borderWidth: 4,
//     borderColor: "#0c0115",
//     alignSelf: "center",
//     backgroundColor: "#ccc",
//   },
//   name: { fontSize: 22, fontWeight: "800", marginTop: 12 },
//   email: { fontSize: 16, fontWeight: "800" },
//   roleBadge: {
//     paddingHorizontal: 16,
//     paddingVertical: 8,
//     borderRadius: 13,
//     marginTop: 8, 
//     alignSelf: "flex-start",
//   },
//   roleText: { fontSize: 18, fontWeight: "700" },
//   sectionTitle: { fontSize: 18, fontWeight: "700", marginBottom: 12 },
//   settingRow: {
//     flexDirection: "row",
//     alignItems: "center", 
//     paddingHorizontal: 16,
//     paddingVertical: 8,
//     borderRadius: 16,
//     borderWidth: 1,
//     gap: 12,
//   },
//   settingText: { flex: 1, fontSize: 16 },
// });

import { useState } from "react";
import {
  Image,
  ImageBackground,
  Keyboard,
  ScrollView,
  StyleSheet,
  Switch,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Ionicons } from "@expo/vector-icons";

import { useTheme, useThemeMode } from "../../../src/theme/ThemeProvider";
import { Input } from "../../../src/components/ui/Input";
import { Button } from "../../../src/components/ui/Button";
import { Modal } from "../../../src/components/ui/Modal";

import { useProfile, useUpdateProfile } from "../../../src/hooks/useUsers";
import { useLogout, useChangePassword } from "../../../src/hooks/useAuth";

interface ProfileForm {
  firstName: string;
  lastName: string;
  pofileImage: string;
}

const MODAL_BACKGROUND = "#0c0115";
const AVATAR_SIZE = 280;
const AVATAR_FALLBACK_BG = "#ccc";
const PLACEHOLDER_COLOR = "rgba(255, 255, 255, 0.4)";

/**
 * Profile screen with personal data, security, and settings sections.
 *
 * @returns The rendered profile screen.
 */
export default function ProfileScreen() {
  const theme = useTheme();

  const [formOverride, setFormOverride] = useState<ProfileForm | null>(null);
  const [pwdModal, setPwdModal] = useState(false);
  const [currentPwd, setCurrentPwd] = useState("");
  const [newPwd, setNewPwd] = useState("");
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [logoutModal, setLogoutModal] = useState(false);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const { mode, setMode } = useThemeMode();
  const { data: profile } = useProfile();
  const updateProfile = useUpdateProfile();
  const logout = useLogout();
  const changePassword = useChangePassword();

  const form: ProfileForm = formOverride ?? {
    firstName: profile?.firstName ?? "",
    lastName: profile?.lastName ?? "",
    pofileImage: profile?.pofileImage ?? "",
  };

  const handleFieldChange = (field: keyof ProfileForm, value: string) => {
    setFormOverride({ ...form, [field]: value });
  };

  const handleSave = () => {
    updateProfile.mutate(form, {
      onSuccess: () => {
        Keyboard.dismiss();
        setFormOverride(null);
        setSuccessMessage("Profile updated");
      },
      onError: (err: Error) => {
        Keyboard.dismiss();
        setErrorMessage(err.message || "Update failed");
      },
    });
  };

  const handleChangePwd = () => {
    changePassword.mutate(
      { currentPassword: currentPwd, newPassword: newPwd },
      {
        onSuccess: () => {
          setPwdModal(false);
          setCurrentPwd("");
          setNewPwd("");
          setSuccessMessage("Password changed");
        },
        onError: (err: Error) =>
          setErrorMessage(err.message || "Change failed"),
      }
    );
  };

  const confirmLogout = async () => {
    setLogoutModal(false);
    setIsLoggingOut(true);
    try {
      await logout.mutateAsync();
    } catch {
      setIsLoggingOut(false);
    }
  };

  if (isLoggingOut) return null;

  return (
    <ImageBackground
      source={require("../../../assets/backgrounds/profile.jpg")}
      style={styles.background}
      resizeMode="cover"
    >
      <View style={styles.overlay} />

      <SafeAreaView style={styles.container} edges={["top"]}>
        <ScrollView contentContainerStyle={styles.scroll}>
          <View style={styles.header}>
            {form.pofileImage ? (
              <Image source={{ uri: form.pofileImage }} style={styles.avatar} />
            ) : (
              <Ionicons
                name="person-circle"
                size={80}
                color={theme.primary}
                style={styles.avatarIcon}
              />
            )}

            <Text style={[styles.name, { color: theme.text }]}>
              {profile?.nickName ?? "-"}
            </Text>
            <Text style={[styles.email, { color: theme.card }]}>
              {profile?.email ?? "-"}
            </Text>
            <View style={[styles.roleBadge, { backgroundColor: theme.card }]}>
              <Text style={[styles.roleText, { color: theme.text }]}>
                {profile?.role ?? "User"}
              </Text>
            </View>
          </View>

          <Text style={[styles.sectionTitle, { color: theme.text }]}>
            Personal data
          </Text>
          <Input
            label="First name"
            value={form.firstName}
            onChangeText={(v) => handleFieldChange("firstName", v)}
            placeholder="Johnnie"
            autoCapitalize="words"
          />
          <Input
            label="Last name"
            value={form.lastName}
            onChangeText={(v) => handleFieldChange("lastName", v)}
            placeholder="Walker"
            autoCapitalize="words"
          />
          <Input
            label="Profile image URL"
            value={form.pofileImage}
            onChangeText={(v) => handleFieldChange("pofileImage", v)}
            placeholder="Profile image url .."
            autoCapitalize="none"
            keyboardType="url"
          />

          <View style={styles.saveWrapper}>
            <Button
              title="Save"
              onPress={handleSave}
              loading={updateProfile.isPending}
              style={{
                ...styles.saveButton,
                borderColor: theme.success,
              }}
              textStyle={{
                ...styles.saveText,
                color: theme.success,
              }}
            />
          </View>

          <Text
            style={[
              styles.sectionTitle,
              styles.sectionSpaced,
              { color: theme.text },
            ]}
          >
            Security
          </Text>
          <Button
            title="Change password"
            onPress={() => setPwdModal(true)}
            style={{
              borderColor: theme.warning,
              alignSelf: "flex-start",
              paddingHorizontal: 23,
            }}
            textStyle={{
              color: theme.warning,
              fontSize: 16,
              fontWeight: "700",
            }}
          />

          <Text
            style={[
              styles.sectionTitle,
              styles.sectionSpaced,
              { color: theme.text },
            ]}
          >
            Settings
          </Text>

          <View
            style={[
              styles.settingRow,
              { backgroundColor: theme.card, borderColor: theme.border },
            ]}
          >
            <Ionicons
              name={mode === "dark" ? "moon" : "sunny"}
              size={22}
              color={theme.text}
            />
            <Text style={[styles.settingText, { color: theme.text }]}>
              Dark theme
            </Text>
            <Switch
              value={mode === "dark"}
              onValueChange={(value) => setMode(value ? "dark" : "light")}
            />
          </View>

          <View style={styles.logoutWrapper}>
            <Button
              title="Logout"
              onPress={() => setLogoutModal(true)}
              loading={logout.isPending || isLoggingOut}
              disabled={isLoggingOut}
              style={{
                ...styles.logoutButton,
                borderColor: theme.danger,
              }}
              textStyle={{
                ...styles.logoutText,
                color: theme.danger,
              }}
            />
          </View>
        </ScrollView>

        <Modal
          visible={pwdModal}
          onClose={() => setPwdModal(false)}
          title="Change password"
          variant="warning"
        >
          <Input
            label="Current password"
            value={currentPwd}
            onChangeText={setCurrentPwd}
            secureTextEntry
            labelColor={theme.warning}
            borderColor={theme.warning}
            backgroundColor={theme.warning}
            placeholderColor={PLACEHOLDER_COLOR}
          />
          <Input
            label="New password"
            value={newPwd}
            onChangeText={setNewPwd}
            secureTextEntry
            labelColor={theme.warning}
            borderColor={theme.warning}
            backgroundColor={theme.warning}
            placeholderColor={PLACEHOLDER_COLOR}
          />
          <Button
            title="Change"
            onPress={handleChangePwd}
            loading={changePassword.isPending}
            style={styles.changePwdButton}
          />
        </Modal>

        <Modal
          visible={!!successMessage}
          onClose={() => setSuccessMessage(null)}
          title="Success"
          variant="success"
        >
          <Text style={[styles.modalMessage, { color: theme.card }]}>
            {successMessage}
          </Text>
          <Button title="OK" onPress={() => setSuccessMessage(null)} />
        </Modal>

        <Modal
          visible={!!errorMessage}
          onClose={() => setErrorMessage(null)}
          title="Error"
          variant="danger"
        >
          <Text style={[styles.modalMessage, { color: theme.card }]}>
            {errorMessage}
          </Text>
          <Button title="OK" onPress={() => setErrorMessage(null)} />
        </Modal>

        <Modal
          visible={logoutModal}
          onClose={() => setLogoutModal(false)}
          title="Logout"
          variant="danger"
        >
          <Text style={[styles.modalMessage, { color: theme.card }]}>
            Ready to log out?
          </Text>
          <View style={styles.modalActions}>
            <Button
              title="Cancel"
              onPress={() => setLogoutModal(false)}
              style={{ borderColor: theme.border, borderWidth: 2 }}
              textStyle={{ color: theme.text }}
            />
            <Button
              title="Logout"
              onPress={confirmLogout}
              loading={logout.isPending || isLoggingOut}
              style={{ borderColor: theme.danger, borderWidth: 2 }}
              textStyle={{ color: theme.danger }}
            />
          </View>
        </Modal>
      </SafeAreaView>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  background: { flex: 1, width: "100%", height: "100%" },
  overlay: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
  },
  container: { flex: 1 },
  scroll: { padding: 16, paddingBottom: 50 },
  header: { marginBottom: 50 },
  avatar: {
    width: AVATAR_SIZE,
    height: AVATAR_SIZE,
    borderRadius: AVATAR_SIZE / 2,
    borderWidth: 4,
    borderColor: MODAL_BACKGROUND,
    alignSelf: "center",
    backgroundColor: AVATAR_FALLBACK_BG,
  },
  avatarIcon: { alignSelf: "center" },
  name: { fontSize: 22, fontWeight: "800", marginTop: 12 },
  email: { fontSize: 16, fontWeight: "800" },
  roleBadge: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 13,
    marginTop: 8,
    alignSelf: "flex-start",
  },
  roleText: { fontSize: 18, fontWeight: "700" },
  sectionTitle: { fontSize: 18, fontWeight: "700", marginBottom: 12 },
  sectionSpaced: { marginTop: 60 },
  saveWrapper: { alignItems: "flex-start", marginTop: 8 },
  saveButton: {
    borderWidth: 2,
    paddingHorizontal: 23,
    paddingVertical: 12,
    alignSelf: "flex-start",
  },
  saveText: { fontSize: 16, fontWeight: "900" },
  changePwdButton: { borderRadius: 16, paddingHorizontal: 25 },
  logoutWrapper: { marginTop: 24, marginBottom: 34 },
  logoutButton: {
    borderWidth: 2,
    paddingHorizontal: 23,
    alignSelf: "flex-start",
  },
  logoutText: { fontSize: 14, fontWeight: "700" },
  settingRow: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 16,
    paddingVertical: 8, 
    borderRadius: 16,
    borderWidth: 1,
    gap: 12,
  },
  settingText: { flex: 1, fontSize: 16 },
  modalMessage: { fontSize: 16, marginBottom: 20, lineHeight: 22 },
  modalActions: {
    flexDirection: "row",
    gap: 12,
    justifyContent: "flex-end",
  },
});
