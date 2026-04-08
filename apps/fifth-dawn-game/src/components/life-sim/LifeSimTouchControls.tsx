import { BedDouble, Hand, Hammer, MoveDown, MoveLeft, MoveRight, MoveUp } from "lucide-react";

type Props = {
  onMove(direction: "up" | "down" | "left" | "right"): void;
  onTool(): void;
  onInteract(): void;
  onSleep(): void;
};

export function LifeSimTouchControls({ onMove, onTool, onInteract, onSleep }: Props) {
  return (
    <div className="rounded-2xl border border-white/10 bg-black/25 p-3 lg:hidden">
      <div className="mb-3 text-xs font-medium uppercase tracking-[0.2em] text-white/60">Touch Controls</div>
      <div className="grid gap-3 sm:grid-cols-[1fr_auto]">
        <div className="grid grid-cols-3 gap-2">
          <div />
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-3 py-3" onClick={() => onMove("up")}>
            <MoveUp className="mx-auto h-5 w-5" />
          </button>
          <div />
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-3 py-3" onClick={() => onMove("left")}>
            <MoveLeft className="mx-auto h-5 w-5" />
          </button>
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-3 py-3" onClick={() => onMove("down")}>
            <MoveDown className="mx-auto h-5 w-5" />
          </button>
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-3 py-3" onClick={() => onMove("right")}>
            <MoveRight className="mx-auto h-5 w-5" />
          </button>
        </div>
        <div className="grid grid-cols-3 gap-2 sm:w-[220px]">
          <button type="button" className="rounded-2xl border border-emerald-300/20 bg-emerald-500/15 px-3 py-3 text-xs" onClick={onTool}>
            <Hammer className="mx-auto mb-1 h-4 w-4" />
            행동
          </button>
          <button type="button" className="rounded-2xl border border-white/10 bg-white/5 px-3 py-3 text-xs" onClick={onInteract}>
            <Hand className="mx-auto mb-1 h-4 w-4" />
            확인
          </button>
          <button type="button" className="rounded-2xl border border-indigo-300/20 bg-indigo-500/15 px-3 py-3 text-xs" onClick={onSleep}>
            <BedDouble className="mx-auto mb-1 h-4 w-4" />
            수면
          </button>
        </div>
      </div>
    </div>
  );
}
