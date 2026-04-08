import { createDefaultLifeSimSettings } from "@/game/life-sim/state/settings";
import type { LifeSimSettings } from "@/game/life-sim/types";

const SETTINGS_KEY = "fifth_dawn_life_sim_settings_v1";

export function loadLifeSimSettings(): LifeSimSettings {
  const fallback = createDefaultLifeSimSettings();
  const raw = localStorage.getItem(SETTINGS_KEY);
  if (!raw) return fallback;

  try {
    const parsed = JSON.parse(raw) as Partial<LifeSimSettings>;
    return {
      ...fallback,
      ...parsed,
      keyBindings: {
        ...fallback.keyBindings,
        ...(parsed.keyBindings || {}),
      },
    };
  } catch {
    return fallback;
  }
}

export function saveLifeSimSettings(settings: LifeSimSettings) {
  localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings));
}
