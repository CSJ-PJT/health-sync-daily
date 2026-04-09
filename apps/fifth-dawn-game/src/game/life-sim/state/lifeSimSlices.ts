import type {
  DeepStakeState,
  LifeSimPlayerState,
  LifeSimQuestJournalState,
  LifeSimRelationshipJournalState,
  LifeSimState,
  LifeSimWorldState,
} from "@/game/life-sim/types";
import type { SettlementState } from "@/game/settlement/settlementTypes";

export function getLifeSimPlayerSlice(state: LifeSimState): LifeSimPlayerState {
  return state.player;
}

export function getLifeSimWorldSlice(state: LifeSimState): LifeSimWorldState {
  return {
    plots: state.plots,
    resourceNodes: state.resourceNodes,
    hazards: state.hazards,
    storyFlags: state.storyFlags,
  };
}

export function getLifeSimQuestSlice(state: LifeSimState): LifeSimQuestJournalState {
  return {
    quests: state.quests,
    progression: state.progression,
  };
}

export function getLifeSimRelationshipSlice(state: LifeSimState): LifeSimRelationshipJournalState {
  return {
    relationships: state.relationships,
  };
}

export function getLifeSimSettlementSlice(state: LifeSimState): SettlementState {
  return state.settlement;
}

export function getLifeSimDeepStakeSlice(state: LifeSimState): DeepStakeState {
  return state.deepStake;
}
