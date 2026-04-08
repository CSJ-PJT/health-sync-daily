const config = {
  appId: process.env.VITE_FIFTH_DAWN_APP_ID || "com.example.fifthdawn",
  appName: process.env.VITE_FIFTH_DAWN_APP_NAME || "Fifth Dawn",
  webDir: "dist",
  bundledWebRuntime: false,
  server: {
    androidScheme: process.env.VITE_FIFTH_DAWN_ANDROID_SCHEME || "https",
  },
  android: {
    backgroundColor: "#07111f",
    allowMixedContent: false,
  },
  plugins: {
    SplashScreen: {
      launchShowDuration: 1200,
      backgroundColor: process.env.VITE_FIFTH_DAWN_STATUS_BAR_COLOR || "#07111f",
      showSpinner: false,
    },
  },
};

export default config;
