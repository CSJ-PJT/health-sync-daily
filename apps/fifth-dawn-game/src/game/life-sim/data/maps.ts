import type { LifeSimMapDefinition, LifeSimTile } from "@/game/life-sim/types";

function tile(x: number, y: number, partial: Partial<LifeSimTile>): LifeSimTile {
  return {
    x,
    y,
    terrain: "grass",
    walkable: true,
    ...partial,
  };
}

function buildFarmTiles(): LifeSimTile[] {
  const tiles: LifeSimTile[] = [];
  for (let y = 0; y < 14; y += 1) {
    for (let x = 0; x < 18; x += 1) {
      const isWall = x === 0 || y === 0 || x === 17 || y === 13;
      const isField = x >= 3 && x <= 8 && y >= 4 && y <= 9;
      const isPond = x >= 12 && x <= 15 && y >= 8 && y <= 11;
      const isCabin = x >= 12 && x <= 15 && y >= 2 && y <= 5;

      if (isWall) {
        tiles.push(tile(x, y, { terrain: "wood", walkable: false }));
      } else if (isPond) {
        tiles.push(tile(x, y, { terrain: "water", walkable: false }));
      } else if (isCabin) {
        tiles.push(tile(x, y, { terrain: "floor", walkable: true }));
      } else if (isField) {
        tiles.push(tile(x, y, { terrain: "soil", tillable: true }));
      } else {
        tiles.push(tile(x, y, { terrain: "grass" }));
      }
    }
  }

  return tiles.map((entry) => {
    if (entry.x === 9 && entry.y === 6) {
      return { ...entry, terrain: "path", warpTo: { mapId: "village", x: 2, y: 7 } };
    }
    if (entry.x === 13 && entry.y === 3) {
      return {
        ...entry,
        bed: true,
        signText: { ko: "여기서 휴식하면 다음 날 아침으로 넘어갑니다.", en: "You can sleep here." },
      };
    }
    if (entry.x === 11 && entry.y === 6) {
      return {
        ...entry,
        signText: {
          ko: "농장 북쪽 무너진 통로는 복구 키트가 있으면 다시 열릴 것 같습니다.",
          en: "The broken northern passage may reopen with a repair kit.",
        },
      };
    }
    return entry;
  });
}

function buildVillageTiles(): LifeSimTile[] {
  const tiles: LifeSimTile[] = [];
  for (let y = 0; y < 14; y += 1) {
    for (let x = 0; x < 18; x += 1) {
      const isBorder = x === 0 || y === 0 || x === 17 || y === 13;
      const isSquare = x >= 4 && x <= 13 && y >= 4 && y <= 10;
      if (isBorder) {
        tiles.push(tile(x, y, { terrain: "stone", walkable: false }));
      } else if (isSquare) {
        tiles.push(tile(x, y, { terrain: "path" }));
      } else {
        tiles.push(tile(x, y, { terrain: "grass" }));
      }
    }
  }

  return tiles.map((entry) => {
    if (entry.x === 1 && entry.y === 7) {
      return { ...entry, warpTo: { mapId: "farm", x: 9, y: 6 } };
    }
    if (entry.x === 15 && entry.y === 7) {
      return { ...entry, warpTo: { mapId: "mine", x: 2, y: 6 } };
    }
    if (entry.x === 9 && entry.y === 3) {
      return {
        ...entry,
        signText: {
          ko: "광장 아래에는 깊은 기록 보관고와 닫힌 정화 관로가 잠들어 있다는 소문이 있습니다.",
          en: "Rumor says a deep archive and sealed purifier lines sleep beneath the square.",
        },
      };
    }
    return entry;
  });
}

function buildMineTiles(): LifeSimTile[] {
  const tiles: LifeSimTile[] = [];
  for (let y = 0; y < 14; y += 1) {
    for (let x = 0; x < 18; x += 1) {
      const isBorder = x === 0 || y === 0 || x === 17 || y === 13;
      const isCenterRuin = x >= 6 && x <= 11 && y >= 4 && y <= 9;
      if (isBorder) {
        tiles.push(tile(x, y, { terrain: "wall", walkable: false }));
      } else if (isCenterRuin) {
        tiles.push(tile(x, y, { terrain: "ruin" }));
      } else {
        tiles.push(tile(x, y, { terrain: "stone" }));
      }
    }
  }

  return tiles.map((entry) => {
    if (entry.x === 1 && entry.y === 6) {
      return { ...entry, warpTo: { mapId: "village", x: 15, y: 7 } };
    }
    if (entry.x === 12 && entry.y === 5) {
      return {
        ...entry,
        signText: {
          ko: "부서진 문양 아래에서 오래된 그림자 행정 구조의 흔적이 미세하게 빛납니다.",
          en: "Beneath the broken sigils, traces of an old shadow administration flicker.",
        },
      };
    }
    return entry;
  });
}

export const lifeSimMaps: Record<string, LifeSimMapDefinition> = {
  farm: {
    id: "farm",
    name: { ko: "복구 농장", en: "Recovery Farm" },
    width: 18,
    height: 14,
    tiles: buildFarmTiles(),
    ambientHint: {
      ko: "오래된 정착지 가장자리에 있는 농장입니다. 새벽 바람이 이 땅이 다시 살아날 수 있다고 속삭입니다.",
      en: "A farm at the edge of an old settlement. The dawn wind says this soil can recover.",
    },
  },
  village: {
    id: "village",
    name: { ko: "여명 광장", en: "Dawn Square" },
    width: 18,
    height: 14,
    tiles: buildVillageTiles(),
    ambientHint: {
      ko: "평화로운 광장 아래에는 깊은 기록과 잊힌 기반 시설이 잠들어 있습니다.",
      en: "Beneath the peaceful square lie sealed records and forgotten infrastructure.",
    },
  },
  mine: {
    id: "mine",
    name: { ko: "정화 광산", en: "Purifier Ruins" },
    width: 18,
    height: 14,
    tiles: buildMineTiles(),
    ambientHint: {
      ko: "무너진 통로와 정화 관로가 뒤엉킨 곳입니다. 지하의 오래된 그림자는 아직 완전히 사라지지 않았습니다.",
      en: "Collapsed passages and purifier conduits twist together here. The old underground shadows have not fully faded.",
    },
  },
};
