import { useEffect, useMemo, useRef, useState } from "react";
import {
  BedDouble,
  Droplets,
  Hammer,
  Pickaxe,
  RefreshCw,
  Save,
  Soup,
  Sparkles,
  Sprout,
  Wheat,
  Wrench,
} from "lucide-react";
import { LifeSimSettingsPanel } from "@/components/life-sim/LifeSimSettingsPanel";
import { SettlementBuilderPanel } from "@/components/life-sim/SettlementBuilderPanel";
import { LifeSimTouchControls } from "@/components/life-sim/LifeSimTouchControls";
import { renderLifeSimScene } from "@/game/life-sim/engine/renderLifeSimScene";
import { resolveInputAction } from "@/game/life-sim/engine/inputBindings";
import { lifeSimItems } from "@/game/life-sim/data/items";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import { lifeSimNpcs } from "@/game/life-sim/data/npcs";
import { lifeSimRecipes } from "@/game/life-sim/data/recipes";
import { getScheduledNpcsForMap, getNpcScheduleStop } from "@/game/life-sim/state/npcSchedule";
import { getActiveQuestSummary } from "@/game/life-sim/state/questRouting";
import {
  getLifeSimPlayerSlice,
  getLifeSimQuestSlice,
  getLifeSimRelationshipSlice,
  getLifeSimSettlementSlice,
  getLifeSimWorldSlice,
} from "@/game/life-sim/state/lifeSimSlices";
import { getProgressionHint, getProgressionOverview } from "@/game/life-sim/state/progressionOverview";
import { applyInputPreset, rebindInputAction } from "@/game/life-sim/state/settings";
import {
  advanceClock,
  createInitialLifeSimState,
  craftRecipe,
  cycleHazards,
  getQuestLabel,
  interactInWorld,
  isRecipeUnlocked,
  movePlayer,
  selectHotbarIndex,
  sleepUntilNextDay,
  useSelectedHotbarItem,
  useToolAction,
} from "@/game/life-sim/state/lifeSimState";
import {
  getSettlementFacilities,
  getSettlementTierLabel,
  getSettlementUnlockedHighlights,
  getSettlementUpgradeCost,
  getNextSettlementGoal,
} from "@/game/life-sim/state/settlementProgress";
import { paintSettlementTile, placeSettlementObject, removeSettlementObject, upgradeSettlement } from "@/game/settlement/settlementState";
import type { LifeSimFacing, LifeSimInputAction, LifeSimRecipeId, LifeSimState } from "@/game/life-sim/types";
import { loadLifeSimState, saveLifeSimState } from "@/services/repositories/lifeSimSaveRepository";
import { loadLifeSimSettings, saveLifeSimSettings } from "@/services/repositories/lifeSimSettingsRepository";

type Props = {
  onExit?: () => void;
};

function formatClock(minutes: number) {
  const hour = Math.floor(minutes / 60);
  const minute = minutes % 60;
  return `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
}

function getItemLabel(itemId: keyof typeof lifeSimItems) {
  return t(lifeSimItems[itemId].name, getLifeSimLocale());
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

function getMapLabel(mapId: LifeSimState["player"]["mapId"]) {
  switch (mapId) {
    case "farm":
      return "복구 농장";
    case "village":
      return "여명 광장";
    case "mine":
      return "정화 광산";
    case "north-pass":
      return "북부 개척지";
  }
}

function getRelationshipLabel(level: number) {
  if (level >= 3) return "깊은 신뢰";
  if (level >= 2) return "가까운 사이";
  if (level >= 1) return "호감 단계";
  return "첫 만남";
}

function getNextRelationshipReward(level: number) {
  if (level < 1) return "1단계 보상: 새벽 순무 씨앗 2개";
  if (level < 2) return "2단계 보상: NPC 전용 선물 + 공명 3";
  if (level < 3) return "3단계 보상: 공명 포인트 8";
  return "관계도 보상을 모두 획득했습니다.";
}

export function LifeSimArena({ onExit }: Props) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [state, setState] = useState<LifeSimState | null>(null);
  const [status, setStatus] = useState("게임 데이터를 불러오는 중입니다.");
  const [saving, setSaving] = useState(false);
  const [selectedRecipe, setSelectedRecipe] = useState<LifeSimRecipeId>("dawn-broth");
  const [showSettings, setShowSettings] = useState(false);
  const [pendingRebind, setPendingRebind] = useState<LifeSimInputAction | null>(null);
  const [bootError, setBootError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const next = await loadLifeSimState("main");
        if (cancelled) return;
        const settings = loadLifeSimSettings();
        setState({ ...next, settings });
        setBootError(null);
        setStatus("복구 농장이 깨어났습니다. 밭을 가꾸고 광산 자원을 모아 정착지와 북부 개척지를 이어 보세요.");
      } catch {
        if (cancelled) return;
        const settings = loadLifeSimSettings();
        const fallback = createInitialLifeSimState({
          startEnergyBonus: 0,
          recoveryBonus: 0,
          cropEfficiencyBonus: 0,
        });
        setState({ ...fallback, settings });
        setBootError("저장소 또는 네트워크 문제로 기본 상태로 시작했습니다.");
        setStatus("기본 농장 상태로 시작했습니다. 링크 없이도 그대로 플레이할 수 있습니다.");
      }
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
      if (pendingRebind) {
        event.preventDefault();
        if (event.key === "Escape") {
          setPendingRebind(null);
          setStatus("키 변경을 취소했습니다.");
          return;
        }
        setState((current) =>
          current
            ? {
                ...current,
                settings: rebindInputAction(current.settings, pendingRebind, event.key),
              }
            : current,
        );
        setStatus(`입력 키를 ${event.key}로 변경했습니다.`);
        setPendingRebind(null);
        return;
      }

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
  }, [pendingRebind, state]);

  useEffect(() => {
    if (!state) return;
    saveLifeSimSettings(state.settings);
    const timer = window.setTimeout(() => {
      void saveLifeSimState(state, state.slot);
    }, 700);
    return () => window.clearTimeout(timer);
  }, [state]);

  const selectedItem = useMemo(() => (state ? useSelectedHotbarItem(state) : "hoe"), [state]);
  const activeQuest = useMemo(() => (state ? getActiveQuestSummary(state) : null), [state]);
  const currentMapNpcs = useMemo(() => (state ? getScheduledNpcsForMap(state, state.player.mapId) : []), [state]);
  const playerSlice = useMemo(() => (state ? getLifeSimPlayerSlice(state) : null), [state]);
  const worldSlice = useMemo(() => (state ? getLifeSimWorldSlice(state) : null), [state]);
  const questSlice = useMemo(() => (state ? getLifeSimQuestSlice(state) : null), [state]);
  const relationshipSlice = useMemo(() => (state ? getLifeSimRelationshipSlice(state) : null), [state]);
  const settlementSlice = useMemo(() => (state ? getLifeSimSettlementSlice(state) : null), [state]);

  const saveNow = async () => {
    if (!state) return;
    setSaving(true);
    await saveLifeSimState(state, state.slot);
    setSaving(false);
    setStatus("농장, 마을, 광산, 정착지 현재 상태를 저장했습니다.");
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
    setStatus("하루를 마무리하고 다음 날로 넘어갑니다.");
  };

  const applyCraft = () => {
    setState((current) => {
      if (!current) return current;
      const result = craftRecipe(current, selectedRecipe);
      if (result.message) setStatus(`${result.message.title}: ${result.message.body}`);
      return result.state;
    });
  };

  const applyPreset = (preset: "wasd" | "arrows") => {
    setState((current) =>
      current
        ? {
            ...current,
            settings: applyInputPreset(current.settings, preset),
          }
        : current,
    );
    setStatus(preset === "wasd" ? "WASD 입력 프리셋을 적용했습니다." : "방향키 입력 프리셋을 적용했습니다.");
  };

  const applySettlementUpgrade = () => {
    setState((current) => {
      if (!current) return current;
      const cost = getSettlementUpgradeCost(current.settlement.level);
      if (current.settlement.level >= 3) {
        setStatus("정착지가 이미 최종 단계입니다.");
        return current;
      }
      if (current.progression.resonancePoints < cost) {
        setStatus(`정착지 업그레이드에는 공명 포인트 ${cost}가 필요합니다.`);
        return current;
      }

      const nextSettlement = upgradeSettlement(current.settlement);
      setStatus(`정착지를 Lv.${nextSettlement.level}로 확장했습니다.`);
      return {
        ...current,
        settlement: nextSettlement,
        progression: {
          ...current.progression,
          resonancePoints: current.progression.resonancePoints - cost,
        },
      };
    });
  };

  if (!state) {
    return (
      <div className="flex h-full min-h-[60vh] items-center justify-center">
        <div className="w-full max-w-xl rounded-[2rem] border border-white/10 bg-black/30 p-8 text-center text-white/90">
          <div className="text-xs uppercase tracking-[0.35em] text-emerald-200/70">Fifth Dawn Boot</div>
          <h2 className="mt-3 text-2xl font-semibold">라이프심 월드를 준비하는 중입니다</h2>
          <p className="mt-3 text-sm text-slate-300">{status}</p>
          <div className="mt-6 text-xs text-slate-500">오프라인 상태여도 잠시 후 기본 농장으로 진입합니다.</div>
        </div>
      </div>
    );
  }

  const currentSettlement = settlementSlice || state.settlement;
  const currentRelations = relationshipSlice?.relationships || state.relationships;
  const currentProgression = questSlice?.progression || state.progression;
  const progressionOverview = getProgressionOverview(currentProgression, worldSlice?.storyFlags || state.storyFlags);

  return (
    <div className="grid h-full min-h-[720px] gap-4 xl:grid-cols-[320px_minmax(0,1fr)_360px]">
      <section className="min-h-0 space-y-4 overflow-y-auto rounded-[1.6rem] border border-white/10 bg-white/5 p-4">
        <div>
          <div className="text-xs uppercase tracking-[0.25em] text-emerald-200/70">Player State</div>
          <h2 className="mt-2 text-xl font-semibold">현재 상태</h2>
        </div>

        <div className="space-y-2 text-sm text-slate-200">
          <div>맵: {getMapLabel(playerSlice?.mapId || state.player.mapId)}</div>
          <div>시간: {state.time.day}일차 {formatClock(state.time.minutes)}</div>
          <div>기력: {playerSlice?.energy || state.player.energy} / {playerSlice?.maxEnergy || state.player.maxEnergy}</div>
          <div>
            건강 보너스: 시작 {state.healthBonuses.startEnergyBonus} / 회복 {state.healthBonuses.recoveryBonus} / 재배 효율{" "}
            {state.healthBonuses.cropEfficiencyBonus}
          </div>
          <div className="inline-flex items-center gap-2 text-amber-200">
            <Sparkles className="h-4 w-4" />
            공명 포인트 {state.progression.resonancePoints}
          </div>
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
          <div className="mb-2 text-sm font-medium">제작 목록</div>
          <div className="space-y-2">
            {(Object.keys(lifeSimRecipes) as LifeSimRecipeId[]).map((recipeId) => {
              const recipe = lifeSimRecipes[recipeId];
              const unlocked = isRecipeUnlocked(state, recipeId);
              return (
                <button
                  key={recipeId}
                  type="button"
                  onClick={() => unlocked && setSelectedRecipe(recipeId)}
                  className={`w-full rounded-2xl border px-3 py-3 text-left text-sm ${
                    selectedRecipe === recipeId && unlocked ? "border-sky-300 bg-sky-500/15" : "border-white/10 bg-black/20"
                  } ${unlocked ? "" : "opacity-60"}`}
                >
                  <div className="font-medium">{t(recipe.title)}</div>
                  <div className="mt-1 text-xs text-white/70">{t(recipe.description)}</div>
                  {!unlocked ? <div className="mt-2 text-[11px] text-amber-300">관련 퀘스트 완료 후 해금</div> : null}
                </button>
              );
            })}
            <button type="button" onClick={applyCraft} className="w-full rounded-2xl border border-amber-300/20 bg-amber-500/15 px-4 py-3 text-sm">
              선택한 레시피 제작
            </button>
          </div>
        </div>

        <LifeSimSettingsPanel
          open={showSettings}
          settings={state.settings}
          pendingRebind={pendingRebind}
          onToggle={() => setShowSettings((current) => !current)}
          onChange={(settings) => setState((current) => (current ? { ...current, settings } : current))}
          onStartRebind={(action) => {
            setPendingRebind(action);
            setStatus(`"${action}" 동작에 연결할 키를 눌러 주세요. 취소하려면 Esc를 누르세요.`);
          }}
        />

        {showSettings ? (
          <div className="grid grid-cols-2 gap-2 text-sm">
            <button type="button" onClick={() => applyPreset("wasd")} className="rounded-2xl border border-white/10 bg-black/20 px-3 py-2">
              WASD 적용
            </button>
            <button type="button" onClick={() => applyPreset("arrows")} className="rounded-2xl border border-white/10 bg-black/20 px-3 py-2">
              방향키 적용
            </button>
          </div>
        ) : null}
      </section>

      <section className="flex min-h-0 flex-col gap-4 rounded-[1.6rem] border border-white/10 bg-black/30 p-4">
        <div className="min-h-0 flex-1 overflow-auto rounded-[1.4rem] border border-white/10 bg-black/30 p-2">
          <canvas ref={canvasRef} className="mx-auto max-w-full rounded-xl bg-black/20" />
        </div>
        <div className="grid gap-3 lg:grid-cols-[1fr_auto]">
          <div className="grid grid-cols-3 gap-2">
            <div />
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("up")}>
              위
            </button>
            <div />
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("left")}>
              왼쪽
            </button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("down")}>
              아래
            </button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => applyMovement("right")}>
              오른쪽
            </button>
          </div>
          <div className="grid grid-cols-2 gap-2">
            <button type="button" className="rounded-2xl border border-white/10 bg-emerald-500/15 px-4 py-3 text-sm" onClick={applyTool}>
              <span className="inline-flex items-center gap-2">
                <Hammer className="h-4 w-4" />
                행동
              </span>
            </button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={applyInteract}>
              대화 / 확인
            </button>
            <button type="button" className="rounded-2xl border border-white/10 bg-indigo-500/15 px-4 py-3 text-sm" onClick={applySleep}>
              <span className="inline-flex items-center gap-2">
                <BedDouble className="h-4 w-4" />
                수면
              </span>
            </button>
            <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={saveNow} disabled={saving}>
              <span className="inline-flex items-center gap-2">
                <Save className="h-4 w-4" />
                {saving ? "저장 중..." : "저장"}
              </span>
            </button>
          </div>
        </div>
        <LifeSimTouchControls onMove={applyMovement} onTool={applyTool} onInteract={applyInteract} onSleep={applySleep} />
      </section>

      <section className="min-h-0 space-y-4 overflow-y-auto rounded-[1.6rem] border border-white/10 bg-white/5 p-4">
        <div>
          <div className="text-xs uppercase tracking-[0.25em] text-sky-200/70">Field Guide</div>
          <h2 className="mt-2 text-xl font-semibold">현장 가이드</h2>
          <p className="mt-2 text-sm text-slate-300">
            밭을 가꾸고 광산 자원을 모아 시설을 복구하세요. 마을 주민과 대화하면 깊은 기록과 정화 계획이 조금씩 드러납니다.
          </p>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">현재 안내</div>
          <div className="mt-2 text-slate-300">{status}</div>
          {bootError ? <div className="mt-2 text-xs text-amber-300">{bootError}</div> : null}
        </div>

        {activeQuest ? (
          <div className="rounded-2xl border border-amber-300/20 bg-amber-500/10 p-4 text-sm">
            <div className="font-medium">활성 퀘스트</div>
            <div className="mt-2 font-semibold text-amber-100">{activeQuest.title}</div>
            <div className="mt-1 text-slate-300">{activeQuest.objective}</div>
          </div>
        ) : null}

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">현재 선택한 도구</div>
          <div className="mt-2">{getItemLabel(selectedItem)}</div>
          <div className="mt-1 text-xs text-slate-400">{t(lifeSimItems[selectedItem].description)}</div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">퀘스트 목록</div>
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
          <div className="font-medium">현재 맵 주민</div>
          <div className="mt-3 space-y-3">
            {currentMapNpcs.map(({ npc, stop }) => {
              const relation = currentRelations[npc.id];
              return (
                <div key={npc.id} className="rounded-xl border border-white/10 px-3 py-3">
                  <div className="font-medium">{t(npc.name)}</div>
                  <div className="mt-1 text-xs text-white/70">{getRelationshipLabel(relation.level)}</div>
                  <div className="mt-1 text-xs text-white/60">친밀도 {relation.friendship}</div>
                  <div className="mt-1 text-[11px] text-amber-200">{getNextRelationshipReward(relation.level)}</div>
                  <div className="mt-2 text-xs text-sky-200">{t(stop.hint)}</div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">전체 일정</div>
          <div className="mt-3 space-y-3">
            {lifeSimNpcs.map((npc) => {
              const stop = getNpcScheduleStop(state, npc.id);
              const relation = currentRelations[npc.id];
              return (
                <div key={npc.id} className="rounded-xl border border-white/10 px-3 py-3">
                  <div className="font-medium">{t(npc.name)}</div>
                  <div className="mt-1 text-xs text-white/70">{getRelationshipLabel(relation.level)}</div>
                  <div className="mt-2 text-xs text-sky-200">{t(stop.hint)}</div>
                  <div className="mt-1 text-[11px] text-slate-400">
                    {getMapLabel(stop.mapId)} · ({stop.x}, {stop.y})
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">진행도</div>
          <div className="mt-3 space-y-2 text-slate-300">
            <div>해금 레시피: {progressionOverview.unlockedRecipeCount}개</div>
            <div>발견 지역: {progressionOverview.discoveredMapCount}곳</div>
            <div>완료 퀘스트: {progressionOverview.completedQuestCount}개</div>
            <div>북쪽 다리: {progressionOverview.restoredBridge ? "복구 완료" : "복구 필요"}</div>
            <div>북부 조사: {progressionOverview.surveyedNorthReach ? "완료" : "미완료"}</div>
            <div className="pt-1 text-xs text-sky-200">{getProgressionHint(currentProgression, worldSlice?.storyFlags || state.storyFlags)}</div>
          </div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">정착지 진행</div>
          <div className="mt-2 text-slate-300">
            {getSettlementTierLabel(currentSettlement.level)} · Lv.{currentSettlement.level}
          </div>
          <div className="mt-2 text-xs text-white/70">현재 해금: {getSettlementUnlockedHighlights(currentSettlement).join(", ")}</div>
          <div className="mt-2 text-xs text-amber-200">
            다음 업그레이드 비용: {currentSettlement.level >= 3 ? "최종 단계" : `공명 ${getSettlementUpgradeCost(currentSettlement.level)}`}
          </div>
          <div className="mt-3 text-xs text-white/70">시설: {getSettlementFacilities(currentSettlement).join(", ")}</div>
          <div className="mt-2 text-xs text-sky-200">{getNextSettlementGoal(currentSettlement)}</div>
        </div>

        <SettlementBuilderPanel
          state={state.settlement}
          onPaint={(x, y, terrain) =>
            setState((current) =>
              current
                ? {
                    ...current,
                    settlement: paintSettlementTile(current.settlement, x, y, terrain),
                  }
                : current,
            )
          }
          onPlace={(x, y, type) =>
            setState((current) =>
              current
                ? {
                    ...current,
                    settlement: placeSettlementObject(current.settlement, x, y, type),
                  }
                : current,
            )
          }
          onRemove={(x, y) =>
            setState((current) =>
              current
                ? {
                    ...current,
                    settlement: removeSettlementObject(current.settlement, x, y),
                  }
                : current,
            )
          }
          onUpgrade={applySettlementUpgrade}
        />

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">조작 안내</div>
          <ul className="mt-2 space-y-2 text-slate-300">
            <li>이동: WASD 또는 방향키</li>
            <li>행동: Space</li>
            <li>대화 / 확인: E</li>
            <li>수면: Q</li>
            <li>핫바: 1~5</li>
          </ul>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">링크 보너스</div>
          <p className="mt-2 text-slate-300">
            Health App과 연결하면 원본 건강 데이터가 아니라 파생된 안전 보너스만 받아 시작 기력, 회복, 재배 효율이 조금 올라갑니다.
            연결하지 않아도 전체 플레이는 그대로 가능합니다.
          </p>
          {onExit ? (
            <button type="button" className="mt-3 rounded-2xl border border-white/10 bg-white/5 px-4 py-2 text-sm" onClick={onExit}>
              <span className="inline-flex items-center gap-2">
                <RefreshCw className="h-4 w-4" />
                게임 선택으로 돌아가기
              </span>
            </button>
          ) : null}
        </div>
      </section>
    </div>
  );
}
