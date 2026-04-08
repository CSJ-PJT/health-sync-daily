import { Settings2 } from "lucide-react";
import { applyInputPreset, type LifeSimInputPreset } from "@/game/life-sim/state/settings";
import type { LifeSimInputAction, LifeSimSaveMode, LifeSimSettings } from "@/game/life-sim/types";

type Props = {
  open: boolean;
  settings: LifeSimSettings;
  onToggle(): void;
  onChange(next: LifeSimSettings): void;
};

const actionLabels: Array<[LifeSimInputAction, string]> = [
  ["move-up", "이동 위"],
  ["move-down", "이동 아래"],
  ["move-left", "이동 왼쪽"],
  ["move-right", "이동 오른쪽"],
  ["interact", "대화/확인"],
  ["use-tool", "행동"],
  ["sleep", "수면"],
  ["hotbar-1", "핫바 1"],
  ["hotbar-2", "핫바 2"],
  ["hotbar-3", "핫바 3"],
  ["hotbar-4", "핫바 4"],
  ["hotbar-5", "핫바 5"],
];

export function LifeSimSettingsPanel({ open, settings, onToggle, onChange }: Props) {
  return (
    <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
      <div className="mb-2 flex items-center justify-between">
        <div className="font-medium">환경 설정</div>
        <button type="button" onClick={onToggle} className="rounded-xl border border-white/10 px-2 py-1 text-xs">
          <span className="inline-flex items-center gap-1"><Settings2 className="h-3.5 w-3.5" />{open ? "접기" : "열기"}</span>
        </button>
      </div>
      {open ? (
        <div className="space-y-3 text-xs text-slate-300">
          <label className="block">
            <div className="mb-1">해상도 배율</div>
            <input
              type="range"
              min="1"
              max="2"
              step="0.25"
              value={settings.resolutionScale}
              onChange={(event) => onChange({ ...settings, resolutionScale: Number(event.target.value) })}
              className="w-full"
            />
          </label>
          <label className="block">
            <div className="mb-1">오디오 볼륨</div>
            <input
              type="range"
              min="0"
              max="1"
              step="0.05"
              value={settings.audioVolume}
              onChange={(event) => onChange({ ...settings, audioVolume: Number(event.target.value) })}
              className="w-full"
            />
          </label>
          <div className="grid grid-cols-3 gap-2">
            {(["auto", "local", "cloud"] as LifeSimSaveMode[]).map((mode) => (
              <button
                key={mode}
                type="button"
                onClick={() => onChange({ ...settings, saveMode: mode })}
                className={`rounded-xl border px-2 py-2 ${settings.saveMode === mode ? "border-emerald-300 bg-emerald-400/15" : "border-white/10"}`}
              >
                {mode}
              </button>
            ))}
          </div>
          <div className="grid grid-cols-2 gap-2">
            {(["wasd", "arrows"] as LifeSimInputPreset[]).map((preset) => (
              <button
                key={preset}
                type="button"
                onClick={() => onChange(applyInputPreset(settings, preset))}
                className="rounded-xl border border-white/10 px-2 py-2"
              >
                {preset === "wasd" ? "WASD" : "Arrow"}
              </button>
            ))}
          </div>
          <div className="grid gap-2">
            <button
              type="button"
              onClick={() =>
                onChange({
                  ...settings,
                  inputMode: settings.inputMode === "keyboard-mouse" ? "controller-ready" : "keyboard-mouse",
                })
              }
              className="rounded-xl border border-white/10 px-2 py-2 text-left"
            >
              입력 모드: {settings.inputMode === "keyboard-mouse" ? "키보드 + 마우스" : "컨트롤러 준비"}
            </button>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={settings.fullscreenPreferred}
                onChange={(event) => onChange({ ...settings, fullscreenPreferred: event.target.checked })}
              />
              전체 화면 선호
            </label>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={settings.showPerformanceOverlay}
                onChange={(event) => onChange({ ...settings, showPerformanceOverlay: event.target.checked })}
              />
              성능 오버레이 표시
            </label>
          </div>
          <div className="rounded-xl border border-white/10 p-3">
            <div className="mb-2 font-medium">현재 키 바인딩</div>
            <div className="space-y-1">
              {actionLabels.map(([action, label]) => (
                <div key={action} className="flex items-center justify-between gap-2">
                  <span>{label}</span>
                  <span className="text-slate-400">{settings.keyBindings[action].join(" / ")}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
