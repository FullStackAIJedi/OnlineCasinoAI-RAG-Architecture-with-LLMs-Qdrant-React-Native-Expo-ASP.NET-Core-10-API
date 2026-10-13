/** @type {import('jest').Config} */
module.exports = {
  preset: "jest-expo",

  setupFilesAfterEnv: ["<rootDir>/jest.setup.js"],

  testMatch: ["<rootDir>/tests/**/*.test.ts", "<rootDir>/tests/**/*.test.tsx"],

  maxWorkers: 1,

  transformIgnorePatterns: [
    "node_modules/(?!((jest-)?react-native|@react-native(-community)?)|expo(nent)?|@expo(nent)?/.*|@expo-google-fonts/.*|react-navigation|@react-navigation/.*|@unimodules/.*|unimodules|sentry-expo|native-base|react-native-svg|@tanstack/.*|msw/.*)",
  ],

  moduleNameMapper: {
    "^@/tests/(.*)$": "<rootDir>/tests/$1",
    "^@/(.*)$": "<rootDir>/src/$1",
    "\\.(jpg|jpeg|png|gif|svg|webp)$": "<rootDir>/tests/__mocks__/fileMock.js",
    "^react-native/Libraries/Animated/NativeAnimatedHelper$":
      "<rootDir>/tests/__mocks__/NativeAnimatedHelper.js",
  },

  collectCoverageFrom: [
    "src/**/*.{ts,tsx}",
    "!src/**/*.d.ts",
    "!src/**/index.ts",
    "!src/api/types.ts",
    "!src/utils/constants.ts",
    "!src/constants/**",
    "!src/app/**",
  ],

  coverageThreshold: {
    global: {
      branches: 40,
      functions: 40,
      lines: 40,
      statements: 40,
    },
  },

  clearMocks: true,
};
