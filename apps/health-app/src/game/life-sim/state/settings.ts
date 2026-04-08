import type { LifeSimInputAction, LifeSimSettings } from "@/game/life-sim/types";

export function createDefaultLifeSimSettings(): LifeSimSettings {
  return {
    resolutionScale: 1,
    fullscreenPreferred: true,
    audioVolume: 0.75,
    keyBindings: {
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
    } satisfies Record<LifeSimInputAction, string[]>,
  };
}
