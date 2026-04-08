import type { LifeSimInputAction, LifeSimSettings } from "@/game/life-sim/types";

export function resolveInputAction(key: string, settings: LifeSimSettings): LifeSimInputAction | null {
  const normalized = key.toLowerCase();
  const match = Object.entries(settings.keyBindings).find(([, keys]) => keys.includes(normalized));
  return (match?.[0] as LifeSimInputAction | undefined) || null;
}
