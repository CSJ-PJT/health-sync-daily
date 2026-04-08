import { useNavigate } from "react-router-dom";
import { FullscreenGameHost } from "@/components/entertainment/FullscreenGameHost";
import { LifeSimArena } from "@/components/entertainment/LifeSimArena";
import { useDeviceBackNavigation } from "@/hooks/useDeviceBackNavigation";

export default function LifeSimPage() {
  const navigate = useNavigate();

  useDeviceBackNavigation({
    fallback: "/game",
    onBackWithinPage: () => {
      if (document.fullscreenElement) {
        void document.exitFullscreen?.();
        return true;
      }
      return false;
    },
  });

  return (
    <FullscreenGameHost
      title="제5새벽 계곡"
      subtitle="회복 농장, 새벽 마을, 정화 광맥을 오가며 하루의 루틴을 쌓는 톱다운 라이프 심 RPG"
      onBack={() => navigate("/game", { replace: true })}
    >
      <LifeSimArena onExit={() => navigate("/game", { replace: true })} />
    </FullscreenGameHost>
  );
}
