import { useMemo, useState } from "react";
import { Hammer, Home, PaintBucket, Trash2 } from "lucide-react";
import { settlementObjectPalette, settlementTerrainPalette } from "@/game/settlement/settlementPalette";
import type { SettlementObjectType, SettlementState, SettlementTerrain } from "@/game/settlement/settlementTypes";

type BuilderTool = "paint" | "place" | "erase";

type Props = {
  state: SettlementState;
  onPaint: (x: number, y: number, terrain: SettlementTerrain) => void;
  onPlace: (x: number, y: number, type: SettlementObjectType) => void;
  onRemove: (x: number, y: number) => void;
};

function terrainColor(terrain: SettlementTerrain) {
  switch (terrain) {
    case "grass":
      return "bg-emerald-700";
    case "sand":
      return "bg-amber-500";
    case "water":
      return "bg-sky-500";
    case "track":
      return "bg-orange-500";
    case "stone":
      return "bg-slate-500";
    case "garden":
      return "bg-lime-700";
    case "plaza":
      return "bg-stone-300 text-slate-900";
  }
}

export function SettlementBuilderPanel({ state, onPaint, onPlace, onRemove }: Props) {
  const [tool, setTool] = useState<BuilderTool>("paint");
  const [selectedTerrain, setSelectedTerrain] = useState<SettlementTerrain>("garden");
  const [selectedObject, setSelectedObject] = useState<SettlementObjectType>("home-core");

  const objectMap = useMemo(() => {
    const map = new Map<string, { emoji: string; label: string }>();
    settlementObjectPalette.forEach((entry) => map.set(entry.type, { emoji: entry.emoji, label: entry.label }));
    return map;
  }, []);

  const handleCellClick = (x: number, y: number) => {
    if (tool === "paint") onPaint(x, y, selectedTerrain);
    if (tool === "place") onPlace(x, y, selectedObject);
    if (tool === "erase") onRemove(x, y);
  };

  return (
    <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
      <div className="flex items-center gap-2 font-medium">
        <Home className="h-4 w-4 text-primary" />
        정착지 확장
      </div>
      <p className="mt-2 text-xs text-slate-400">
        FitCraft Island에서 다듬은 2D 건설 흐름을 Fifth Dawn의 거주지 확장과 정착지 복구 모드로 수렴한 기초 패널입니다.
      </p>

      <div className="mt-4 grid grid-cols-3 gap-2">
        <button
          type="button"
          onClick={() => setTool("paint")}
          className={`rounded-xl border px-3 py-2 ${tool === "paint" ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"}`}
        >
          <span className="inline-flex items-center gap-2">
            <PaintBucket className="h-4 w-4" />
            지형
          </span>
        </button>
        <button
          type="button"
          onClick={() => setTool("place")}
          className={`rounded-xl border px-3 py-2 ${tool === "place" ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"}`}
        >
          <span className="inline-flex items-center gap-2">
            <Hammer className="h-4 w-4" />
            배치
          </span>
        </button>
        <button
          type="button"
          onClick={() => setTool("erase")}
          className={`rounded-xl border px-3 py-2 ${tool === "erase" ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"}`}
        >
          <span className="inline-flex items-center gap-2">
            <Trash2 className="h-4 w-4" />
            지우기
          </span>
        </button>
      </div>

      {tool === "paint" ? (
        <div className="mt-4 grid grid-cols-2 gap-2">
          {settlementTerrainPalette.map((entry) => (
            <button
              key={entry.terrain}
              type="button"
              onClick={() => setSelectedTerrain(entry.terrain)}
              className={`rounded-xl border px-3 py-2 text-left ${selectedTerrain === entry.terrain ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"}`}
            >
              {entry.label}
            </button>
          ))}
        </div>
      ) : null}

      {tool === "place" ? (
        <div className="mt-4 grid grid-cols-2 gap-2">
          {settlementObjectPalette.map((entry) => (
            <button
              key={entry.type}
              type="button"
              onClick={() => setSelectedObject(entry.type)}
              className={`rounded-xl border px-3 py-2 text-left ${selectedObject === entry.type ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"}`}
            >
              <span className="mr-2">{entry.emoji}</span>
              {entry.label}
            </button>
          ))}
        </div>
      ) : null}

      <div className="mt-4 grid gap-1 rounded-xl border border-white/10 bg-slate-950/40 p-2" style={{ gridTemplateColumns: `repeat(${state.width}, minmax(0, 1fr))` }}>
        {state.tiles.map((tile) => {
          const object = state.objects.find((entry) => entry.x === tile.x && entry.y === tile.y);
          return (
            <button
              key={`${tile.x}-${tile.y}`}
              type="button"
              onClick={() => handleCellClick(tile.x, tile.y)}
              className={`relative aspect-square rounded-md border border-black/10 text-[10px] ${terrainColor(tile.terrain)}`}
            >
              {object ? objectMap.get(object.type)?.emoji || "•" : ""}
            </button>
          );
        })}
      </div>

      <div className="mt-3 text-xs text-slate-400">
        테마: {state.theme} · 좋아요 {state.likes} · 방문 {state.visits}
      </div>
    </div>
  );
}
