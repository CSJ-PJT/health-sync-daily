import { Component, type ErrorInfo, type ReactNode } from "react";

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
      message: error.message || "알 수 없는 렌더 오류가 발생했습니다.",
    };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("Fifth Dawn runtime boundary", error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-[60vh] items-center justify-center">
          <div className="w-full max-w-2xl rounded-3xl border border-rose-300/20 bg-rose-500/10 p-6 text-sm text-rose-50">
            <div className="text-xs uppercase tracking-[0.3em] text-rose-200/70">Runtime Fallback</div>
            <h2 className="mt-3 text-2xl font-semibold">게임 화면을 그리는 중 오류가 발생했습니다</h2>
            <p className="mt-3 text-rose-100/90">
              최소한의 진단 패널은 정상적으로 보이고 있습니다. 아래 오류 메시지를 기준으로 추가 복구를 진행하면 됩니다.
            </p>
            <div className="mt-4 rounded-2xl border border-white/10 bg-black/30 px-4 py-3 font-mono text-xs text-amber-100">
              {this.state.message}
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
