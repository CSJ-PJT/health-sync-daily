import type { LifeSimProgressionState, LifeSimStoryFlags } from "@/game/life-sim/types";

export function getProgressionOverview(progression: LifeSimProgressionState, storyFlags: LifeSimStoryFlags) {
  return {
    unlockedRecipeCount: progression.unlockedRecipes.length,
    discoveredMapCount: progression.discoveredMaps.length,
    completedQuestCount: progression.completedQuestIds.length,
    restoredBridge: storyFlags.restoredBridge,
    surveyedNorthReach: storyFlags.surveyedNorthReach,
  };
}

export function getProgressionHint(progression: LifeSimProgressionState, storyFlags: LifeSimStoryFlags) {
  if (!storyFlags.harvestedFirstCrop) {
    return "농장에서 첫 수확을 마치면 본격적인 성장 루프가 열립니다.";
  }
  if (!storyFlags.enteredMine) {
    return "마을 동쪽 광산으로 들어가 자원 채집 루프를 시작하세요.";
  }
  if (!storyFlags.repairedLantern) {
    return "정화 등불을 복구해 정비공과 복구 루프를 연결하세요.";
  }
  if (!storyFlags.restoredBridge) {
    return "다리 복구 키트를 만들어 북쪽 경로를 열어 보세요.";
  }
  if (!storyFlags.surveyedNorthReach) {
    return "북부 개척지에서 공명 파편을 채집하고 전초기지 후보지를 조사하세요.";
  }
  if (progression.resonancePoints < 20) {
    return "공명 포인트를 더 모아 정착지 업그레이드를 준비하세요.";
  }
  return "정착지 업그레이드와 시설 확장을 통해 다음 거주 구역과 관문 단서를 준비하세요.";
}
