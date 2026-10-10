export const lightColors = {
  background: "#F5F5F5",
  card: "#FFFFFF",
  text: "#080112",
  textSecondary: "#000001",
  textTertiary: "#10021b",
  primary: "#000000",
  border: "#333333",
  danger: "#e40a40",
  success: "#13ef17",
  warning: "#ff890a",
  overlay: "#13031f",
  clean: "#FFFFFF", 
  barIcon: "#979798",  
};

export const darkColors: typeof lightColors = {
  background: "#0c0115",  
  card: "#0c0115",  
  text: "#FFFFFF",
  textSecondary: "#AAAAAA",
  textTertiary: "#10021b",
  primary: "#221e2d",
  border: "#333333",
  danger: "#e40a40",  
  success: "#13ef17", 
  warning: "#ff890a",  
  overlay: "#ecebed",
  clean: "#FFFFFF", 
  barIcon: "#ffffff", 
};

export type Theme = typeof lightColors;
