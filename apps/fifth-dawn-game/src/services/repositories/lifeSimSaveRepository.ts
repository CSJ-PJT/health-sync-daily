import type { GameLinkProfile } from "@health-sync/shared-types";
import { createInitialLifeSimState } from "@/game/life-sim/state/lifeSimState";
import type { LifeSimHealthBonuses, LifeSimState } from "@/game/life-sim/types";
import { createDefaultSettlement } from "@/game/settlement/settlementState";
import { loadDerivedGameLinkBundle } from "@/services/repositories/gameLinkRepository";
import {
  createCloudLifeSimSaveAdapter,
  createLocalLifeSimSaveAdapter,
  resolveLifeSimSaveAdapters,
} from "@/services/repositories/lifeSimPersistence";

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

export async function loadLifeSimBonuses() {
  try {
    const bundle = await Promise.race([
      loadDerivedGameLinkBundle(),
      new Promise<null>((resolve) => window.setTimeout(() => resolve(null), 1200)),
    ]);
    return toHealthBonuses(bundle?.profile);
  } catch {
    return toHealthBonuses(null);
  }
}

export async function loadLifeSimState(slot = "main"): Promise<LifeSimState> {
  const bonuses = await loadLifeSimBonuses();
  const fallback = createInitialLifeSimState(bonuses);
  const localAdapter = createLocalLifeSimSaveAdapter();
  const cloudAdapter = createCloudLifeSimSaveAdapter();

  const hydrateState = (loaded: LifeSimState) => ({
    ...fallback,
    ...loaded,
    slot,
    healthBonuses: bonuses,
    settlement: loaded.settlement || createDefaultSettlement("새벽 거주지"),
    relationships: {
      ...fallback.relationships,
      ...loaded.relationships,
    },
  });

  const localLoaded = await localAdapter.load(slot);
  if (localLoaded) {
    return hydrateState(localLoaded);
  }

  if (cloudAdapter.isAvailable()) {
    try {
      const cloudLoaded = await Promise.race([
        cloudAdapter.load(slot),
        new Promise<null>((resolve) => window.setTimeout(() => resolve(null), 1500)),
      ]);

      if (cloudLoaded) {
        const hydrated = hydrateState(cloudLoaded);
        await localAdapter.save(hydrated, slot);
        return hydrated;
      }
    } catch {
      return fallback;
    }
  }

  return fallback;
}

export async function saveLifeSimState(state: LifeSimState, slot = "main") {
  const next = {
    ...state,
    slot,
  };

  for (const adapter of resolveLifeSimSaveAdapters(next)) {
    await adapter.save(next, slot);
  }
}
