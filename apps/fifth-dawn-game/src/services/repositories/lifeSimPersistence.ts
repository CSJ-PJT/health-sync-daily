import type { LifeSimState } from "@/game/life-sim/types";
import { supabase } from "@/integrations/supabase/client";
import { getStoredGameAccountId, getStoredGameLinkToken } from "@/services/repositories/gameLinkRepository";

const SAVE_KEY = "fifth_dawn_life_sim_save_v1";

export type SaveEnvelope = {
  slot: string;
  savedAt: string;
  state: LifeSimState;
};

export type LifeSimSaveAdapter = {
  id: "local" | "cloud";
  isAvailable(): boolean;
  load(slot: string): Promise<LifeSimState | null>;
  save(state: LifeSimState, slot: string): Promise<boolean>;
};

export function buildLifeSimStorageKey(slot: string) {
  return `${SAVE_KEY}:${slot}`;
}

export function createLocalLifeSimSaveAdapter(): LifeSimSaveAdapter {
  return {
    id: "local",
    isAvailable() {
      return true;
    },
    async load(slot) {
      const localRaw = localStorage.getItem(buildLifeSimStorageKey(slot));
      if (!localRaw) return null;
      try {
        const parsed = JSON.parse(localRaw) as SaveEnvelope;
        return parsed.state;
      } catch {
        localStorage.removeItem(buildLifeSimStorageKey(slot));
        return null;
      }
    },
    async save(state, slot) {
      localStorage.setItem(
        buildLifeSimStorageKey(slot),
        JSON.stringify({
          slot,
          savedAt: new Date().toISOString(),
          state,
        } satisfies SaveEnvelope),
      );
      return true;
    },
  };
}

export function createCloudLifeSimSaveAdapter(): LifeSimSaveAdapter {
  return {
    id: "cloud",
    isAvailable() {
      return Boolean(getStoredGameLinkToken() && getStoredGameAccountId());
    },
    async load(slot) {
      const token = getStoredGameLinkToken();
      const gameAccountId = getStoredGameAccountId();
      if (!token || !gameAccountId) return null;

      const { data, error } = await supabase.rpc("fetch_life_sim_state", {
        supplied_link_token: token,
        supplied_game_account_id: gameAccountId,
        requested_slot: slot,
      });
      if (error || !data) return null;
      return data as LifeSimState;
    },
    async save(state, slot) {
      const token = getStoredGameLinkToken();
      const gameAccountId = getStoredGameAccountId();
      if (!token || !gameAccountId) return false;

      const { error } = await supabase.rpc("upsert_life_sim_state", {
        supplied_link_token: token,
        supplied_game_account_id: gameAccountId,
        requested_slot: slot,
        requested_state: state,
      });
      return !error;
    },
  };
}

export function resolveLifeSimLoadAdapters() {
  const local = createLocalLifeSimSaveAdapter();
  const cloud = createCloudLifeSimSaveAdapter();
  return [local, cloud].filter((adapter) => adapter.isAvailable());
}

export function resolveLifeSimSaveAdapters(state: LifeSimState) {
  const local = createLocalLifeSimSaveAdapter();
  const cloud = createCloudLifeSimSaveAdapter();

  if (state.settings.saveMode === "local") return [local];
  if (state.settings.saveMode === "cloud") return cloud.isAvailable() ? [cloud] : [local];
  return cloud.isAvailable() ? [local, cloud] : [local];
}
