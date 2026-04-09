import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./index.css";

const bootFallback = document.getElementById("boot-fallback");
if (bootFallback) {
  bootFallback.remove();
}

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);
