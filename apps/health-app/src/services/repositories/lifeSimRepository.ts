import { createInitialLifeSimState } from "@/game/life-sim/state/lifeSimState";
import type { LifeSimHealthBonuses, LifeSimState } from "@/game/life-sim/types";
import { supabase } from "@/integrations/supabase/client";
import { readScopedJson, writeScopedJson } from "@/services/persistence/scopedStorage";

type LifeSimSaveEnvelope = {
  slot: string;
  savedAt: string;
  state: LifeSimState;
};

export interface LifeSimSaveAdapter {
  load(slot?: string): Promise<LifeSimState>;
  save(state: LifeSimState, slot?: string): Promise<boolean>;
  loadBonuses(): Promise<LifeSimHealthBonuses>;
}

const DEFAULT_SLOT = "main";
const SAVE_KEY = "life_sim_save_v1";

function getProfileId() {
  return localStorage.getItem("profile_id");
}

function getUserId() {
  return localStorage.getItem("user_id") || "me";
}

function buildScopedKey(slot: string) {
  return `${SAVE_KEY}:${slot}`;
}

function average(values: number[]) {
  if (values.length === 0) return 0;
  return values.reduce((sum, value) => sum + value, 0) / values.length;
}

function normalizeBonuses(input?: Partial<LifeSimHealthBonuses>): LifeSimHealthBonuses {
  return {
    startEnergyBonus: Math.max(0, Math.min(6, Math.round(input?.startEnergyBonus || 0))),
    recoveryBonus: Math.max(0, Math.min(5, Math.round(input?.recoveryBonus || 0))),
    cropEfficiencyBonus: Math.max(0, Math.min(1, Math.round(input?.cropEfficiencyBonus || 0))),
  };
}

export async function loadLifeSimHealthBonuses(): Promise<LifeSimHealthBonuses> {
  const userId = getUserId();
  const fallback = normalizeBonuses();

  const { data, error } = await supabase
    .from("health_data")
    .select("steps_data, sleep_data, running_data, synced_at")
    .eq("user_id", userId)
    .order("synced_at", { ascending: false })
    .limit(14);

  if (error || !data || data.length === 0) {
    return fallback;
  }

  const rows = (data as Array<Record<string, unknown>>) || [];
  const stepAverage = average(
    rows
      .map((row) => Number((row.steps_data as { count?: number } | null)?.count || 0))
      .filter((value) => value > 0),
  );
  const sleepAverage = average(
    rows
      .map((row) => Number((row.sleep_data as { totalMinutes?: number } | null)?.totalMinutes || 0))
      .filter((value) => value > 0),
  );
  const weeklyRuns = rows.filter((row) => {
    const summary = (row.running_data as { summary?: { distanceKm?: number } } | null)?.summary;
    return Number(summary?.distanceKm || 0) > 0;
  }).length;

  return normalizeBonuses({
    startEnergyBonus: stepAverage >= 10000 ? 4 : stepAverage >= 8000 ? 2 : 0,
    recoveryBonus: sleepAverage >= 450 ? 2 : sleepAverage >= 390 ? 1 : 0,
    cropEfficiencyBonus: weeklyRuns >= 3 ? 1 : 0,
  });
}

async function loadServerSave(slot: string) {
  const profileId = getProfileId();
  if (!profileId) return null;

  const { data, error } = await supabase
    .from("entertainment_life_sim_saves")
    .select("*")
    .eq("profile_id", profileId)
    .eq("slot", slot)
    .maybeSingle();

  if (error || !data) return null;

  const row = data as Record<string, unknown>;
  return row.state as LifeSimState;
}

async function saveServerSave(state: LifeSimState, slot: string) {
  const profileId = getProfileId();
  if (!profileId) return false;

  const { error } = await supabase.from("entertainment_life_sim_saves").upsert({
    id: `${profileId}:${slot}`,
    profile_id: profileId,
    user_id: getUserId(),
    slot,
    state,
    updated_at: new Date().toISOString(),
  });

  return !error;
}

export async function loadLifeSimState(slot = DEFAULT_SLOT) {
  const bonuses = await loadLifeSimHealthBonuses();
  const local = readScopedJson<LifeSimSaveEnvelope | null>(buildScopedKey(slot), null);

  if (local?.state) {
    return {
      ...local.state,
      slot,
      healthBonuses: local.state.healthBonuses || bonuses,
    };
  }

  const server = await loadServerSave(slot);
  if (server) {
    const hydrated = {
      ...server,
      slot,
      healthBonuses: server.healthBonuses || bonuses,
    };
    writeScopedJson(buildScopedKey(slot), {
      slot,
      savedAt: new Date().toISOString(),
      state: hydrated,
    });
    return hydrated;
  }

  return createInitialLifeSimState(bonuses);
}

export async function saveLifeSimState(state: LifeSimState, slot = DEFAULT_SLOT) {
  const envelope: LifeSimSaveEnvelope = {
    slot,
    savedAt: new Date().toISOString(),
    state: {
      ...state,
      slot,
    },
  };

  writeScopedJson(buildScopedKey(slot), envelope);
  const serverSaved = await saveServerSave(envelope.state, slot);
  return serverSaved;
}

export const lifeSimRepository: LifeSimSaveAdapter = {
  load: loadLifeSimState,
  save: saveLifeSimState,
  loadBonuses: loadLifeSimHealthBonuses,
};
