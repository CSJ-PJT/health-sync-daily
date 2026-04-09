import { useEffect, useRef, useState } from "react";
import { BedDouble, Hammer, Hand, MoveDown, MoveLeft, MoveRight, MoveUp } from "lucide-react";
import { renderLifeSimScene } from "@/game/life-sim/engine/renderLifeSimScene";
import {
  advanceClock,
  createInitialLifeSimState,
  cycleHazards,
  interactInWorld,
  movePlayer,
  sleepUntilNextDay,
  useToolAction,
} from "@/game/life-sim/state/lifeSimState";
import type { LifeSimFacing, LifeSimState } from "@/game/life-sim/types";

function formatClock(minutes: number) {
  const hour = Math.floor(minutes / 60);
  const minute = minutes % 60;
  return `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
}

export function EmergencyLifeSimFallback() {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [state, setState] = useState<LifeSimState>(() =>
    createInitialLifeSimState({
      startEnergyBonus: 0,
      recoveryBonus: 0,
      cropEfficiencyBonus: 0,
    }),
  );
  const [status, setStatus] = useState("고급 UI 복구 전까지 로컬 비상 모드로 플레이할 수 있습니다.");

  useEffect(() => {
    if (!canvasRef.current) return;
    renderLifeSimScene(canvasRef.current, state);
  }, [state]);

  useEffect(() => {
    const timer = window.setInterval(() => {
      setState((current) => cycleHazards(advanceClock(current, 10)));
    }, 2500);
    return () => window.clearInterval(timer);
  }, []);

  const move = (direction: LifeSimFacing) => {
    setState((current) => movePlayer(current, direction));
  };

  const useTool = () => {
    setState((current) => {
      const result = useToolAction(current);
      if (result.message) setStatus(`${result.message.title}: ${result.message.body}`);
      return result.state;
    });
  };

  const interact = () => {
    setState((current) => {
      const result = interactInWorld(current);
      if (result.message) setStatus(`${result.message.title}: ${result.message.body}`);
      return result.state;
    });
  };

  const sleep = () => {
    setState((current) => sleepUntilNextDay(current));
    setStatus("다음 날로 넘어갑니다.");
  };

  return (
    <div className="space-y-4">
      <div className="rounded-3xl border border-amber-300/20 bg-amber-500/10 p-4 text-sm text-amber-50">
        <div className="text-xs uppercase tracking-[0.3em] text-amber-200/70">Emergency Local Mode</div>
        <h2 className="mt-2 text-xl font-semibold">Deep Stake 비상 플레이 화면</h2>
        <p className="mt-2 text-amber-50/90">
          메인 게임 UI에서 런타임 오류가 발생해도 이 화면으로 즉시 진입해 월드 이동과 기본 상호작용을 계속 테스트할 수 있습니다.
        </p>
      </div>

      <div className="rounded-3xl border border-white/10 bg-black/30 p-4">
        <div className="mb-3 flex flex-wrap items-center justify-between gap-3 text-sm text-slate-200">
          <div>맵: {state.player.mapId}</div>
          <div>
            {state.time.day}일차 · {formatClock(state.time.minutes)}
          </div>
          <div>
            기력 {state.player.energy} / {state.player.maxEnergy}
          </div>
        </div>

        <div className="relative min-h-[420px] rounded-2xl border border-white/10 bg-slate-950/60 p-2">
          <canvas
            ref={canvasRef}
            width={720}
            height={540}
            className="mx-auto block max-w-full rounded-xl bg-slate-900 shadow-[0_0_0_1px_rgba(255,255,255,0.08)]"
          />
          <div className="pointer-events-none absolute bottom-4 left-4 right-4 rounded-2xl border border-white/10 bg-black/45 px-4 py-3 text-xs text-slate-200">
            이동 버튼으로 플레이어를 움직이고, 행동/확인/수면 버튼으로 기본 루프를 검증할 수 있습니다.
          </div>
        </div>
      </div>

      <div className="grid gap-3 lg:grid-cols-[1fr_auto]">
        <div className="grid grid-cols-3 gap-2 rounded-2xl border border-white/10 bg-black/20 p-3">
          <div />
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => move("up")}>
            <span className="inline-flex items-center gap-2">
              <MoveUp className="h-4 w-4" />
              위
            </span>
          </button>
          <div />
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => move("left")}>
            <span className="inline-flex items-center gap-2">
              <MoveLeft className="h-4 w-4" />
              왼쪽
            </span>
          </button>
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => move("down")}>
            <span className="inline-flex items-center gap-2">
              <MoveDown className="h-4 w-4" />
              아래
            </span>
          </button>
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={() => move("right")}>
            <span className="inline-flex items-center gap-2">
              <MoveRight className="h-4 w-4" />
              오른쪽
            </span>
          </button>
        </div>

        <div className="grid grid-cols-3 gap-2 rounded-2xl border border-white/10 bg-black/20 p-3">
          <button type="button" className="rounded-2xl border border-emerald-300/20 bg-emerald-500/15 px-4 py-3 text-sm" onClick={useTool}>
            <span className="inline-flex items-center gap-2">
              <Hammer className="h-4 w-4" />
              행동
            </span>
          </button>
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm" onClick={interact}>
            <span className="inline-flex items-center gap-2">
              <Hand className="h-4 w-4" />
              확인
            </span>
          </button>
          <button type="button" className="rounded-2xl border border-indigo-300/20 bg-indigo-500/15 px-4 py-3 text-sm" onClick={sleep}>
            <span className="inline-flex items-center gap-2">
              <BedDouble className="h-4 w-4" />
              수면
            </span>
          </button>
        </div>
      </div>

      <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm text-slate-200">
        <div className="font-medium">현재 상태</div>
        <div className="mt-2">{status}</div>
      </div>
    </div>
  );
}
