// import { Ionicons } from "@expo/vector-icons";

// export interface SymbolDef {
//   code: string;
//   emoji: string;
//   color: string;
//   icon: keyof typeof Ionicons.glyphMap;
//   label: string;
// }

// export const SLOT_SYMBOLS: Record<string, SymbolDef> = {
//   cherry: {
//     code: "cherry",
//     emoji: "🍒",
//     color: "#E63946",
//     icon: "heart",
//     label: "Cherry",
//   },
//   lemon: {
//     code: "lemon",
//     emoji: "🍋",
//     color: "#F4D03F",
//     icon: "ellipse",
//     label: "Lemon",
//   },
//   bell: {
//     code: "bell",
//     emoji: "🔔",
//     color: "#F39C12",
//     icon: "notifications",
//     label: "Bell",
//   },
//   bar: {
//     code: "bar",
//     emoji: "🎰",
//     color: "#8E44AD",
//     icon: "remove",
//     label: "BAR",
//   },
//   seven: {
//     code: "seven",
//     emoji: "7️⃣",
//     color: "#27AE60",
//     icon: "star",
//     label: "Seven",
//   },
//   diamond: {
//     code: "diamond",
//     emoji: "💎",
//     color: "#3498DB",
//     icon: "diamond",
//     label: "Diamond",
//   },
//   clover: {
//     code: "clover",
//     emoji: "🍀",
//     color: "#2ECC71",
//     icon: "leaf",
//     label: "Clover",
//   },
//   star: {
//     code: "star",
//     emoji: "⭐",
//     color: "#F1C40F",
//     icon: "sparkles",
//     label: "Star",
//   },
// };

// export const ALL_SYMBOL_CODES = Object.keys(SLOT_SYMBOLS);

// export const getSymbol = (code: string | undefined): SymbolDef => {
//   return SLOT_SYMBOLS[code ?? ""] ?? {
//     code: "?",
//     emoji: "❓",
//     color: "#888",
//     icon: "help",
//     label: "?",
//   };
// };

import { Ionicons } from "@expo/vector-icons";

export interface SymbolDef {
  code: string;
  emoji: string;
  color: string;
  icon: keyof typeof Ionicons.glyphMap;
  label: string;
}

/**
 * All slot symbols available on the reels, keyed by symbol code.
 */
export const SLOT_SYMBOLS: Record<string, SymbolDef> = {
  cherry: {
    code: "cherry",
    emoji: "🍒",
    color: "#E63946",
    icon: "heart",
    label: "Cherry",
  },
  lemon: {
    code: "lemon",
    emoji: "🍋",
    color: "#F4D03F",
    icon: "ellipse",
    label: "Lemon",
  },
  bell: {
    code: "bell",
    emoji: "🔔",
    color: "#F39C12",
    icon: "notifications",
    label: "Bell",
  },
  bar: {
    code: "bar",
    emoji: "🎰",
    color: "#8E44AD",
    icon: "remove",
    label: "BAR",
  },
  seven: {
    code: "seven",
    emoji: "7️⃣",
    color: "#27AE60",
    icon: "star",
    label: "Seven",
  },
  diamond: {
    code: "diamond",
    emoji: "💎",
    color: "#3498DB",
    icon: "diamond",
    label: "Diamond",
  },
  clover: {
    code: "clover",
    emoji: "🍀",
    color: "#2ECC71",
    icon: "leaf",
    label: "Clover",
  },
  star: {
    code: "star",
    emoji: "⭐",
    color: "#F1C40F",
    icon: "sparkles",
    label: "Star",
  },
};

/**
 * All symbol codes, used when picking a random symbol during the spin.
 */
export const ALL_SYMBOL_CODES = Object.keys(SLOT_SYMBOLS);

const UNKNOWN_SYMBOL: SymbolDef = {
  code: "?",
  emoji: "❓",
  color: "#888",
  icon: "help",
  label: "?",
};

/**
 * Looks up a slot symbol by code, falling back to a placeholder
 * if the code is missing or unknown.
 *
 * @param code - The symbol code, or undefined.
 * @returns The resolved symbol definition.
 */
export const getSymbol = (code: string | undefined): SymbolDef => {
  return SLOT_SYMBOLS[code ?? ""] ?? UNKNOWN_SYMBOL;
};
