import { lifeSimDialogue, lifeSimNpcs } from "@/game/life-sim/data/npcs";
import { lifeSimMaps } from "@/game/life-sim/data/maps";
import { lifeSimItems } from "@/game/life-sim/data/items";
import { t } from "@/game/life-sim/data/localization";
import { createLifeSimEventSink } from "@/game/life-sim/state/events";
import { createDefaultLifeSimSettings } from "@/game/life-sim/state/settings";
import type {
  LifeSimCropPlot,
  LifeSimDialogueLine,
  LifeSimFacing,
  LifeSimHealthBonuses,
  LifeSimItemId,
  LifeSimMapId,
  LifeSimNpcId,
  LifeSimState,
  LifeSimUiMessage,
} from "@/game/life-sim/types";

type LifeSimActionResult = {
  state: LifeSimState;
  message?: LifeSimUiMessage;
};

function createMessage(title: string, body: string): LifeSimUiMessage {
  return {
    id: `life-sim-message-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
    title,
    body,
  };
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

function upsertPlot(state: LifeSimState, patch: LifeSimCropPlot) {
  const index = state.plots.findIndex((plot) => plot.x === patch.x && plot.y === patch.y);
  if (index === -1) {
    return [...state.plots, patch];
  }
  return state.plots.map((plot, plotIndex) => (plotIndex === index ? patch : plot));
}

function getPlot(state: LifeSimState, x: number, y: number) {
  return state.plots.find((plot) => plot.x === x && plot.y === y) || null;
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

function chooseDialogue(state: LifeSimState, npcId: LifeSimNpcId): LifeSimDialogueLine {
  const npcLines = lifeSimDialogue.filter((line) => line.speaker === npcId);
  const hasTurnip = (state.player.inventory.turnip || 0) > 0;

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
      case "default":
      default:
        return false;
    }
  });

  return priority || npcLines.find((line) => line.condition === "default") || npcLines[0];
}

function progressCrops(state: LifeSimState) {
  return state.plots.map((plot) => {
    if (!plot.cropKind) {
      return plot;
    }
    const wateredYesterday = plot.wateredOnDay === state.time.day;
    const nextStage = wateredYesterday ? Math.min(plot.growthStage + 1 + state.healthBonuses.cropEfficiencyBonus, 3) : plot.growthStage;
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
    node.depletedUntilDay && node.depletedUntilDay <= state.time.day + 1
      ? { ...node, depletedUntilDay: undefined }
      : node,
  );
}

export function createInitialLifeSimState(
  bonuses: LifeSimHealthBonuses = {
    startEnergyBonus: 0,
    recoveryBonus: 0,
    cropEfficiencyBonus: 0,
  },
): LifeSimState {
  return {
    version: 1,
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
        "purity-lantern": 0,
      },
      hotbar: ["hoe", "turnip-seeds", "watering-can", "pickaxe", "turnip"],
      selectedHotbarIndex: 0,
    },
    time: {
      day: 1,
      minutes: 6 * 60,
    },
    plots: [],
    resourceNodes: [
      { id: "ore-1", mapId: "mine", x: 8, y: 6, itemId: "ore-fragment" },
      { id: "scrap-1", mapId: "mine", x: 10, y: 8, itemId: "scrap-bundle" },
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
      archivist: { friendship: 0 },
      mechanic: { friendship: 0 },
    },
    storyFlags: {
      metArchivist: false,
      metMechanic: false,
      enteredMine: false,
      harvestedFirstCrop: false,
      repairedLantern: false,
    },
    healthBonuses: bonuses,
    settings: createDefaultLifeSimSettings(),
    lastDialogue: undefined,
    messageLog: [
      createMessage("복원 농장", "농장은 아직 거칠지만 오늘부터 다시 일으켜 세울 수 있습니다."),
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

  let nextState = {
    ...state,
    player: {
      ...state.player,
      facing,
    },
  };

  if (!nextTile || !nextTile.walkable) {
    return nextState;
  }

  nextState = {
    ...nextState,
    player: {
      ...nextState.player,
      x: nextTile.x,
      y: nextTile.y,
    },
  };

  if (touchingHazard) {
    const sink = createLifeSimEventSink(nextState.eventLog);
    sink.emit("hazard_hit", { hazardId: touchingHazard.id });
    nextState = appendMessage(
      spendEnergy(nextState, 2),
      createMessage("그림자 충돌", `${t(touchingHazard.label)}에 스쳤습니다. 에너지 2를 잃었습니다.`),
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
      },
      createMessage(lifeSimMaps[nextTile.warpTo.mapId].name.ko, lifeSimMaps[nextTile.warpTo.mapId].ambientHint.ko),
    );
  }

  return nextState;
}

export function cycleHazards(state: LifeSimState) {
  return {
    ...state,
    hazards: state.hazards.map((hazard) => {
      const next = hazard[hazard.axis] + hazard.direction;
      if (next < hazard.range[0] || next > hazard.range[1]) {
        return {
          ...hazard,
          direction: (hazard.direction === 1 ? -1 : 1) as 1 | -1,
        };
      }
      return {
        ...hazard,
        [hazard.axis]: next,
      };
    }),
  };
}

export function advanceClock(state: LifeSimState, minutes = 10) {
  const nextMinutes = state.time.minutes + minutes;
  if (nextMinutes < 24 * 60) {
    return {
      ...state,
      time: {
        ...state.time,
        minutes: nextMinutes,
      },
    };
  }
  return sleepUntilNextDay(state);
}

export function sleepUntilNextDay(state: LifeSimState) {
  const sink = createLifeSimEventSink(state.eventLog);
  sink.emit("day_ended", { day: state.time.day });

  const nextDay = state.time.day + 1;
  const recoveredEnergy = state.player.maxEnergy + state.healthBonuses.recoveryBonus;

  return appendMessage(
    {
      ...state,
      time: {
        day: nextDay,
        minutes: 6 * 60,
      },
      player: {
        ...state.player,
        mapId: "farm",
        x: 13,
        y: 3,
        energy: recoveredEnergy,
      },
      plots: progressCrops(state),
      resourceNodes: refreshResourceNodes(state),
    },
    createMessage("다음 날", `Day ${nextDay}가 시작되었습니다. 에너지가 회복되고 작물이 자랐습니다.`),
  );
}

export function useSelectedHotbarItem(state: LifeSimState) {
  return state.player.hotbar[state.player.selectedHotbarIndex] || "hoe";
}

export function useToolAction(state: LifeSimState): LifeSimActionResult {
  const selectedItem = useSelectedHotbarItem(state);
  const frontTile = getFrontTile(state);
  if (!frontTile) {
    return { state };
  }

  if (selectedItem === "hoe") {
    if (state.player.mapId !== "farm" || !frontTile.tillable) {
      return { state, message: createMessage("괭이", "농장에 있는 경작 가능한 흙에서만 사용할 수 있습니다.") };
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
        spendEnergy(
          {
            ...state,
            plots: upsertPlot(state, { ...plot, tilled: true }),
          },
          1,
        ),
        createMessage("밭 갈기", "흙을 갈아 씨앗을 심을 수 있는 상태로 만들었습니다."),
      ),
    };
  }

  if (selectedItem === "turnip-seeds") {
    const plot = getPlot(state, frontTile.x, frontTile.y);
    if (!plot?.tilled || plot.cropKind) {
      return { state, message: createMessage("씨앗 심기", "갈아 둔 빈 흙에서만 씨앗을 심을 수 있습니다.") };
    }
    if ((state.player.inventory["turnip-seeds"] || 0) <= 0) {
      return { state, message: createMessage("씨앗 심기", "심을 씨앗이 없습니다.") };
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
    return { state: appendMessage(nextState, createMessage("파종", "순무 씨앗을 심었습니다.")) };
  }

  if (selectedItem === "watering-can") {
    const plot = getPlot(state, frontTile.x, frontTile.y);
    if (!plot?.cropKind) {
      return { state, message: createMessage("물 주기", "씨앗이나 작물이 심긴 흙에서만 물을 줄 수 있습니다.") };
    }
    return {
      state: appendMessage(
        spendEnergy(
          {
            ...state,
            plots: upsertPlot(state, { ...plot, wateredOnDay: state.time.day }),
          },
          1,
        ),
        createMessage("물 주기", "작물에 물을 주었습니다."),
      ),
    };
  }

  if (selectedItem === "pickaxe") {
    const node = state.resourceNodes.find(
      (entry) =>
        entry.mapId === state.player.mapId &&
        !entry.depletedUntilDay &&
        entry.x === frontTile.x &&
        entry.y === frontTile.y,
    );
    if (!node) {
      return { state, message: createMessage("채굴", "부술 수 있는 광석이나 잔해가 없습니다.") };
    }
    const sink = createLifeSimEventSink(state.eventLog);
    sink.emit("resource_mined", { nodeId: node.id, itemId: node.itemId });
    return {
      state: appendMessage(
        spendEnergy(
          {
            ...state,
            player: withInventoryDelta(state, node.itemId, 1),
            resourceNodes: state.resourceNodes.map((entry) =>
              entry.id === node.id ? { ...entry, depletedUntilDay: state.time.day + 1 } : entry,
            ),
          },
          2,
        ),
        createMessage("채굴", `${t(lifeSimItems[node.itemId].name)} 1개를 획득했습니다.`),
      ),
    };
  }

  if (selectedItem === "turnip") {
    return { state, message: createMessage("순무", "마을 주민과의 대화나 의뢰에 사용해 보세요.") };
  }

  return { state };
}

export function interactInWorld(state: LifeSimState): LifeSimActionResult {
  const frontTile = getFrontTile(state);
  if (!frontTile) {
    return { state };
  }

  const npc = lifeSimNpcs.find(
    (entry) => entry.mapId === state.player.mapId && entry.x === frontTile.x && entry.y === frontTile.y,
  );

  if (npc) {
    const line = chooseDialogue(state, npc.id);
    const sink = createLifeSimEventSink(state.eventLog);
    sink.emit("npc_talked", { npcId: npc.id, lineId: line.id });

    const receivesLantern = npc.id === "mechanic" && (state.player.inventory.turnip || 0) > 0;

    return {
      state: appendMessage(
        {
          ...state,
          lastDialogue: {
            npcId: npc.id,
            lines: [line.text.ko],
          },
          relationships: {
            ...state.relationships,
            [npc.id]: {
              friendship: state.relationships[npc.id].friendship + 1,
              lastTalkDay: state.time.day,
            },
          },
          storyFlags: {
            ...state.storyFlags,
            metArchivist: npc.id === "archivist" ? true : state.storyFlags.metArchivist,
            metMechanic: npc.id === "mechanic" ? true : state.storyFlags.metMechanic,
            repairedLantern: receivesLantern ? true : state.storyFlags.repairedLantern,
          },
          player: receivesLantern
            ? {
                ...withInventoryDelta(state, "turnip", -1),
                inventory: {
                  ...withInventoryDelta(state, "turnip", -1).inventory,
                  "purity-lantern": (state.player.inventory["purity-lantern"] || 0) + 1,
                },
              }
            : state.player,
        },
        createMessage(t(npc.name), line.text.ko),
      ),
    };
  }

  if (frontTile.bed && state.player.mapId === "farm") {
    return {
      state: sleepUntilNextDay(state),
      message: createMessage("취침", "침대에 들어 다음 날을 시작합니다."),
    };
  }

  const plot = getPlot(state, frontTile.x, frontTile.y);
  if (plot?.readyToHarvest && plot.cropKind === "turnip") {
    const sink = createLifeSimEventSink(state.eventLog);
    sink.emit("crop_harvested", { cropKind: plot.cropKind });
    return {
      state: appendMessage(
        {
          ...state,
          player: withInventoryDelta(state, "turnip", 1),
          plots: state.plots.map((entry) =>
            entry.x === plot.x && entry.y === plot.y
              ? { ...entry, cropKind: undefined, growthStage: 0, readyToHarvest: false, wateredOnDay: undefined }
              : entry,
          ),
          storyFlags: {
            ...state.storyFlags,
            harvestedFirstCrop: true,
          },
        },
        createMessage("수확", "순무를 수확했습니다."),
      ),
    };
  }

  if (frontTile.signText) {
    return {
      state: appendMessage(state, createMessage("표지판", frontTile.signText.ko)),
    };
  }

  return {
    state,
    message: createMessage("탐색", "상호작용할 대상이 없습니다."),
  };
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
