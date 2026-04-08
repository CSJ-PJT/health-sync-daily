import { lifeSimMaps } from "@/game/life-sim/data/maps";
import { t } from "@/game/life-sim/data/localization";
import { getScheduledNpcsForMap } from "@/game/life-sim/state/npcSchedule";
import type { LifeSimMapId, LifeSimState, LifeSimTerrain, LifeSimTile } from "@/game/life-sim/types";

const TILE_SIZE = 36;

function terrainColor(terrain: LifeSimTerrain) {
  switch (terrain) {
    case "grass":
      return "#587d43";
    case "path":
      return "#b99562";
    case "soil":
      return "#6e5232";
    case "tilled":
      return "#5a3c20";
    case "water":
      return "#3b82f6";
    case "wood":
      return "#6b4f33";
    case "stone":
      return "#6b7280";
    case "floor":
      return "#d8c9a8";
    case "ruin":
      return "#475569";
    case "wall":
      return "#1f2937";
  }
}

function drawTile(ctx: CanvasRenderingContext2D, tile: LifeSimTile, state: LifeSimState, mapId: LifeSimMapId) {
  const plot = mapId === "farm" ? state.plots.find((entry) => entry.x === tile.x && entry.y === tile.y) : null;
  const color = plot?.tilled ? terrainColor("tilled") : terrainColor(tile.terrain);
  const px = tile.x * TILE_SIZE;
  const py = tile.y * TILE_SIZE;

  ctx.fillStyle = color;
  ctx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
  ctx.strokeStyle = "rgba(0,0,0,0.14)";
  ctx.strokeRect(px, py, TILE_SIZE, TILE_SIZE);

  if (plot?.cropKind) {
    ctx.fillStyle = plot.readyToHarvest ? "#84cc16" : plot.wateredOnDay === state.time.day ? "#22c55e" : "#65a30d";
    const size = 8 + plot.growthStage * 4;
    ctx.fillRect(px + 12, py + (TILE_SIZE - size) - 6, size, size);
  }

  if (tile.bed) {
    ctx.fillStyle = "#c084fc";
    ctx.fillRect(px + 6, py + 8, TILE_SIZE - 12, TILE_SIZE - 16);
  }

  if (tile.warpTo) {
    ctx.fillStyle = "rgba(255,255,255,0.18)";
    ctx.fillRect(px + 10, py + 10, TILE_SIZE - 20, TILE_SIZE - 20);
  }
}

export function renderLifeSimScene(canvas: HTMLCanvasElement, state: LifeSimState) {
  const ctx = canvas.getContext("2d");
  if (!ctx) return;

  const map = lifeSimMaps[state.player.mapId];
  const scale = state.settings.resolutionScale || 1;
  canvas.width = map.width * TILE_SIZE;
  canvas.height = map.height * TILE_SIZE;
  canvas.style.width = `${Math.round(canvas.width * scale)}px`;
  canvas.style.height = `${Math.round(canvas.height * scale)}px`;

  ctx.clearRect(0, 0, canvas.width, canvas.height);

  map.tiles.forEach((tile) => drawTile(ctx, tile, state, map.id));

  state.resourceNodes
    .filter((node) => node.mapId === map.id && !node.depletedUntilDay)
    .forEach((node) => {
      ctx.fillStyle = node.itemId === "ore-fragment" ? "#f59e0b" : "#94a3b8";
      ctx.fillRect(node.x * TILE_SIZE + 8, node.y * TILE_SIZE + 8, TILE_SIZE - 16, TILE_SIZE - 16);
    });

  state.hazards
    .filter((hazard) => hazard.mapId === map.id)
    .forEach((hazard) => {
      ctx.fillStyle = "#ef4444";
      ctx.beginPath();
      ctx.arc(hazard.x * TILE_SIZE + TILE_SIZE / 2, hazard.y * TILE_SIZE + TILE_SIZE / 2, 9, 0, Math.PI * 2);
      ctx.fill();
    });

  getScheduledNpcsForMap(state, map.id).forEach(({ npc, stop }) => {
    ctx.fillStyle = npc.id === "archivist" ? "#a78bfa" : "#38bdf8";
    ctx.fillRect(stop.x * TILE_SIZE + 10, stop.y * TILE_SIZE + 6, TILE_SIZE - 20, TILE_SIZE - 12);
    ctx.fillStyle = "#ffffff";
    ctx.font = "11px sans-serif";
    ctx.fillText(t(npc.name), stop.x * TILE_SIZE + 2, stop.y * TILE_SIZE + TILE_SIZE - 4);
  });

  ctx.fillStyle = "#f8fafc";
  ctx.fillRect(state.player.x * TILE_SIZE + 9, state.player.y * TILE_SIZE + 6, TILE_SIZE - 18, TILE_SIZE - 12);
  ctx.fillStyle = "#0f172a";
  if (state.player.facing === "up") ctx.fillRect(state.player.x * TILE_SIZE + 15, state.player.y * TILE_SIZE + 4, 6, 6);
  if (state.player.facing === "down") ctx.fillRect(state.player.x * TILE_SIZE + 15, state.player.y * TILE_SIZE + TILE_SIZE - 10, 6, 6);
  if (state.player.facing === "left") ctx.fillRect(state.player.x * TILE_SIZE + 4, state.player.y * TILE_SIZE + 14, 6, 6);
  if (state.player.facing === "right") ctx.fillRect(state.player.x * TILE_SIZE + TILE_SIZE - 10, state.player.y * TILE_SIZE + 14, 6, 6);

  ctx.fillStyle = "rgba(15,23,42,0.72)";
  ctx.fillRect(10, 10, 300, 60);
  ctx.fillStyle = "#f8fafc";
  ctx.font = "13px sans-serif";
  ctx.fillText(`${t(map.name)} · ${state.time.day}일차`, 20, 30);
  const hour = Math.floor(state.time.minutes / 60);
  const minute = state.time.minutes % 60;
  ctx.fillText(`시간 ${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")} · 기력 ${state.player.energy}/${state.player.maxEnergy}`, 20, 50);

  if (state.settings.showPerformanceOverlay) {
    ctx.fillStyle = "rgba(15,23,42,0.72)";
    ctx.fillRect(canvas.width - 170, 10, 160, 64);
    ctx.fillStyle = "#cbd5e1";
    ctx.fillText(`맵 타일 ${map.width}x${map.height}`, canvas.width - 160, 30);
    ctx.fillText(`자원 노드 ${state.resourceNodes.length}`, canvas.width - 160, 48);
    ctx.fillText(`로그 ${state.eventLog.length}`, canvas.width - 160, 66);
  }
}
