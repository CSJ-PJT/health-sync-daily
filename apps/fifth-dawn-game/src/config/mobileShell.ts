export const fifthDawnMobileShell = {
  productKey: import.meta.env.VITE_FIFTH_DAWN_PRODUCT_KEY || "fifth-dawn",
  appName: import.meta.env.VITE_FIFTH_DAWN_APP_NAME || "Deep Stake",
  preferredOrientation: import.meta.env.VITE_FIFTH_DAWN_ORIENTATION || "landscape",
  statusBarColor: import.meta.env.VITE_FIFTH_DAWN_STATUS_BAR_COLOR || "#07111f",
} as const;
