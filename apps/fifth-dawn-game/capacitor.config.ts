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
};

export default config;
