import { Maximize2, Minimize2, X } from "lucide-react";
import { useEffect, useState, type ReactNode } from "react";

type Props = {
  title: string;
  subtitle: string;
  onExit?: () => void;
  sidebar?: ReactNode;
  children: ReactNode;
};

export function FullscreenGameHost({ title, subtitle, onExit, sidebar, children }: Props) {
  const [isFullscreen, setIsFullscreen] = useState(false);

  useEffect(() => {
    const handler = () => setIsFullscreen(Boolean(document.fullscreenElement));
    document.addEventListener("fullscreenchange", handler);
    return () => document.removeEventListener("fullscreenchange", handler);
  }, []);

  const toggleFullscreen = async () => {
    if (document.fullscreenElement) {
      await document.exitFullscreen();
      return;
    }
    await document.documentElement.requestFullscreen();
  };

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(120,184,255,0.18),_transparent_40%),linear-gradient(180deg,#07111f_0%,#111827_45%,#050b14_100%)] text-slate-50">
      <div className="mx-auto flex min-h-screen max-w-[1600px] flex-col gap-4 p-4">
        <header className="flex flex-wrap items-center justify-between gap-3 rounded-3xl border border-white/10 bg-black/20 px-5 py-4 backdrop-blur">
          <div>
            <div className="text-xs uppercase tracking-[0.3em] text-emerald-200/80">Fifth Dawn Valley</div>
            <h1 className="text-2xl font-semibold">{title}</h1>
            <p className="text-sm text-slate-300">{subtitle}</p>
          </div>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={toggleFullscreen}
              className="rounded-2xl border border-white/10 bg-white/5 px-4 py-2 text-sm transition hover:bg-white/10"
            >
              <span className="inline-flex items-center gap-2">
                {isFullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
                {isFullscreen ? "창 모드" : "전체 화면"}
              </span>
            </button>
            {onExit ? (
              <button
                type="button"
                onClick={onExit}
                className="rounded-2xl border border-rose-300/20 bg-rose-400/10 px-4 py-2 text-sm transition hover:bg-rose-400/20"
              >
                <span className="inline-flex items-center gap-2">
                  <X className="h-4 w-4" />
                  종료
                </span>
              </button>
            ) : null}
          </div>
        </header>

        <div className="grid min-h-0 flex-1 gap-4 xl:grid-cols-[1fr_320px]">
          <div className="min-h-0 rounded-[2rem] border border-white/10 bg-black/20 p-3 backdrop-blur">
            {children}
          </div>
          {sidebar ? (
            <aside className="min-h-0 rounded-[2rem] border border-white/10 bg-black/20 p-4 backdrop-blur">
              {sidebar}
            </aside>
          ) : null}
        </div>
      </div>
    </div>
  );
}
