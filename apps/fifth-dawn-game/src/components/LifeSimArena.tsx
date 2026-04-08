import { useEffect, useMemo, useRef, useState } from "react";
import { BedDouble, Droplets, Hammer, Pickaxe, RefreshCw, Save, Sprout, Wheat } from "lucide-react";
import { renderLifeSimScene } from "@/game/life-sim/engine/renderLifeSimScene";
import { resolveInputAction } from "@/game/life-sim/engine/inputBindings";
import { lifeSimItems } from "@/game/life-sim/data/items";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import {
  advanceClock,
  cycleHazards,
  interactInWorld,
  movePlayer,
  selectHotbarIndex,
  sleepUntilNextDay,
  useSelectedHotbarItem,
  useToolAction,
} from "@/game/life-sim/state/lifeSimState";
import type { LifeSimFacing, LifeSimState } from "@/game/life-sim/types";
import { loadLifeSimState, saveLifeSimState } from "@/services/repositories/lifeSimSaveRepository";

function formatClock(minutes: number) {
  const hour = Math.floor(minutes / 60);
  const minute = minutes % 60;
  return `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
}

function getItemLabel(itemId: keyof typeof lifeSimItems) {
  return t(lifeSimItems[itemId].name, getLifeSimLocale());
}

type Props = {
  onExit?: () => void;
};

export function LifeSimArena({ onExit }: Props) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [state, setState] = useState<LifeSimState | null>(null);
  const [status, setStatus] = useState("세이브 데이터를 불러오는 중입니다.");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      const next = await loadLifeSimState("main");
      if (cancelled) return;
      setState(next);
      setStatus("복구 농장에 도착했습니다. 밭을 일구고, 마을과 광산을 탐험해 보세요.");
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
    setStatus("현재 농장과 마을, 광산 상태를 저장했습니다.");
  };

  const applyMovement = (facing: LifeSimFacing) => {
    setState((current) => (current ? movePlayer(current, facing) : current));
  };

  const applyTool = () => {
    setState((current) => {
      if (!current) return current;
      const result = useToolAction(current);
      if (result.message) setStatus(`${result.message.title} · ${result.message.body}`);
      return result.state;
    });
  };

  const applyInteract = () => {
    setState((current) => {
      if (!current) return current;
      const result = interactInWorld(current);
      if (result.message) setStatus(`${result.message.title} · ${result.message.body}`);
      return result.state;
    });
  };

  const applySleep = () => {
    setState((current) => (current ? sleepUntilNextDay(current) : current));
    setStatus("하루를 마무리하고 다음 날 새벽으로 넘어갑니다.");
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
          <div>
            연결 보너스 · 시작 {state.healthBonuses.startEnergyBonus} / 회복 {state.healthBonuses.recoveryBonus} / 작물 효율{" "}
            {state.healthBonuses.cropEfficiencyBonus}
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
                <div className="mb-1 flex justify-center">
                  {itemId === "hoe" ? <Hammer className="h-4 w-4" /> : null}
                  {itemId === "watering-can" ? <Droplets className="h-4 w-4" /> : null}
                  {itemId === "pickaxe" ? <Pickaxe className="h-4 w-4" /> : null}
                  {itemId === "turnip-seeds" ? <Sprout className="h-4 w-4" /> : null}
                  {itemId === "turnip" ? <Wheat className="h-4 w-4" /> : null}
                </div>
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
          <h2 className="mt-2 text-xl font-semibold">새벽 작업대</h2>
          <p className="mt-2 text-sm text-slate-300">
            괭이로 흙을 갈고, 씨앗을 심고, 물을 준 뒤 잠을 자면 작물이 자랍니다. 마을의 기록관과 정비공에게 말을 걸면
            숨겨진 이야기와 조건부 대사를 볼 수 있습니다.
          </p>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">선택한 도구</div>
          <div className="mt-2">{getItemLabel(selectedItem)}</div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">현재 안내</div>
          <div className="mt-2 text-slate-300">{status}</div>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">조작 안내</div>
          <ul className="mt-2 space-y-2 text-slate-300">
            <li>이동: WASD 또는 방향키</li>
            <li>행동: Space</li>
            <li>대화/확인: E</li>
            <li>수면: N</li>
            <li>핫바: 1~5</li>
          </ul>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
          <div className="font-medium">연결 보너스</div>
          <p className="mt-2 text-slate-300">
            Health App과 연결되면 걸음, 수면, 회복, 수분 기록에서 계산된 파생 지표만 받아 시작 기력, 회복량, 작물 효율에
            작은 보너스를 줍니다. 연결하지 않아도 전체 게임 진행은 가능합니다.
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
