import { Component, type ErrorInfo, type ReactNode } from "react";
import { EmergencyLifeSimFallback } from "@/components/EmergencyLifeSimFallback";

type Props = {
  children: ReactNode;
};

type State = {
  hasError: boolean;
  message: string;
};

export class GameRuntimeBoundary extends Component<Props, State> {
  state: State = {
    hasError: false,
    message: "",
  };

  static getDerivedStateFromError(error: Error): State {
    return {
      hasError: true,
      message: error.message || "알 수 없는 런타임 오류가 발생했습니다.",
    };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("Fifth Dawn runtime boundary", error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="space-y-4">
          <div className="rounded-3xl border border-rose-300/20 bg-rose-500/10 p-6 text-sm text-rose-50">
            <div className="text-xs uppercase tracking-[0.3em] text-rose-200/70">Runtime Fallback</div>
            <h2 className="mt-3 text-2xl font-semibold">메인 게임 UI에서 오류가 발생했습니다</h2>
            <p className="mt-3 text-rose-100/90">고급 패널을 우회하고, 즉시 플레이 가능한 로컬 비상 모드로 내려갑니다.</p>
            <div className="mt-4 rounded-2xl border border-white/10 bg-black/30 px-4 py-3 font-mono text-xs text-amber-100">
              {this.state.message}
            </div>
          </div>
          <EmergencyLifeSimFallback />
        </div>
      );
    }

    return this.props.children;
  }
}
