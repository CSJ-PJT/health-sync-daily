import type { LocaleCode, LocalizedText } from "@/game/life-sim/types";

export function getLifeSimLocale(): LocaleCode {
  const stored = localStorage.getItem("app_language");
  if (stored === "ko" || stored === "en") {
    return stored;
  }

  if (typeof navigator !== "undefined" && navigator.language.toLowerCase().startsWith("ko")) {
    return "ko";
  }

  return "en";
}

export function t(text: LocalizedText, locale = getLifeSimLocale()) {
  return text[locale];
}
