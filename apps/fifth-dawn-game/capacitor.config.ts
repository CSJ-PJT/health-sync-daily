const config = {
  appId: process.env.VITE_FIFTH_DAWN_APP_ID || "com.example.fifthdawn",
  appName: process.env.VITE_FIFTH_DAWN_APP_NAME || "Deep Stake",
  webDir: "dist",
  bundledWebRuntime: false,
  android: {
    backgroundColor: "#07111f",
    allowMixedContent: false,
  },
  plugins: {
    SplashScreen: {
      launchShowDuration: 0,
      launchAutoHide: true,
      backgroundColor: process.env.VITE_FIFTH_DAWN_STATUS_BAR_COLOR || "#07111f",
      showSpinner: false,
    },
  },
};

export default config;
