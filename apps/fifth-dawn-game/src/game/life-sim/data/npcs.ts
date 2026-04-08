import type { LifeSimDialogueLine, LifeSimNpcDefinition } from "@/game/life-sim/types";

export const lifeSimNpcs: LifeSimNpcDefinition[] = [
  {
    id: "archivist",
    name: { ko: "기록관 아리아", en: "Archivist Aria" },
    mapId: "village",
    x: 10,
    y: 5,
    scheduleHint: {
      morning: { ko: "광장 기록 보관소 근처를 정리합니다.", en: "Organizes records near the square archive." },
      evening: { ko: "광장 가장자리에서 봉인 기록을 검토합니다.", en: "Reviews sealed records at the edge of the square." },
    },
  },
  {
    id: "mechanic",
    name: { ko: "정비공 도윤", en: "Mechanic Doyun" },
    mapId: "village",
    x: 5,
    y: 8,
    scheduleHint: {
      morning: { ko: "정화 배관을 손보며 통로 복구를 준비합니다.", en: "Repairs purifier lines and plans route restoration." },
      evening: { ko: "작업대에서 부품을 조립합니다.", en: "Assembles parts at the workbench." },
    },
  },
];

export const lifeSimDialogue: LifeSimDialogueLine[] = [
  {
    id: "archivist-default",
    speaker: "archivist",
    condition: "default",
    text: {
      ko: "봉인된 기록에는 이 마을 아래 더 거대한 체계가 잠들어 있었다고 적혀 있어요. 누군가 중요한 장을 뜯어낸 흔적도 남아 있죠.",
      en: "The sealed records say a much larger system once slept beneath this village. Someone tore out the most important pages.",
    },
  },
  {
    id: "archivist-first-day",
    speaker: "archivist",
    condition: "first-day",
    text: {
      ko: "새 농장지기가 왔군요. 가장 긴 새벽은 늘 가장 깊은 어둠 뒤에 시작된답니다.",
      en: "So the new farmer has arrived. The longest dawn always begins after the deepest dark.",
    },
  },
  {
    id: "archivist-mine",
    speaker: "archivist",
    condition: "mine-visited",
    text: {
      ko: "광산의 문양을 봤다면 기억해 두세요. 그건 단순한 폐허가 아니라 오래된 그림자 행정 구조의 흔적일지도 몰라요.",
      en: "If you saw the sigils in the mine, remember them well. They may be traces of an old shadow administration, not mere ruin.",
    },
  },
  {
    id: "mechanic-default",
    speaker: "mechanic",
    condition: "default",
    text: {
      ko: "겉만 고치는 걸로는 부족해요. 정화 배관을 다시 깨우면 마을 전체가 숨을 돌릴 수 있을 겁니다.",
      en: "Surface repairs are not enough. If we wake the purifier lines, the whole town can breathe again.",
    },
  },
  {
    id: "mechanic-low-energy",
    speaker: "mechanic",
    condition: "low-energy",
    text: {
      ko: "기력이 많이 빠졌네요. 오늘은 일찍 들어가 쉬고, 내일 다시 움직이세요.",
      en: "You look drained. Rest early tonight and move again tomorrow.",
    },
  },
  {
    id: "mechanic-has-turnip",
    speaker: "mechanic",
    condition: "has-turnip",
    text: {
      ko: "새벽 순무를 가져왔군요. 식재료는 물론이고, 정화 작업 전 몸을 추스르는 데도 큰 도움이 돼요.",
      en: "You brought a dawn turnip. Good for food, and good for steadying yourself before hard work.",
    },
  },
];
