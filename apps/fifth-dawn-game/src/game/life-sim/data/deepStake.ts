import type { DeepStakeCodexEntry, DeepStakeFactionId, DeepStakeFiveDWorldHint, LocalizedText } from "@/game/life-sim/types";

export const deepStakeFactionNames: Record<DeepStakeFactionId, LocalizedText> = {
  "luminous-companions": { ko: "빛의 동행자", en: "Luminous Companions" },
  "serpent-court": { ko: "사문 궁정", en: "Serpent Court" },
  "deep-archive": { ko: "심층 기록보관소", en: "Deep Archive" },
  dawnkeepers: { ko: "새벽 수호단", en: "Dawnkeepers" },
  "shadow-administration": { ko: "그림자 행정 잔재", en: "Shadow Administration Remnants" },
};

export const baseDeepStakeCodexEntries: DeepStakeCodexEntry[] = [
  {
    id: "longest-dawn",
    title: { ko: "가장 긴 새벽", en: "The Longest Dawn" },
    body: {
      ko: "이 세계는 긴 어둠의 끝에서 흔들리고 있습니다. 농장과 마을, 광산의 회복은 단순한 생존이 아니라 새벽을 앞당기는 선택입니다.",
      en: "This world trembles at the end of a long darkness. Restoring the farm, village, and mine is not just survival, but a choice that hastens dawn.",
    },
    unlocked: true,
  },
  {
    id: "deep-stake",
    title: { ko: "딥 스테이크 정렬", en: "Deep Stake Alignment" },
    body: {
      ko: "마을을 돕고, 정착지를 어떤 방향으로 확장하며, 누구의 제안을 받아들이는지에 따라 당신의 정렬과 공명대가 달라집니다.",
      en: "Your alignment and resonance band shift based on how you help the village, shape your settlement, and whose offers you accept.",
    },
    unlocked: true,
  },
  {
    id: "fifth-earth",
    title: { ko: "다섯 번째 지구 전이", en: "Fifth Earth Transition" },
    body: {
      ko: "진정한 상위 세계는 단순한 탈출구가 아닙니다. 공명과 정화가 충분히 쌓일 때, 더 넓은 거주지와 관문 네트워크가 열릴 것입니다.",
      en: "The true higher world is not a simple escape. When resonance and purification accumulate, broader residencies and gate networks will open.",
    },
    unlocked: false,
  },
];

export const baseFiveDWorldHints: DeepStakeFiveDWorldHint[] = [
  {
    id: "star-hub",
    title: { ko: "별 허브", en: "Star Hub" },
    summary: { ko: "여러 거주 세계를 잇는 만남의 관문입니다.", en: "A meeting gate linking many resident worlds." },
    unlocked: false,
  },
  {
    id: "companion-homeworld",
    title: { ko: "동행자 거주권", en: "Companion Residency" },
    summary: { ko: "빛의 동행자와 함께하는 거주 구역 후보입니다.", en: "A residency candidate shared with luminous companions." },
    unlocked: false,
  },
  {
    id: "origin-homeworld",
    title: { ko: "기원 세계 항로", en: "Origin World Route" },
    summary: { ko: "더 높은 정렬과 공명이 필요합니다.", en: "Requires higher alignment and resonance." },
    unlocked: false,
  },
];
