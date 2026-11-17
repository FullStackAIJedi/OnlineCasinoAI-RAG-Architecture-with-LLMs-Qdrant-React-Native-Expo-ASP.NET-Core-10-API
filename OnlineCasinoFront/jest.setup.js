process.env.EXPO_PUBLIC_API_URL = "http://localhost:5000";

const originalError = console.error;
console.error = (...args) => {
  const msg = typeof args[0] === "string" ? args[0] : "";
  if (
    msg.includes("not wrapped in act") ||
    msg.includes("You called act") ||
    msg.includes("overlapping act") ||
    msg.includes(
      "The current testing environment is not configured to support act",
    )
  ) {
    return;
  }
  originalError(...args);
};

const originalWarn = console.warn;
console.warn = (...args) => {
  const msg = typeof args[0] === "string" ? args[0] : "";
  if (msg.includes("Animated") || msg.includes("useNativeDriver")) {
    return;
  }
  originalWarn(...args);
};

jest.mock("expo-secure-store", () => ({
  getItemAsync: jest.fn(),
  setItemAsync: jest.fn(),
  deleteItemAsync: jest.fn(),
}));

jest.mock("expo-router", () => {
  const React = require("react");
  const { Text, View } = require("react-native");
  return {
    router: { push: jest.fn(), replace: jest.fn(), back: jest.fn() },
    useLocalSearchParams: jest.fn(() => ({})),
    useRouter: jest.fn(() => ({
      push: jest.fn(),
      replace: jest.fn(),
      back: jest.fn(),
    })),
    useFocusEffect: (cb) => {
      React.useEffect(cb, [cb]);
    },
    useNavigation: jest.fn(() => ({
      navigate: jest.fn(),
      goBack: jest.fn(),
    })),
    Link: (props) => React.createElement(Text, null, props.children),
    Stack: (props) => React.createElement(View, null, props.children),
    Tabs: (props) => React.createElement(View, null, props.children),
    Redirect: (props) => React.createElement(Text, null, String(props.href)),
  };
});

jest.mock("@expo/vector-icons", () => {
  const React = require("react");
  const { Text } = require("react-native");
  const makeIcon = (name) => (props) =>
    React.createElement(Text, props, `[${name}]`);
  return {
    Ionicons: makeIcon("Ionicons"),
    MaterialIcons: makeIcon("MaterialIcons"),
    FontAwesome: makeIcon("FontAwesome"),
    AntDesign: makeIcon("AntDesign"),
    Feather: makeIcon("Feather"),
  };
});

jest.mock("react-native-safe-area-context", () => {
  const React = require("react");
  const { View } = require("react-native");
  const insets = { top: 0, bottom: 0, left: 0, right: 0 };
  const frame = { x: 0, y: 0, width: 390, height: 844 };
  return {
    SafeAreaProvider: (props) =>
      React.createElement(View, null, props.children),
    SafeAreaView: (props) => React.createElement(View, props, props.children),
    useSafeAreaInsets: () => insets,
    useSafeAreaFrame: () => frame,
    initialWindowMetrics: { insets, frame },
  };
});

jest.mock("@react-native-community/datetimepicker", () => {
  const React = require("react");
  const { View, Text } = require("react-native");
  const DateTimePicker = (props) =>
    React.createElement(View, props, React.createElement(Text, null, "Picker"));
  return {
    __esModule: true,
    default: DateTimePicker,
    DateTimePickerAndroid: {
      open: jest.fn(),
    },
  };
});

afterAll(() => {
  jest.clearAllTimers();
  jest.useRealTimers();
});
