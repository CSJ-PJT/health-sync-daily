import { useEffect, useMemo, useRef, useState } from "react";
import {
  BedDouble,
  Droplets,
  Hammer,
  Pickaxe,
  RefreshCw,
  Save,
  Soup,
  Sprout,
  Wheat,
  Wrench,
} from "lucide-react";
import { renderLifeSimScene } from "@/game/life-sim/engine/renderLifeSimScene";
import { resolveInputAction } from "@/game/life-sim/engine/inputBindings";
import { lifeSimItems } from "@/game/life-sim/data/items";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import { lifeSimNpcs } from "@/game/life-sim/data/npcs";
import { lifeSimRecipes } from "@/game/life-sim/data/recipes";
import {
  advanceClock,
  craftRecipe,
  cycleHazards,
  getQuestLabel,
  interactInWorld,
  movePlayer,
  selectHotbarIndex,
  sleepUntilNextDay,
  useSelectedHotbarItem,
  useToolAction,
} from "@/game/life-sim/state/lifeSimState";
import type { LifeSimFacing, LifeSimRecipeId, LifeSimState } from "@/game/life-sim/types";
import { loadLifeSimState, saveLifeSimState } from "@/services/repositories/lifeSimSaveRepository";

function formatClock(minutes: number) {
  const hour = Math.floor(minutes / 60);
  const minute = minutes % 60;
  return `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
}

function getItemLabel(itemId: keyof typeof lifeSimItems) {
  return t(lifeSimItems[itemId].name, getLifeSimLocale());
}

function getRelationshipLabel(level: number, locale = getLifeSimLocale()) {
  if (locale === "ko") {
    if (level >= 3) return "깊은 신뢰";
    if (level >= 2) return "친밀";
    if (level >= 1) return "안면 있음";
    return "낯선 사이";
  }
  if (level >= 3) return "Deep Trust";
  if (level >= 2) return "Close";
  if (level >= 1) return "Acquainted";
  return "Strangers";
}

function getItemIcon(itemId: keyof typeof lifeSimItems) {
  switch (itemId) {
    case "hoe":
      return <Hammer className="h-4 w-4" />;
    case "watering-can":
      return <Droplets className="h-4 w-4" />;
    case "pickaxe":
      return <Pickaxe className="h-4 w-4" />;
    case "turnip-seeds":
      return <Sprout className="h-4 w-4" />;
    case "turnip":
      return <Wheat className="h-4 w-4" />;
    case "dawn-broth":
      return <Soup className="h-4 w-4" />;
    default:
      return <Wrench className="h-4 w-4" />;
  }
}

type Props = {
  onExit?: () => void;
};

export function LifeSimArena({ onExit }: Props) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [state, setState] = useState<LifeSimState | null>(null);
  const [status, setStatus] = useState("세이브 데이터를 불러오는 중입니다.");
  const [saving, setSaving] = useState(false);
  const [selectedRecipe, setSelectedRecipe] = useState<LifeSimRecipeId>("dawn-broth");

  useEffect(() => {
    let cancelled = false;
    async function load() {
      const next = await loadLifeSimState("main");
      if (cancelled) return;
      setState(next);
      setStatus("복구 농장에 도착했습니다. 밭을 일구고 마을과 광산을 탐험해 보세요.");
    }
    void load();
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    if (!state || !canvasRef.current) return;
    renderLifeSimScene(canvasRef.current, state);
  }, [state]);

  useEffect(() => {
    if (!state) return;
    const timer = window.setInterval(() => {
      setState((current) => (current ? cycleHazards(advanceClock(current, 10)) : current));
    }, 2500);
    return () => window.clearInterval(timer);
  }, [state?.time.day]);

  useEffect(() => {
    if (!state) return;
    const handleKeyDown = (event: KeyboardEvent) => {
      const action = resolveInputAction(event.key, state.settings);
      if (!action) return;
      event.preventDefault();

      setState((current) => {
        if (!current) return current;
        switch (action) {
          case "move-up":
            return movePlayer(current, "up");
          case "move-down":
            return movePlayer(current, "down");
          case "move-left":
            return movePlayer(current, "left");
          case "move-right":
            return movePlayer(current, "right");
          case "interact":
            return interactInWorld(current).state;
          case "use-tool":
            return useToolAction(current).state;
          case "sleep":
            return sleepUntilNextDay(current);
          case "hotbar-1":
            return selectHotbarIndex(current, 0);
          case "hotbar-2":
            return selectHotbarIndex(current, 1);
          case "hotbar-3":
            return selectHotbarIndex(current, 2);
          case "hotbar-4":
            return selectHotbarIndex(current, 3);
          case "hotbar-5":
            return selectHotbarIndex(current, 4);
          default:
            return current;
        }
      });
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [state]);

  useEffect(() => {
    if (!state) return;
    const timer = window.setTimeout(() => {
      void saveLifeSimState(state, state.slot);
    }, 700);
    return () => window.clearTimeout(timer);
  }, [state]);

  const selectedItem = useMemo(() => (state ? useSelectedHotbarItem(state) : "hoe"), [state]);

  const saveNow = async () => {
    if (!state) return;
    setSaving(true);
    await saveLifeSimState(state, state.slot);
    setSaving(false);
    setStatus("농장, 마을, 광산의 현재 상태를 저장했습니다.");
  };

  const applyMovement = (facing: LifeSimFacing) => {
    setState((current) => (current ? movePlayer(current, facing) : current));
  };

  const applyTool = () => {
    setState((current) => {
      if (!current) return current;
      const result = useToolAction(current);
      if (result.message) setStatus(`${result.message.title}: ${result.message.body}`);
      return result.state;
    });
  };

  const applyInteract = () => {
    setState((current) => {
      if (!current) return current;
      const result = interactInWorld(current);
      if (result.message) setStatus(`${result.message.title}: ${result.message.body}`);
      return result.state;
    });
  };

  const applySleep = () => {
    setState((current) => (current ? sleepUntilNextDay(current) : current));
    setStatus("하루를 마무리하고 다음 날 새벽으로 넘어갑니다.");
  };

  const applyCraft = () => {
    setState((current) => {
      if (!current) return current;
      const result = craftRecipe(current, selectedRecipe);
      if (result.message) setStatus(`${result.message.title}: ${result.message.body}`);
      return result.state;
    });
  };

  if (!state) {
    return <div className="flex h-full items-center justify-center text-sm text-white/80">{status}</div>;
  }

  return (
    <div className="grid h-full min-h-[720px] gap-4 xl:grid-cols-[320px_minmax(0,1fr)_360px]">
      <section className="min-h-0 space-y-4 overflow-y-auto rounded-[1.6rem] border border-white/10 bg-white/5 p-4">
        <div>
          <div className="text-xs uppercase tracking-[0.25em] text-emerald-200/70">Player State</div>
          <h2 className="mt-2 text-xl font-semibold">현재 상태</h2>
        </div>

        <div className="space-y-2 text-sm text-slate-200">
          <div>맵: {state.player.mapId === "farm" ? "복구 농장" : state.player.mapId === "village" ? "새벽 광장" : "정화 광산"}</div>
          <div>시간: {state.time.day}일차 · {formatClock(state.time.minutes)}</div>
          <div>기력: {state.player.energy} / {state.player.maxEnergy}</div>
          <div>연결 보너스: 시작 {state.healthBonuses.startEnergyBonus} · 회복 {state.healthBonuses.recoveryBonus} · 작물 {state.healthBonuses.cropEfficiencyBonus}</div>
        </div>

        <div>
          <div className="mb-2 text-sm font-medium">핫바</div>
          <div className="grid grid-cols-5 gap-2">
            {state.player.hotbar.map((itemId, index) => (
              <button
                key={`${itemId}-${index}`}
                type="button"
                onClick={() => setState((current) => (current ? selectHotbarIndex(current, index) : current))}
                className={`rounded-2xl border p-2 text-center text-xs transition ${
                  state.player.selectedHotbarIndex === index
                    ? "border-emerald-300 bg-emerald-400/20"
                    : "border-white/10 bg-black/20 hover:bg-black/30"
                }`}
              >
                <div className="mb-1 flex justify-center">{getItemIcon(itemId)}</div>
                <div>{getItemLabel(itemId)}</div>
                <div className="text-[10px] text-white/65">{state.player.inventory[itemId] || 0}</div>
              </button>
            ))}
          </div>
        </div>

        <div>
          <div className="mb-2 text-sm font-medium">인벤토리</div>
          <div className="space-y-2 text-sm">
            {Object.entries(state.player.inventory).map(([itemId, amount]) => (
              <div key={itemId} className="flex items-center justify-between rounded-2xl border border-white/10 bg-black/20 px-3 py-2">
                <span>{getItemLabel(itemId as keyof typeof lifeSimItems)}</span>
                <span>{amount}</span>
              </div>
            ))}
          </div>
        </div>

        <div>
          <div className="mb-2 text-sm font-medium">제작대</div>
          <div className="space-y-2">
            {(Object.keys(lifeSimRecipes) as LifeSimRecipeId[]).map((recipeId) => {
              const recipe = lifeSimRecipes[recipeId];
              return (
                <button
                  key={recipeId}
                  type="button"
                  onClick={() => setSelectedRecipe(recipeId)}
                  className={`w-full rounded-2xl border px-3 py-3 text-left text-sm ${
                    selectedRecipe === recipeId ? "border-sky-300 bg-sky-500/15" : "border-white/10 bg-black/20"
                  }`}
                >
                  <div className="font-medium">{t(recipe.title)}</div>
                  <div className="mt-1 text-xs text-white/70">{t(recipe.description)}</div>
                </button>
              );
            })}
            <button type="button" onClick={applyCraft} className="w-full rounded-2xl border border-amber-300/20 bg-amber-500/15 px-4 py-3 text-sm">
              선택한 레시피 제작
            </button>
          </div>
        </div>
      </section>

      <section className="flex min-h-0 flex-col gap-4 rounded-[1.6rem] border border-white/10 bg-black/30 p-4">
        <div className="min-h-0 flex-1 overflow-auto rounded-[1.4rem] border border-white/10 bg-black/30 p-2">
          <canvas ref={canvasRef} className="mx-auto max-w-full rounded-xl bg-black/20" />
        </div>
        <div className="grid gap-3 lg:grid-cols-[1fr_auto]">
          <div className="grid grid-cols-3 gap-2">
            <div />
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("up")}>위</button>
            <div />
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("left")}>왼쪽</button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("down")}>아래</button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("right")}>오른쪽</button>
          </div>
          <div className="grid grid-cols-2 gap-2">
            <button type="button" className="rounded-2xl border border-white/10 bg-emerald-500/15 px-4 py-3 text-sm" onClick={applyTool}>
              <span className="inline-flex items-center gap-2"><Hammer className="h-4 w-4" />행동</span>
            </button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={applyInteract}>대화/확인</button>
            <button type="button" className="rounded-2xl border border-white/10 bg-indigo-500/15 px-4 py-3 text-sm" onClick={applySleep}>
              <span className="inline-flex items-center gap-2"><BedDouble className="h-4 w-4" />수면</span>
            </button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={saveNow} disabled={saving}>
              <span className="inline-flex items-center gap-2"><Save className="h-4 w-4" />{saving ? "저장 중" : "저장"}</span>
            </button>
          </div>
        </div>
      </section>

      <section className="min-h-0 space-y-4 overflow-y-auto rounded-[1.6rem] border border-white/10 bg-white/5 p-4">
        <div>
          <div className="text-xs uppercase tracking-[0.25em] text-sky-200/70">Field Guide</div>
          <h2 className="mt-2 text-xl font-semibold">정착 가이드</h2>
          <p className="mt-2 text-sm text-slate-300">
            밭을 갈고, 씨앗을 심고, 광산 자원을 모아 제작을 확장하세요. 마을 사람들과 대화하면 숨은 기록과 정화 계획이 조금씩 드러납니다.
          </p>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">현재 안내</div>
          <div className="mt-2 text-slate-300">{status}</div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">현재 선택한 도구</div>
          <div className="mt-2">{getItemLabel(selectedItem)}</div>
          <div className="mt-1 text-xs text-slate-400">{t(lifeSimItems[selectedItem].description)}</div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">퀘스트</div>
          <div className="mt-3 space-y-3">
            {state.quests.map((quest) => {
              const definition = getQuestLabel(quest.id);
              return (
                <div key={quest.id} className="rounded-xl border border-white/10 px-3 py-3">
                  <div className="font-medium">{t(definition.title)}</div>
                  <div className="mt-1 text-xs text-white/70">{t(definition.description)}</div>
                  <div className="mt-2 text-xs text-emerald-300">
                    {quest.status === "completed" ? `완료 · 보상 ${t(definition.rewardText)}` : `진행 중 · 보상 ${t(definition.rewardText)}`}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">관계도</div>
          <div className="mt-3 space-y-3">
            {lifeSimNpcs.map((npc) => {
              const relation = state.relationships[npc.id];
              const schedule = state.time.minutes < 14 * 60 ? npc.scheduleHint.morning : npc.scheduleHint.evening;
              return (
                <div key={npc.id} className="rounded-xl border border-white/10 px-3 py-3">
                  <div className="font-medium">{t(npc.name)}</div>
                  <div className="mt-1 text-xs text-white/70">{getRelationshipLabel(relation.level)}</div>
                  <div className="mt-1 text-xs text-white/60">우정 {relation.friendship}</div>
                  <div className="mt-2 text-xs text-sky-200">{t(schedule)}</div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">조작 안내</div>
          <ul className="mt-2 space-y-2 text-slate-300">
            <li>이동: WASD 또는 방향 버튼</li>
            <li>행동: Space</li>
            <li>대화/확인: E</li>
            <li>수면: N</li>
            <li>핫바: 1~5</li>
          </ul>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">연결 보너스</div>
          <p className="mt-2 text-slate-300">
            Health App과 연결하면 원본 건강 데이터가 아니라 파생된 게임 보너스만 받아 시작 기력, 회복력, 작물 효율이 조금 높아집니다.
            연결하지 않아도 전체 플레이는 그대로 가능합니다.
          </p>
          {onExit ? (
            <button type="button" className="mt-3 rounded-2xl border border-white/10 bg-white/5 px-4 py-2 text-sm" onClick={onExit}>
              <span className="inline-flex items-center gap-2"><RefreshCw className="h-4 w-4" />게임 선택으로 돌아가기</span>
            </button>
          ) : null}
        </div>
      </section>
    </div>
  );
}
