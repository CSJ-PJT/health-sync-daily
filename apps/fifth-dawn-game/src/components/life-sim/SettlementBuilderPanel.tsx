import { useMemo, useState } from "react";
import { Hammer, Home, PaintBucket, Sparkles, Trash2 } from "lucide-react";
import { settlementObjectPalette, settlementTerrainPalette, settlementThemePalette } from "@/game/settlement/settlementPalette";
import type { SettlementObjectType, SettlementState, SettlementTerrain, SettlementTheme } from "@/game/settlement/settlementTypes";

type BuilderTool = "paint" | "place" | "erase";

type Props = {
  state: SettlementState;
  extraUnlockedObjectTypes?: SettlementObjectType[];
  unlockedThemes?: SettlementTheme[];
  onPaint: (x: number, y: number, terrain: SettlementTerrain) => void;
  onPlace: (x: number, y: number, type: SettlementObjectType) => void;
  onRemove: (x: number, y: number) => void;
  onThemeChange?: (theme: SettlementTheme) => void;
  onUpgrade?: () => void;
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

const upgradeHint: Record<SettlementState["level"], string> = {
  1: "정원과 기본 거주 공간을 갖춘 초기 정착 단계입니다.",
  2: "정화 시설과 생산 구역이 열리는 중간 정착 단계입니다.",
  3: "비콘과 감시 구조물까지 갖춘 고도 정착 단계입니다.",
};

export function SettlementBuilderPanel({
  state,
  extraUnlockedObjectTypes = [],
  unlockedThemes = [],
  onPaint,
  onPlace,
  onRemove,
  onThemeChange,
  onUpgrade,
}: Props) {
  const [tool, setTool] = useState<BuilderTool>("paint");
  const [selectedTerrain, setSelectedTerrain] = useState<SettlementTerrain>("garden");
  const [selectedObject, setSelectedObject] = useState<SettlementObjectType>("home-core");

  const objectMap = useMemo(() => {
    const map = new Map<string, { emoji: string; label: string }>();
    settlementObjectPalette.forEach((entry) => map.set(entry.type, { emoji: entry.emoji, label: entry.label }));
    return map;
  }, []);

  const visibleObjects = settlementObjectPalette.filter((entry) =>
    Array.from(new Set([...state.unlockedObjectTypes, ...extraUnlockedObjectTypes])).includes(entry.type),
  );

  const visibleThemes = settlementThemePalette.filter(
    (entry) => ["recovery-farm", "dawn-square", "star-hub"].includes(entry.theme) || unlockedThemes.includes(entry.theme),
  );

  const handleCellClick = (x: number, y: number) => {
    if (tool === "paint") onPaint(x, y, selectedTerrain);
    if (tool === "place") onPlace(x, y, selectedObject);
    if (tool === "erase") onRemove(x, y);
  };

  return (
    <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2 font-medium">
          <Home className="h-4 w-4 text-primary" />
          정착지 확장
        </div>
        <div className="rounded-full border border-amber-300/20 bg-amber-500/10 px-2 py-1 text-[11px] text-amber-200">Lv.{state.level}</div>
      </div>

      <p className="mt-2 text-xs text-slate-400">
        FitCraft 계열 2D 건설 흐름을 Fifth Dawn 안에서 거주지와 정착지 확장 모드로 계속 통합하고 있습니다.
      </p>

      <div className="mt-3 rounded-xl border border-white/10 bg-white/5 p-3 text-xs text-slate-300">
        <div className="font-medium text-white">{state.title}</div>
        <div className="mt-1">{upgradeHint[state.level]}</div>
        <div className="mt-2 text-[11px] text-sky-200">현재 테마: {visibleThemes.find((entry) => entry.theme === state.theme)?.label || state.theme}</div>
        <div className="mt-2 text-[11px] text-slate-400">복구 구조물 {state.restoredStructures.length}개 · 방문 {state.visits} · 좋아요 {state.likes}</div>
      </div>

      {onThemeChange ? (
        <div className="mt-4 grid grid-cols-2 gap-2">
          {visibleThemes.map((entry) => (
            <button
              key={entry.theme}
              type="button"
              onClick={() => onThemeChange(entry.theme)}
              className={`rounded-xl border px-3 py-2 text-left ${
                state.theme === entry.theme ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"
              }`}
            >
              {entry.label}
            </button>
          ))}
        </div>
      ) : null}

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
            제거
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
              className={`rounded-xl border px-3 py-2 text-left ${
                selectedTerrain === entry.terrain ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"
              }`}
            >
              {entry.label}
            </button>
          ))}
        </div>
      ) : null}

      {tool === "place" ? (
        <div className="mt-4 grid grid-cols-2 gap-2">
          {visibleObjects.map((entry) => (
            <button
              key={entry.type}
              type="button"
              onClick={() => setSelectedObject(entry.type)}
              className={`rounded-xl border px-3 py-2 text-left ${
                selectedObject === entry.type ? "border-primary bg-primary/15" : "border-white/10 bg-white/5"
              }`}
            >
              <span className="mr-2">{entry.emoji}</span>
              {entry.label}
            </button>
          ))}
        </div>
      ) : null}

      {onUpgrade ? (
        <button
          type="button"
          onClick={onUpgrade}
          disabled={state.level >= 3}
          className="mt-4 w-full rounded-xl border border-amber-300/20 bg-amber-500/15 px-3 py-3 text-sm disabled:cursor-not-allowed disabled:opacity-50"
        >
          <span className="inline-flex items-center gap-2">
            <Sparkles className="h-4 w-4" />
            {state.level >= 3 ? "최종 정착 단계" : `정착지 업그레이드 (다음 Lv.${state.level + 1})`}
          </span>
        </button>
      ) : null}

      <div
        className="mt-4 grid gap-1 rounded-xl border border-white/10 bg-slate-950/40 p-2"
        style={{ gridTemplateColumns: `repeat(${state.width}, minmax(0, 1fr))` }}
      >
        {state.tiles.map((tile) => {
          const object = state.objects.find((entry) => entry.x === tile.x && entry.y === tile.y);
          return (
            <button
              key={`${tile.x}-${tile.y}`}
              type="button"
              onClick={() => handleCellClick(tile.x, tile.y)}
              className={`relative aspect-square rounded-md border border-black/10 text-[10px] ${terrainColor(tile.terrain)}`}
            >
              {object ? objectMap.get(object.type)?.emoji || "" : ""}
            </button>
          );
        })}
      </div>
    </div>
  );
}
