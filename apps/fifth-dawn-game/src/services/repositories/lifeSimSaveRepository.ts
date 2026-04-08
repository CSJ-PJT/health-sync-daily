import type { GameLinkProfile } from "@health-sync/shared-types";
import { createInitialLifeSimState } from "@/game/life-sim/state/lifeSimState";
import type { LifeSimHealthBonuses, LifeSimState } from "@/game/life-sim/types";
import { supabase } from "@/integrations/supabase/client";
import { getStoredGameAccountId, getStoredGameLinkToken, loadDerivedGameLinkBundle } from "@/services/repositories/gameLinkRepository";

const SAVE_KEY = "fifth_dawn_life_sim_save_v1";

function normalizeBonuses(input?: Partial<LifeSimHealthBonuses>): LifeSimHealthBonuses {
  return {
    startEnergyBonus: Math.max(0, Math.min(6, Math.round(input?.startEnergyBonus || 0))),
    recoveryBonus: Math.max(0, Math.min(5, Math.round(input?.recoveryBonus || 0))),
    cropEfficiencyBonus: Math.max(0, Math.min(2, Math.round(input?.cropEfficiencyBonus || 0))),
  };
}

function toHealthBonuses(profile?: GameLinkProfile | null): LifeSimHealthBonuses {
  if (!profile) {
    return normalizeBonuses();
  }

  const activityBoost = profile.activityTier >= 4 ? 4 : profile.activityTier >= 3 ? 2 : 0;
  const recoveryBoost = profile.sleepTier >= 4 || profile.recoveryTier >= 4 ? 2 : profile.sleepTier >= 3 ? 1 : 0;
  const cropBoost = profile.consistencyScore >= 80 || profile.hydrationTier >= 4 ? 1 : 0;

  return normalizeBonuses({
    startEnergyBonus: activityBoost,
    recoveryBonus: recoveryBoost,
    cropEfficiencyBonus: cropBoost,
  });
}

function buildStorageKey(slot: string) {
  return `${SAVE_KEY}:${slot}`;
}

type SaveEnvelope = {
  slot: string;
  savedAt: string;
  state: LifeSimState;
};

async function loadServerState(slot: string) {
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
}

async function saveServerState(state: LifeSimState, slot: string) {
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
}

export async function loadLifeSimBonuses() {
  const bundle = await loadDerivedGameLinkBundle();
  return toHealthBonuses(bundle?.profile);
}

export async function loadLifeSimState(slot = "main"): Promise<LifeSimState> {
  const bonuses = await loadLifeSimBonuses();
  const localRaw = localStorage.getItem(buildStorageKey(slot));
  if (localRaw) {
    try {
      const parsed = JSON.parse(localRaw) as SaveEnvelope;
      return {
        ...parsed.state,
        slot,
        healthBonuses: bonuses,
      };
    } catch {
      localStorage.removeItem(buildStorageKey(slot));
    }
  }

  const serverState = await loadServerState(slot);
  if (serverState) {
    const hydrated = {
      ...serverState,
      slot,
      healthBonuses: bonuses,
    };
    localStorage.setItem(
      buildStorageKey(slot),
      JSON.stringify({
        slot,
        savedAt: new Date().toISOString(),
        state: hydrated,
      } satisfies SaveEnvelope),
    );
    return hydrated;
  }

  return createInitialLifeSimState(bonuses);
}

export async function saveLifeSimState(state: LifeSimState, slot = "main") {
  const next = {
    ...state,
    slot,
  };
  localStorage.setItem(
    buildStorageKey(slot),
    JSON.stringify({
      slot,
      savedAt: new Date().toISOString(),
      state: next,
    } satisfies SaveEnvelope),
  );
  await saveServerState(next, slot);
}
