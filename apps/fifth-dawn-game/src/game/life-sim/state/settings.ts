import type { LifeSimInputAction, LifeSimSettings } from "@/game/life-sim/types";

export type LifeSimInputPreset = "wasd" | "arrows";

export const lifeSimInputPresets: Record<LifeSimInputPreset, Record<LifeSimInputAction, string[]>> = {
  wasd: {
    "move-up": ["w", "arrowup"],
    "move-down": ["s", "arrowdown"],
    "move-left": ["a", "arrowleft"],
    "move-right": ["d", "arrowright"],
    interact: ["e", "enter"],
    "use-tool": [" ", "space"],
    sleep: ["q"],
    "hotbar-1": ["1"],
    "hotbar-2": ["2"],
    "hotbar-3": ["3"],
    "hotbar-4": ["4"],
    "hotbar-5": ["5"],
  },
  arrows: {
    "move-up": ["arrowup", "w"],
    "move-down": ["arrowdown", "s"],
    "move-left": ["arrowleft", "a"],
    "move-right": ["arrowright", "d"],
    interact: ["enter", "e"],
    "use-tool": [" ", "space"],
    sleep: ["q"],
    "hotbar-1": ["1"],
    "hotbar-2": ["2"],
    "hotbar-3": ["3"],
    "hotbar-4": ["4"],
    "hotbar-5": ["5"],
  },
};

export function createDefaultLifeSimSettings(): LifeSimSettings {
  return {
    resolutionScale: 1,
    fullscreenPreferred: true,
    audioVolume: 0.75,
    inputMode: "keyboard-mouse",
    saveMode: "auto",
    showPerformanceOverlay: false,
    keyBindings: lifeSimInputPresets.wasd,
  };
}

export function applyInputPreset(settings: LifeSimSettings, preset: LifeSimInputPreset): LifeSimSettings {
  return {
    ...settings,
    keyBindings: lifeSimInputPresets[preset],
  };
}

export function rebindInputAction(
  settings: LifeSimSettings,
  action: LifeSimInputAction,
  key: string,
): LifeSimSettings {
  const normalized = key.toLowerCase();
  const nextBindings = Object.fromEntries(
    Object.entries(settings.keyBindings).map(([bindingAction, keys]) => [
      bindingAction,
      bindingAction === action ? [normalized, ...keys.filter((entry) => entry !== normalized)].slice(0, 2) : keys.filter((entry) => entry !== normalized),
    ]),
  ) as LifeSimSettings["keyBindings"];

  if (nextBindings[action].length === 0) {
    nextBindings[action] = [normalized];
  }

  return {
    ...settings,
    keyBindings: nextBindings,
  };
}
