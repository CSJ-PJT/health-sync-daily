import { lifeSimItems } from "@/game/life-sim/data/items";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import { lifeSimMaps } from "@/game/life-sim/data/maps";
import { lifeSimDialogue } from "@/game/life-sim/data/npcs";
import { lifeSimQuestDefinitions, initialLifeSimQuests } from "@/game/life-sim/data/quests";
import { lifeSimRecipes } from "@/game/life-sim/data/recipes";
import { applyQuestDeepStakeInfluence, createInitialDeepStakeState, getDeepStakeNpcEcho } from "@/game/life-sim/state/deepStake";
import { createDefaultSettlement, unlockSettlementStructure } from "@/game/settlement/settlementState";
import { createLifeSimEventSink } from "@/game/life-sim/state/events";
import { getNpcAtPosition } from "@/game/life-sim/state/npcSchedule";
import { createDefaultLifeSimSettings } from "@/game/life-sim/state/settings";
import type {
  LifeSimCropPlot,
  LifeSimDialogueLine,
  LifeSimFacing,
  LifeSimHealthBonuses,
  LifeSimItemId,
  LifeSimMapId,
  LifeSimNpcId,
  LifeSimProgressionState,
  LifeSimQuestId,
  LifeSimQuestState,
  LifeSimRecipeId,
  LifeSimRelationshipState,
  LifeSimState,
  LifeSimUiMessage,
  LocalizedText,
} from "@/game/life-sim/types";

type LifeSimActionResult = {
  state: LifeSimState;
  message?: LifeSimUiMessage;
};

function createMessage(title: string, body: string): LifeSimUiMessage {
  return {
    id: `life-sim-message-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
    title,
    body,
  };
}

function createLocalizedMessage(title: LocalizedText, body: LocalizedText): LifeSimUiMessage {
  const locale = getLifeSimLocale();
  return createMessage(t(title, locale), t(body, locale));
}

function appendMessage(state: LifeSimState, message?: LifeSimUiMessage) {
  if (!message) return state;
  return {
    ...state,
    messageLog: [message, ...state.messageLog].slice(0, 24),
  };
}

function getMapTile(mapId: LifeSimMapId, x: number, y: number) {
  return lifeSimMaps[mapId].tiles.find((tile) => tile.x === x && tile.y === y) || null;
}

function getFrontTile(state: LifeSimState) {
  const { x, y, facing, mapId } = state.player;
  const offsets: Record<LifeSimFacing, [number, number]> = {
    up: [0, -1],
    down: [0, 1],
    left: [-1, 0],
    right: [1, 0],
  };
  const [dx, dy] = offsets[facing];
  return getMapTile(mapId, x + dx, y + dy);
}

function withInventoryDelta(state: LifeSimState, itemId: LifeSimItemId, amount: number) {
  const nextValue = Math.max(0, (state.player.inventory[itemId] || 0) + amount);
  return {
    ...state.player,
    inventory: {
      ...state.player.inventory,
      [itemId]: nextValue,
    },
  };
}

function spendEnergy(state: LifeSimState, amount: number) {
  return {
    ...state,
    player: {
      ...state.player,
      energy: Math.max(0, state.player.energy - amount),
    },
  };
}

function getPlot(state: LifeSimState, x: number, y: number) {
  return state.plots.find((plot) => plot.x === x && plot.y === y) || null;
}

function upsertPlot(state: LifeSimState, patch: LifeSimCropPlot) {
  const index = state.plots.findIndex((plot) => plot.x === patch.x && plot.y === patch.y);
  if (index === -1) return [...state.plots, patch];
  return state.plots.map((plot, plotIndex) => (plotIndex === index ? patch : plot));
}

function getRelationshipLevel(friendship: number): 0 | 1 | 2 | 3 {
  if (friendship >= 9) return 3;
  if (friendship >= 6) return 2;
  if (friendship >= 3) return 1;
  return 0;
}

function updateRelationship(
  relationships: Record<LifeSimNpcId, LifeSimRelationshipState>,
  npcId: LifeSimNpcId,
  day: number,
  amount = 1,
) {
  const current = relationships[npcId];
  const friendship = current.friendship + amount;
  return {
    ...relationships,
    [npcId]: {
      friendship,
      lastTalkDay: day,
      level: getRelationshipLevel(friendship),
      rewardedLevels: current.rewardedLevels || [],
    },
  };
}

function updateProgression(
  progression: LifeSimProgressionState,
  patch: Partial<LifeSimProgressionState>,
): LifeSimProgressionState {
  return { ...progression, ...patch };
}

function applyRelationshipMilestoneRewards(state: LifeSimState, npcId: LifeSimNpcId) {
  const relation = state.relationships[npcId];
  const rewardedLevels = relation.rewardedLevels || [];
  let nextState = state;

  const markRewarded = (level: number) => {
    nextState = {
      ...nextState,
      relationships: {
        ...nextState.relationships,
        [npcId]: {
          ...nextState.relationships[npcId],
          rewardedLevels: Array.from(new Set([...(nextState.relationships[npcId].rewardedLevels || []), level])).sort(
            (a, b) => a - b,
          ),
        },
      },
    };
  };

  if (relation.level >= 1 && !rewardedLevels.includes(1)) {
    nextState = appendMessage(
      {
        ...nextState,
        player: withInventoryDelta(nextState, "turnip-seeds", 2),
      },
      createMessage("관계도 보상", "새로운 인연의 표시로 새벽 순무 씨앗 2개를 받았습니다."),
    );
    markRewarded(1);
  }

  if (relation.level >= 2 && !rewardedLevels.includes(2)) {
    nextState = appendMessage(
      {
        ...nextState,
        player:
          npcId === "mechanic"
            ? withInventoryDelta(nextState, "scrap-bundle", 1)
            : withInventoryDelta(nextState, "dawn-broth", 1),
        progression: updateProgression(nextState.progression, {
          resonancePoints: nextState.progression.resonancePoints + 3,
        }),
      },
      createMessage(
        "관계도 보상",
        npcId === "mechanic"
          ? "정비공이 고철 묶음 1개와 공명 3을 건네주었습니다."
          : "기록 관리인이 새벽 수프 1개와 공명 3을 건네주었습니다.",
      ),
    );
    markRewarded(2);
  }

  if (relation.level >= 3 && !rewardedLevels.includes(3)) {
    nextState = appendMessage(
      {
        ...nextState,
        progression: updateProgression(nextState.progression, {
          resonancePoints: nextState.progression.resonancePoints + 8,
        }),
      },
      createMessage("깊은 신뢰", "깊은 신뢰 단계에 도달했습니다. 공명 8을 얻었습니다."),
    );
    markRewarded(3);
  }

  return nextState;
}

function completeQuest(quests: LifeSimQuestState[], questId: LifeSimQuestId, day: number): LifeSimQuestState[] {
  return quests.map((quest) =>
    quest.id === questId && quest.status !== "completed" ? { ...quest, status: "completed", completedOnDay: day } : quest,
  );
}

function grantQuestReward(state: LifeSimState, questId: LifeSimQuestId) {
  switch (questId) {
    case "first-harvest":
      return appendMessage(
        {
          ...state,
          player: withInventoryDelta(state, "dawn-broth", 1),
          progression: updateProgression(state.progression, {
            resonancePoints: state.progression.resonancePoints + 5,
            completedQuestIds: [...state.progression.completedQuestIds, questId],
          }),
        },
        createMessage("퀘스트 완료", "첫 수확을 마쳤습니다. 새벽 수프 1개와 공명 5를 얻었습니다."),
      );
    case "mine-recon":
      return appendMessage(
        {
          ...state,
          progression: updateProgression(state.progression, {
            resonancePoints: state.progression.resonancePoints + 10,
            unlockedRecipes: Array.from(new Set([...state.progression.unlockedRecipes, "purity-lantern"])),
            completedQuestIds: [...state.progression.completedQuestIds, questId],
          }),
        },
        createMessage("퀘스트 완료", "광산 정찰을 마쳤습니다. 정화 등불 레시피와 공명 10을 얻었습니다."),
      );
    case "repair-lantern":
      return appendMessage(
        {
          ...state,
          relationships: updateRelationship(state.relationships, "mechanic", state.time.day, 2),
          settlement: unlockSettlementStructure(state.settlement, "purity-lantern"),
          progression: updateProgression(state.progression, {
            resonancePoints: state.progression.resonancePoints + 15,
            unlockedRecipes: Array.from(new Set([...state.progression.unlockedRecipes, "bridge-kit"])),
            completedQuestIds: [...state.progression.completedQuestIds, questId],
          }),
        },
        createMessage("퀘스트 완료", "등불 복구를 마쳤습니다. 공명 15와 다리 복구 키트 레시피를 해금했습니다."),
      );
    case "restore-bridge":
      return appendMessage(
        {
          ...state,
          settlement: unlockSettlementStructure(state.settlement, "north-bridge"),
          progression: updateProgression(state.progression, {
            resonancePoints: state.progression.resonancePoints + 20,
            completedQuestIds: [...state.progression.completedQuestIds, questId],
          }),
        },
        createMessage("퀘스트 완료", "무너진 통로를 복원했습니다. 공명 20과 다음 지역 단서를 얻었습니다."),
      );
    case "north-reach":
      return appendMessage(
        {
          ...state,
          settlement: unlockSettlementStructure(state.settlement, "north-outpost"),
          progression: updateProgression(state.progression, {
            resonancePoints: state.progression.resonancePoints + 12,
            completedQuestIds: [...state.progression.completedQuestIds, questId],
          }),
        },
        createMessage("퀘스트 완료", "북부 개척지 조사를 마쳤습니다. 공명 12와 북부 전초기지를 해금했습니다."),
      );
  }
}

function refreshQuestState(state: LifeSimState) {
  let nextState = state;
  const sink = createLifeSimEventSink(state.eventLog);

  const maybeComplete = (questId: LifeSimQuestId, condition: boolean) => {
    const current = nextState.quests.find((quest) => quest.id === questId);
    if (!condition || !current || current.status === "completed") return;
    nextState = {
      ...nextState,
      quests: completeQuest(nextState.quests, questId, nextState.time.day),
    };
    sink.emit("quest_completed", { questId });
    nextState = applyQuestDeepStakeInfluence(grantQuestReward(nextState, questId), questId);
  };

  maybeComplete("first-harvest", nextState.storyFlags.harvestedFirstCrop);
  maybeComplete("mine-recon", nextState.storyFlags.enteredMine);
  maybeComplete("repair-lantern", nextState.storyFlags.repairedLantern);
  maybeComplete("restore-bridge", nextState.storyFlags.restoredBridge);
  maybeComplete("north-reach", nextState.storyFlags.surveyedNorthReach);

  return nextState;
}

function progressCrops(state: LifeSimState) {
  return state.plots.map((plot) => {
    if (!plot.cropKind) return plot;
    const wateredToday = plot.wateredOnDay === state.time.day;
    const nextStage = wateredToday
      ? Math.min(plot.growthStage + 1 + state.healthBonuses.cropEfficiencyBonus, 3)
      : plot.growthStage;
    return {
      ...plot,
      wateredOnDay: undefined,
      growthStage: nextStage,
      readyToHarvest: nextStage >= 3,
    };
  });
}

function refreshResourceNodes(state: LifeSimState) {
  return state.resourceNodes.map((node) =>
    node.depletedUntilDay && node.depletedUntilDay <= state.time.day + 1 ? { ...node, depletedUntilDay: undefined } : node,
  );
}

function chooseDialogue(state: LifeSimState, npcId: LifeSimNpcId): LifeSimDialogueLine {
  const npcLines = lifeSimDialogue.filter((line) => line.speaker === npcId);
  const hasTurnip = (state.player.inventory.turnip || 0) > 0;
  const relationLevel = state.relationships[npcId].level;

  if (relationLevel >= 2) {
    const friendLine = npcLines.find((line) => line.id === `${npcId}-friend`);
    if (friendLine) return friendLine;
  }

  const priority = npcLines.find((line) => {
    switch (line.condition) {
      case "has-turnip":
        return hasTurnip;
      case "mine-visited":
        return state.storyFlags.enteredMine;
      case "low-energy":
        return state.player.energy <= Math.floor(state.player.maxEnergy * 0.3);
      case "first-day":
        return state.time.day === 1;
      default:
        return false;
    }
  });

  return priority || npcLines.find((line) => line.condition === "default") || npcLines[0];
}

export function isRecipeUnlocked(state: LifeSimState, recipeId: LifeSimRecipeId) {
  return state.progression.unlockedRecipes.includes(recipeId);
}

function canCraftRecipe(state: LifeSimState, recipeId: LifeSimRecipeId) {
  if (!isRecipeUnlocked(state, recipeId)) return false;
  const recipe = lifeSimRecipes[recipeId];
  return recipe.ingredients.every((ingredient) => (state.player.inventory[ingredient.itemId] || 0) >= ingredient.amount);
}

export function craftRecipe(state: LifeSimState, recipeId: LifeSimRecipeId): LifeSimActionResult {
  if (!isRecipeUnlocked(state, recipeId)) {
    return {
      state,
        message: createMessage("제작 잠김", "이 레시피는 아직 해금되지 않았습니다."),
    };
  }

  const recipe = lifeSimRecipes[recipeId];
  if (!canCraftRecipe(state, recipeId)) {
    return {
      state,
        message: createMessage("제작 실패", `${t(recipe.title)} 제작 재료가 부족합니다.`),
    };
  }

  const nextPlayer = { ...state.player, inventory: { ...state.player.inventory } };
  recipe.ingredients.forEach((ingredient) => {
    nextPlayer.inventory[ingredient.itemId] = Math.max(0, (nextPlayer.inventory[ingredient.itemId] || 0) - ingredient.amount);
  });
  nextPlayer.inventory[recipe.resultItemId] = (nextPlayer.inventory[recipe.resultItemId] || 0) + recipe.resultAmount;

  const sink = createLifeSimEventSink(state.eventLog);
  sink.emit("recipe_crafted", { recipeId, resultItemId: recipe.resultItemId });

  const nextState = appendMessage(
    {
      ...state,
      player: nextPlayer,
      storyFlags: {
        ...state.storyFlags,
        cookedFirstMeal: recipeId === "dawn-broth" ? true : state.storyFlags.cookedFirstMeal,
      },
    },
    createMessage("제작 완료", `${t(recipe.title)} 제작을 마쳤습니다.`),
  );

  return { state: refreshQuestState(nextState) };
}

export function consumeSelectedItem(state: LifeSimState): LifeSimActionResult {
  const selectedItem = useSelectedHotbarItem(state);
  if (selectedItem !== "dawn-broth") {
    return { state, message: createMessage("사용 불가", "지금 선택한 아이템은 직접 사용할 수 없습니다.") };
  }
  if ((state.player.inventory["dawn-broth"] || 0) <= 0) {
    return { state, message: createMessage("수프 부족", "새벽 수프가 인벤토리에 없습니다.") };
  }

  return {
    state: appendMessage(
      {
        ...state,
        player: {
          ...withInventoryDelta(state, "dawn-broth", -1),
          energy: Math.min(state.player.maxEnergy, state.player.energy + 6),
        },
      },
      createMessage("새벽 수프", "따뜻한 수프로 기력 6을 회복했습니다."),
    ),
  };
}

export function createInitialLifeSimState(
  bonuses: LifeSimHealthBonuses = { startEnergyBonus: 0, recoveryBonus: 0, cropEfficiencyBonus: 0 },
): LifeSimState {
  return {
    version: 4,
    slot: "main",
    player: {
      mapId: "farm",
      x: 5,
      y: 6,
      facing: "down",
      energy: 24 + bonuses.startEnergyBonus,
      maxEnergy: 24 + bonuses.startEnergyBonus,
      inventory: {
        hoe: 1,
        "watering-can": 1,
        pickaxe: 1,
        "turnip-seeds": 6,
        turnip: 0,
        "ore-fragment": 0,
        "scrap-bundle": 0,
        "resonance-shard": 0,
        "purity-lantern": 0,
        "dawn-broth": 0,
        "bridge-kit": 0,
      },
      hotbar: ["hoe", "turnip-seeds", "watering-can", "pickaxe", "dawn-broth"],
      selectedHotbarIndex: 0,
    },
    time: { day: 1, minutes: 6 * 60 },
    plots: [],
    resourceNodes: [
      { id: "ore-1", mapId: "mine", x: 8, y: 6, itemId: "ore-fragment" },
      { id: "scrap-1", mapId: "mine", x: 10, y: 8, itemId: "scrap-bundle" },
      { id: "resonance-1", mapId: "north-pass", x: 13, y: 5, itemId: "resonance-shard" },
    ],
    hazards: [
      {
        id: "wisp-1",
        mapId: "mine",
        x: 12,
        y: 7,
        direction: 1,
        axis: "y",
        range: [5, 9],
        label: { ko: "그림자 불꽃", en: "Shadow Ember" },
      },
    ],
    relationships: {
      archivist: { friendship: 0, level: 0, rewardedLevels: [] },
      mechanic: { friendship: 0, level: 0, rewardedLevels: [] },
    },
    storyFlags: {
      metArchivist: false,
      metMechanic: false,
      enteredMine: false,
      harvestedFirstCrop: false,
      repairedLantern: false,
      cookedFirstMeal: false,
      restoredBridge: false,
      surveyedNorthReach: false,
    },
    quests: initialLifeSimQuests,
    progression: {
      resonancePoints: 0,
      unlockedRecipes: ["dawn-broth"],
      discoveredMaps: ["farm", "village"],
      completedQuestIds: [],
    },
    deepStake: createInitialDeepStakeState(),
    settlement: createDefaultSettlement("새벽 거주지"),
    healthBonuses: bonuses,
    settings: createDefaultLifeSimSettings(),
    lastDialogue: undefined,
    messageLog: [
      createLocalizedMessage(
        { ko: "복구 농장", en: "Recovery Farm" },
        {
          ko: "농장은 거칠지만 긴 밤이 지나면 다시 새벽을 맞을 준비를 하고 있습니다.",
          en: "The farm is rough, but it is preparing to meet dawn again after a long night.",
        },
      ),
    ],
    eventLog: [],
  };
}

export function movePlayer(state: LifeSimState, facing: LifeSimFacing): LifeSimState {
  const offsets: Record<LifeSimFacing, [number, number]> = {
    up: [0, -1],
    down: [0, 1],
    left: [-1, 0],
    right: [1, 0],
  };
  const [dx, dy] = offsets[facing];
  const nextTile = getMapTile(state.player.mapId, state.player.x + dx, state.player.y + dy);
  const touchingHazard = state.hazards.find(
    (hazard) => hazard.mapId === state.player.mapId && hazard.x === state.player.x + dx && hazard.y === state.player.y + dy,
  );

  let nextState: LifeSimState = {
    ...state,
    player: { ...state.player, facing },
  };

  if (!nextTile || !nextTile.walkable) return nextState;

  nextState = {
    ...nextState,
    player: { ...nextState.player, x: nextTile.x, y: nextTile.y },
  };

  if (touchingHazard) {
    const sink = createLifeSimEventSink(nextState.eventLog);
    sink.emit("hazard_hit", { hazardId: touchingHazard.id });
    nextState = appendMessage(
      spendEnergy(nextState, 2),
      createMessage("그림자 충돌", `${t(touchingHazard.label)}에 부딪혀 기력 2를 잃었습니다.`),
    );
  }

  if (nextTile.warpTo) {
    nextState = appendMessage(
      {
        ...nextState,
        player: {
          ...nextState.player,
          mapId: nextTile.warpTo.mapId,
          x: nextTile.warpTo.x,
          y: nextTile.warpTo.y,
        },
        storyFlags: {
          ...nextState.storyFlags,
          enteredMine: nextTile.warpTo.mapId === "mine" ? true : nextState.storyFlags.enteredMine,
        },
        progression: {
          ...nextState.progression,
          discoveredMaps: Array.from(new Set([...nextState.progression.discoveredMaps, nextTile.warpTo.mapId])),
        },
      },
      createMessage(t(lifeSimMaps[nextTile.warpTo.mapId].name), t(lifeSimMaps[nextTile.warpTo.mapId].ambientHint)),
    );
  }

  if (
    nextState.player.mapId === "farm" &&
    nextState.storyFlags.restoredBridge &&
    nextState.player.x === 11 &&
    nextState.player.y === 5
  ) {
    nextState = appendMessage(
      {
        ...nextState,
        player: {
          ...nextState.player,
          mapId: "north-pass",
          x: 9,
          y: 12,
        },
        progression: {
          ...nextState.progression,
          discoveredMaps: Array.from(new Set([...nextState.progression.discoveredMaps, "north-pass"])),
        },
      },
      createMessage("북부 개척지", "복구된 북쪽 통로를 지나 새로운 거주 후보지에 도착했습니다."),
    );
  }

  return refreshQuestState(nextState);
}

export function cycleHazards(state: LifeSimState) {
  return {
    ...state,
    hazards: state.hazards.map((hazard) => {
      const next = hazard[hazard.axis] + hazard.direction;
      if (next < hazard.range[0] || next > hazard.range[1]) {
        return { ...hazard, direction: (hazard.direction === 1 ? -1 : 1) as 1 | -1 };
      }
      return { ...hazard, [hazard.axis]: next };
    }),
  };
}

export function advanceClock(state: LifeSimState, minutes = 10) {
  const nextMinutes = state.time.minutes + minutes;
  if (nextMinutes < 24 * 60) {
    return { ...state, time: { ...state.time, minutes: nextMinutes } };
  }
  return sleepUntilNextDay(state);
}

export function sleepUntilNextDay(state: LifeSimState) {
  const sink = createLifeSimEventSink(state.eventLog);
  sink.emit("day_ended", { day: state.time.day });
  const nextDay = state.time.day + 1;
  const recoveredEnergy = state.player.maxEnergy + state.healthBonuses.recoveryBonus;

  return refreshQuestState(
    appendMessage(
      {
        ...state,
        time: { day: nextDay, minutes: 6 * 60 },
        player: { ...state.player, mapId: "farm", x: 13, y: 3, energy: recoveredEnergy },
        plots: progressCrops(state),
        resourceNodes: refreshResourceNodes(state),
      },
      createMessage("다음 날", `${nextDay}일차 아침이 시작됐습니다. 기력을 회복하고 밭을 돌볼 시간입니다.`),
    ),
  );
}

export function useSelectedHotbarItem(state: LifeSimState) {
  return state.player.hotbar[state.player.selectedHotbarIndex] || "hoe";
}

export function useToolAction(state: LifeSimState): LifeSimActionResult {
  const selectedItem = useSelectedHotbarItem(state);
  const frontTile = getFrontTile(state);
  if (!frontTile) return { state };

  if (selectedItem === "dawn-broth") return consumeSelectedItem(state);

  if (selectedItem === "hoe") {
    if (state.player.mapId !== "farm" || !frontTile.tillable) {
        return { state, message: createMessage("괭이", "농장에서 경작 가능한 땅 앞에서만 사용할 수 있습니다.") };
    }
    const existing = getPlot(state, frontTile.x, frontTile.y);
    const plot: LifeSimCropPlot = existing || {
      x: frontTile.x,
      y: frontTile.y,
      tilled: true,
      growthStage: 0,
      readyToHarvest: false,
    };
    return {
      state: appendMessage(
        spendEnergy({ ...state, plots: upsertPlot(state, { ...plot, tilled: true }) }, 1),
        createMessage("밭 갈기", "땅을 갈아 씨앗을 심을 수 있는 밭을 만들었습니다."),
      ),
    };
  }

  if (selectedItem === "turnip-seeds") {
    const plot = getPlot(state, frontTile.x, frontTile.y);
    if (!plot?.tilled || plot.cropKind) {
        return { state, message: createMessage("씨앗 심기", "갈아 둔 빈 밭에서만 씨앗을 심을 수 있습니다.") };
    }
    if ((state.player.inventory["turnip-seeds"] || 0) <= 0) {
        return { state, message: createMessage("씨앗 부족", "심을 새벽 순무 씨앗이 없습니다.") };
    }
    const nextState = spendEnergy(
      {
        ...state,
        player: withInventoryDelta(state, "turnip-seeds", -1),
        plots: upsertPlot(state, {
          ...plot,
          cropKind: "turnip",
          plantedOnDay: state.time.day,
          growthStage: 0,
          readyToHarvest: false,
        }),
      },
      1,
    );
    return { state: appendMessage(nextState, createMessage("파종", "새벽 순무 씨앗을 심었습니다.")) };
  }

  if (selectedItem === "watering-can") {
    const plot = getPlot(state, frontTile.x, frontTile.y);
    if (!plot?.cropKind) {
        return { state, message: createMessage("물 주기", "작물이 자라는 밭에서만 물을 줄 수 있습니다.") };
    }
    return {
      state: appendMessage(
        spendEnergy({ ...state, plots: upsertPlot(state, { ...plot, wateredOnDay: state.time.day }) }, 1),
        createMessage("물 주기", "밭에 물을 주었습니다."),
      ),
    };
  }

  if (selectedItem === "pickaxe") {
    const node = state.resourceNodes.find(
      (entry) => entry.mapId === state.player.mapId && !entry.depletedUntilDay && entry.x === frontTile.x && entry.y === frontTile.y,
    );
    if (!node) {
        return { state, message: createMessage("채집", "캘 수 있는 광석이나 공명 결정이 없습니다.") };
    }
    const sink = createLifeSimEventSink(state.eventLog);
    sink.emit("resource_mined", { nodeId: node.id, itemId: node.itemId });
    const nextState = appendMessage(
      spendEnergy(
        {
          ...state,
          player: withInventoryDelta(state, node.itemId, 1),
          storyFlags: {
            ...state.storyFlags,
            surveyedNorthReach: node.itemId === "resonance-shard" ? true : state.storyFlags.surveyedNorthReach,
          },
          resourceNodes: state.resourceNodes.map((entry) =>
            entry.id === node.id ? { ...entry, depletedUntilDay: state.time.day + 1 } : entry,
          ),
        },
        2,
      ),
      createMessage("채집", `${t(lifeSimItems[node.itemId].name)} 1개를 얻었습니다.`),
    );
    return { state: refreshQuestState(nextState) };
  }

  if (selectedItem === "turnip") {
    return { state, message: createMessage("새벽 순무", "거래하거나 요리에 사용해 보세요.") };
  }

  if (selectedItem === "bridge-kit") {
    if (state.player.mapId !== "farm") {
        return { state, message: createMessage("다리 복구", "농장 북쪽의 무너진 통로 앞에서 사용해야 합니다.") };
    }
    if (frontTile.x !== 11 || frontTile.y !== 6) {
        return { state, message: createMessage("다리 복구", "통로 표시 앞에서 복구 키트를 사용해 보세요.") };
    }
    if ((state.player.inventory["bridge-kit"] || 0) <= 0) {
        return { state, message: createMessage("키트 부족", "복구 키트가 없습니다.") };
    }
    const sink = createLifeSimEventSink(state.eventLog);
    sink.emit("story_flag_changed", { flag: "restoredBridge", value: true });
    const nextState = appendMessage(
      {
        ...state,
        player: withInventoryDelta(state, "bridge-kit", -1),
        storyFlags: { ...state.storyFlags, restoredBridge: true },
      },
      createMessage("다리 복구", "농장 북쪽의 무너진 통로를 복구했습니다. 다음 지역으로 갈 준비가 됐습니다."),
    );
    return { state: refreshQuestState(nextState) };
  }

  return { state };
}

export function interactInWorld(state: LifeSimState): LifeSimActionResult {
  const frontTile = getFrontTile(state);
  if (!frontTile) return { state };

  const npc = getNpcAtPosition(state, state.player.mapId, frontTile.x, frontTile.y);
  if (npc) {
    const line = chooseDialogue(state, npc.id);
    const sink = createLifeSimEventSink(state.eventLog);
    sink.emit("npc_talked", { npcId: npc.id, lineId: line.id });

    const canRepairLantern =
      npc.id === "mechanic" && (state.player.inventory["purity-lantern"] || 0) > 0 && !state.storyFlags.repairedLantern;

    let nextState = appendMessage(
      {
        ...state,
        lastDialogue: { npcId: npc.id, lines: [t(line.text)] },
        relationships: updateRelationship(state.relationships, npc.id, state.time.day),
        storyFlags: {
          ...state.storyFlags,
          metArchivist: npc.id === "archivist" ? true : state.storyFlags.metArchivist,
          metMechanic: npc.id === "mechanic" ? true : state.storyFlags.metMechanic,
          repairedLantern: canRepairLantern ? true : state.storyFlags.repairedLantern,
        },
      },
      createMessage(t(npc.name), t(line.text)),
    );

    const deepStakeEcho = getDeepStakeNpcEcho(nextState, npc.id);
    if (deepStakeEcho) {
      nextState = appendMessage(nextState, createMessage(deepStakeEcho.title, deepStakeEcho.body));
    }

    nextState = applyRelationshipMilestoneRewards(nextState, npc.id);

    if (canRepairLantern) {
      sink.emit("story_flag_changed", { flag: "repairedLantern", value: true });
      nextState = appendMessage(
        nextState,
        createMessage("정화 등불", "정비공이 정화 등불을 조정했습니다. 광산 깊은 곳으로 갈 준비가 됐습니다."),
      );
    }

    return { state: refreshQuestState(nextState) };
  }

  if (frontTile.bed && state.player.mapId === "farm") {
    return { state: sleepUntilNextDay(state), message: createMessage("휴식", "하루를 마무리하고 다음 날로 넘어갑니다.") };
  }

  const plot = getPlot(state, frontTile.x, frontTile.y);
  if (plot?.readyToHarvest && plot.cropKind === "turnip") {
    const sink = createLifeSimEventSink(state.eventLog);
    sink.emit("crop_harvested", { cropKind: plot.cropKind });
    const nextState = appendMessage(
      {
        ...state,
        player: withInventoryDelta(state, "turnip", 1),
        plots: state.plots.map((entry) =>
          entry.x === plot.x && entry.y === plot.y
            ? { ...entry, cropKind: undefined, growthStage: 0, readyToHarvest: false, wateredOnDay: undefined }
            : entry,
        ),
        storyFlags: { ...state.storyFlags, harvestedFirstCrop: true },
      },
      createMessage("수확", "새벽 순무를 수확했습니다."),
    );
    return { state: refreshQuestState(nextState) };
  }

  if (frontTile.signText) {
    return { state: appendMessage(state, createMessage("표지판", t(frontTile.signText))) };
  }

  return { state, message: createMessage("탐색", "상호작용할 대상이 없습니다.") };
}

export function selectHotbarIndex(state: LifeSimState, index: number) {
  return {
    ...state,
    player: {
      ...state.player,
      selectedHotbarIndex: Math.max(0, Math.min(index, state.player.hotbar.length - 1)),
    },
  };
}

export function describeHotbarItem(itemId: LifeSimItemId) {
  return {
    title: t(lifeSimItems[itemId].name),
    body: t(lifeSimItems[itemId].description),
  };
}

export function getQuestLabel(questId: LifeSimQuestId) {
  return lifeSimQuestDefinitions[questId];
}
