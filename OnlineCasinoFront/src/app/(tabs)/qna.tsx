import { useRef, useState } from "react";
import {
  ActivityIndicator,
  FlatList,
  ImageBackground,
  KeyboardAvoidingView,
  Platform,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Ionicons } from "@expo/vector-icons";

import { useTheme } from "../../../src/theme/ThemeProvider";
import { useAskQuestion } from "../../../src/hooks/useRag";

interface ChatMessage {
  id: string;
  role: "user" | "assistant";
  text: string;
  sources?: string[];
  isError?: boolean;
}

const SUGGESTIONS = [
  "What is the minimum deposit?",
  "How does the welcome bonus work?",
  "How long do withdrawals take?",
  "How do I set a deposit limit?",
];

/**
 * Q&A screen — chat interface for Casino.RagService.
 *
 * Uses `useAskQuestion` → `POST /api/chat/policy-question`.
 * Displays the answer and the sources (policy files) returned by the server.
 *
 * Features:
 * - "Clear chat" button (top-right) appears when there are messages.
 * - Submit button inside the input field (bottom-right).
 * - Suggestions shown when the conversation is empty.
 */
export default function QnaScreen() {
  const theme = useTheme();
  const listRef = useRef<FlatList<ChatMessage>>(null);

  const [question, setQuestion] = useState("");
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [search, setSearch] = useState("");

  const ask = useAskQuestion();

  const scrollToEnd = () =>
    requestAnimationFrame(() =>
      listRef.current?.scrollToEnd({ animated: true }),
    );

  const sendQuestion = (raw: string) => {
    const trimmed = raw.trim();
    if (!trimmed || ask.isPending) return;

    setMessages((prev) => [
      ...prev,
      { id: `u-${Date.now()}`, role: "user", text: trimmed },
    ]);
    setQuestion("");
    scrollToEnd();

    ask.mutate(trimmed, {
      onSuccess: (data) => {
        setMessages((prev) => [
          ...prev,
          {
            id: `a-${Date.now()}`,
            role: "assistant",
            text: data.answer,
            sources: data.sources,
          },
        ]);
        scrollToEnd();
      },
      onError: (err) => {
        setMessages((prev) => [
          ...prev,
          {
            id: `e-${Date.now()}`,
            role: "assistant",
            text: err.message || "Something went wrong. Please try again.",
            isError: true,
          },
        ]);
        scrollToEnd();
      },
    });
  };

  const handleSend = () => sendQuestion(question);

  const handleNewChat = () => {
    setMessages([]);
    setQuestion("");
    setSearch("");
  };

  // Filter messages by search (for display purposes only; does not send to RAG)
  const filtered = search.trim()
    ? messages.filter((m) =>
        m.text.toLowerCase().includes(search.toLowerCase()),
      )
    : messages;

  const showEmpty = messages.length === 0;

  return (
    <ImageBackground
      source={require("../../../assets/backgrounds/profile.jpg")}
      style={styles.background}
      resizeMode="cover"
    >
      <View style={styles.overlay} />

      <SafeAreaView style={styles.container} edges={["top"]}>
        {messages.length > 0 && (
          <View style={styles.clearBar}>
            <TouchableOpacity
              onPress={handleNewChat}
              style={[
                styles.clearBtn,
                {
                  backgroundColor: theme.danger,
                  borderColor: theme.border,
                },
              ]}
              accessibilityLabel="Clear conversation"
            >
              <Ionicons name="close" size={18} color={theme.text} />
              <Text style={[styles.clearText, { color: theme.text }]}>
                Clear
              </Text>
            </TouchableOpacity>
          </View>
        )}

        <KeyboardAvoidingView
          style={styles.flex}
          behavior={Platform.OS === "ios" ? "padding" : undefined}
          keyboardVerticalOffset={90}
        >
          <FlatList
            ref={listRef}
            data={filtered}
            keyExtractor={(m) => m.id}
            contentContainerStyle={styles.listContent}
            keyboardShouldPersistTaps="handled"
            renderItem={({ item }) => {
              const isUser = item.role === "user";
              return (
                <View
                  style={[
                    styles.bubble,
                    {
                      alignSelf: isUser ? "flex-end" : "flex-start",
                      backgroundColor: isUser
                        ? theme.primary
                        : item.isError
                          ? theme.danger
                          : theme.card,
                      borderColor: theme.border,
                    },
                  ]}
                >
                  <Text
                    style={[
                      styles.bubbleText,
                      {
                        color: isUser
                          ? "#fff"
                          : item.isError
                            ? "#fff"
                            : theme.text,
                      },
                    ]}
                  >
                    {item.text}
                  </Text>

                  {item.sources && item.sources.length > 0 && (
                    <View style={styles.sourcesRow}>
                      <Ionicons
                        name="document-text-outline"
                        size={12}
                        color={theme.textSecondary}
                      />
                      <Text
                        style={[styles.sources, { color: theme.textSecondary }]}
                      >
                        {item.sources.join(", ")}
                      </Text>
                    </View>
                  )}
                </View>
              );
            }}
            ListEmptyComponent={
              showEmpty ? (
                <View style={styles.emptyBox}>
                  <Ionicons
                    name="chatbubbles-outline"
                    size={48}
                    color={theme.textSecondary}
                  />
                  <Text style={[styles.emptyTitle, { color: theme.text }]}>
                    Ask about casino policies
                  </Text>
                  <Text
                    style={[
                      styles.emptySubtitle,
                      { color: theme.textSecondary },
                    ]}
                  >
                    Bonuses, deposits, withdrawals, responsible gaming — I'm
                    here to help.
                  </Text>

                  <View style={styles.suggestions}>
                    {SUGGESTIONS.map((s) => (
                      <Text
                        key={s}
                        onPress={() => sendQuestion(s)}
                        style={[
                          styles.suggestion,
                          {
                            color: theme.text,
                            backgroundColor: theme.card,
                            borderColor: theme.border,
                          },
                        ]}
                      >
                        {s}
                      </Text>
                    ))}
                  </View>
                </View>
              ) : (
                <Text
                  style={[styles.emptySearch, { color: theme.textSecondary }]}
                >
                  No messages match "{search}"
                </Text>
              )
            }
            ListFooterComponent={
              ask.isPending ? (
                <View style={styles.typingRow}>
                  <ActivityIndicator color={theme.warning} size="small" />
                  <Text
                    style={[styles.typingText, { color: theme.textSecondary }]}
                  >
                    Thinking...
                  </Text>
                </View>
              ) : undefined
            }
            onContentSizeChange={scrollToEnd}
          />

          {/* Долна лента: input + submit бутон */}
          <View
            style={{
              flexDirection: "row",
              alignItems: "center",
              paddingHorizontal: 12,
              paddingTop: 10,
              paddingBottom: 80,
              backgroundColor: theme.card,
              borderTopWidth: 1,
              borderColor: theme.border,
            }}
          >
            <TextInput
              value={question}
              onChangeText={setQuestion}
              placeholder="Ask a question..."
              placeholderTextColor={theme.textSecondary}
              style={{
                flex: 1,
                minHeight: 48,
                maxHeight: 120,
                borderWidth: 1.5,
                borderRadius: 24,
                paddingHorizontal: 16,
                paddingVertical: 10,
                fontSize: 15,
                color: theme.text,
                backgroundColor: theme.background,
                borderColor: theme.border,
              }}
              multiline
              maxLength={500}
            />
            <TouchableOpacity
              onPress={handleSend}
              disabled={ask.isPending || !question.trim()}
              activeOpacity={0.7}
              style={{
                width: 48,
                height: 48,
                borderRadius: 24,
                backgroundColor:
                  ask.isPending || !question.trim()
                    ? theme.border
                    : theme.success,
                alignItems: "center",
                justifyContent: "center",
                marginLeft: 10,
              }}
              accessibilityLabel="Send question"
            >
              {ask.isPending ? (
                <ActivityIndicator size="small" color="#fff" />
              ) : (
                <Ionicons name="arrow-up" size={24} color="#fff" />
              )}
            </TouchableOpacity>
          </View>
        </KeyboardAvoidingView>
      </SafeAreaView>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  background: { flex: 1, width: "100%", height: "100%" },
  overlay: {
    ...StyleSheet.absoluteFill,
    backgroundColor: "rgba(12, 1, 21, 0.55)",
  },
  container: { flex: 1 },
  flex: { flex: 1 },
  clearBar: {
    flexDirection: "row",
    justifyContent: "flex-end",
    paddingHorizontal: 12,
    paddingTop: 8,
    paddingBottom: 4,
  },
  clearBtn: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 16,
    borderWidth: 1,
  },
  clearText: {
    fontSize: 13,
    fontWeight: "600",
  },
  listContent: { padding: 16, gap: 10, flexGrow: 1 },
  bubble: {
    maxWidth: "85%",
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 16,
    borderWidth: 1,
  },
  bubbleText: { fontSize: 15, lineHeight: 21 },
  sourcesRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
    marginTop: 8,
    paddingTop: 6,
    borderTopWidth: StyleSheet.hairlineWidth,
    borderTopColor: "rgba(255,255,255,0.15)",
  },
  sources: { fontSize: 11, fontStyle: "italic", flex: 1 },
  emptyBox: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    paddingHorizontal: 24,
  },
  emptyTitle: {
    fontSize: 20,
    fontWeight: "800",
    marginTop: 16,
    textAlign: "center",
  },
  emptySubtitle: {
    fontSize: 14,
    marginTop: 8,
    textAlign: "center",
    lineHeight: 20,
  },
  emptySearch: {
    textAlign: "center",
    marginTop: 24,
    fontSize: 14,
    fontStyle: "italic",
  },
  suggestions: { marginTop: 28, gap: 10, width: "100%" },
  suggestion: {
    fontSize: 14,
    fontWeight: "600",
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderRadius: 12,
    borderWidth: 1,
    overflow: "hidden",
  },
  typingRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
    paddingHorizontal: 4,
    paddingVertical: 8,
  },
  typingText: { fontSize: 13, fontStyle: "italic" },
});
