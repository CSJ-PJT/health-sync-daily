import type { SettlementObject, SettlementObjectType, SettlementState, SettlementTerrain } from "@/game/settlement/settlementTypes";

export function createDefaultSettlement(title = "새벽 거주지"): SettlementState {
  const width = 8;
  const height = 8;
  const tiles = Array.from({ length: width * height }, (_, index) => {
    const x = index % width;
    const y = Math.floor(index / width);
    return {
      x,
      y,
      terrain: y >= 5 ? "garden" : y === 3 ? "plaza" : "grass",
    } as const;
  });

  return {
    worldId: "fifth-dawn-home",
    version: 1,
    width,
    height,
    title,
    theme: "recovery-farm",
    tiles,
    objects: [
      { id: "home-core", type: "home-core", x: 3, y: 2 },
      { id: "beacon-1", type: "beacon", x: 4, y: 2 },
      { id: "tree-1", type: "tree", x: 1, y: 1 },
      { id: "garden-1", type: "garden-bed", x: 5, y: 6 },
    ],
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

export function placeSettlementObject(
  state: SettlementState,
  x: number,
  y: number,
  type: SettlementObjectType,
): SettlementState {
  const existing = state.objects.find((entry) => entry.x === x && entry.y === y);
  const nextObject: SettlementObject = existing
    ? { ...existing, type }
    : {
        id: `settlement-object-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
        type,
        x,
        y,
      };

  return {
    ...state,
    objects: existing
      ? state.objects.map((entry) => (entry.id === existing.id ? nextObject : entry))
      : [...state.objects, nextObject],
  };
}

export function removeSettlementObject(state: SettlementState, x: number, y: number): SettlementState {
  return {
    ...state,
    objects: state.objects.filter((entry) => !(entry.x === x && entry.y === y)),
  };
}
