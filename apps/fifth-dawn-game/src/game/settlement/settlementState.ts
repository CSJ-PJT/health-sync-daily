import { settlementObjectPalette } from "@/game/settlement/settlementPalette";
import type {
  SettlementObject,
  SettlementObjectType,
  SettlementState,
  SettlementTerrain,
  SettlementUpgradeLevel,
} from "@/game/settlement/settlementTypes";

function buildDefaultTiles(width: number, height: number) {
  return Array.from({ length: width * height }, (_, index) => {
    const x = index % width;
    const y = Math.floor(index / width);
    return {
      x,
      y,
      terrain: x === 0 || y === 0 || x === width - 1 || y === height - 1 ? "stone" : "grass",
    } as const;
  });
}

function createObject(type: SettlementObjectType, x: number, y: number): SettlementObject {
  return {
    id: `settlement-object-${type}-${x}-${y}-${Math.random().toString(36).slice(2, 8)}`,
    type,
    x,
    y,
  };
}

function resolveUnlockedObjectTypes(level: SettlementUpgradeLevel, existing?: SettlementObjectType[]) {
  const byLevel = settlementObjectPalette
    .filter((entry) => entry.unlockLevel <= level)
    .map((entry) => entry.type);
  return Array.from(new Set([...(existing || []), ...byLevel]));
}

export function createDefaultSettlement(title = "새벽 거주지"): SettlementState {
  return {
    worldId: "fifth-dawn-settlement",
    version: 2,
    width: 8,
    height: 8,
    title,
    theme: "recovery-farm",
    level: 1,
    tiles: buildDefaultTiles(8, 8),
    objects: [createObject("home-core", 3, 3), createObject("garden-bed", 4, 4)],
    unlockedObjectTypes: resolveUnlockedObjectTypes(1),
    restoredStructures: [],
    likes: 0,
    visits: 0,
  };
}

export function paintSettlementTile(state: SettlementState, x: number, y: number, terrain: SettlementTerrain): SettlementState {
  return {
    ...state,
    tiles: state.tiles.map((tile) => (tile.x === x && tile.y === y ? { ...tile, terrain } : tile)),
  };
}

export function placeSettlementObject(state: SettlementState, x: number, y: number, type: SettlementObjectType): SettlementState {
  if (!state.unlockedObjectTypes.includes(type)) {
    return state;
  }

  const nextObjects = state.objects.filter((entry) => !(entry.x === x && entry.y === y));
  nextObjects.push(createObject(type, x, y));

  return {
    ...state,
    objects: nextObjects,
  };
}

export function removeSettlementObject(state: SettlementState, x: number, y: number): SettlementState {
  return {
    ...state,
    objects: state.objects.filter((entry) => !(entry.x === x && entry.y === y)),
  };
}

export function unlockSettlementStructure(state: SettlementState, structureKey: string): SettlementState {
  if (state.restoredStructures.includes(structureKey)) {
    return state;
  }

  return {
    ...state,
    restoredStructures: [...state.restoredStructures, structureKey],
  };
}

export function upgradeSettlement(state: SettlementState): SettlementState {
  const nextLevel = Math.min(3, state.level + 1) as SettlementUpgradeLevel;
  if (nextLevel === state.level) {
    return state;
  }

  return {
    ...state,
    level: nextLevel,
    unlockedObjectTypes: resolveUnlockedObjectTypes(nextLevel, state.unlockedObjectTypes),
    restoredStructures: Array.from(new Set([...state.restoredStructures, `level-${nextLevel}`])),
  };
}
