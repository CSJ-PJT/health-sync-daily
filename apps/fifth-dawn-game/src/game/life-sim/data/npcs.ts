import type { LifeSimDialogueLine, LifeSimNpcDefinition } from "@/game/life-sim/types";

export const lifeSimNpcs: LifeSimNpcDefinition[] = [
  {
    id: "archivist",
    name: { ko: "기록관 아리아", en: "Archivist Aria" },
    mapId: "village",
    x: 10,
    y: 5,
  },
  {
    id: "mechanic",
    name: { ko: "정비공 도윤", en: "Mechanic Doyun" },
    mapId: "village",
    x: 5,
    y: 8,
  },
];

export const lifeSimDialogue: LifeSimDialogueLine[] = [
  {
    id: "archivist-default",
    speaker: "archivist",
    condition: "default",
    text: {
      ko: "봉인된 기록에 따르면 이 마을은 더 거대한 묻힌 체계 위에 세워졌어요. 누군가 많은 페이지를 잘라냈죠.",
      en: "The sealed records say this village was built over a much larger buried system. Someone removed many of the pages.",
    },
  },
  {
    id: "archivist-first-day",
    speaker: "archivist",
    condition: "first-day",
    text: {
      ko: "새 농장주군요. 가장 긴 새벽은 언제나 가장 어두운 순간 뒤에 시작돼요.",
      en: "So you are the new farmer. The longest dawn begins right after the deepest dark.",
    },
  },
  {
    id: "archivist-mine",
    speaker: "archivist",
    condition: "mine-visited",
    text: {
      ko: "광산 아래에서 본 문양을 기억해 두세요. 그건 단순한 폐허가 아니라 오래된 그림자 행정 구조의 흔적일지 몰라요.",
      en: "Remember the marks you saw in the mine. They may be traces of an old shadow administration, not mere rubble.",
    },
  },
  {
    id: "mechanic-default",
    speaker: "mechanic",
    condition: "default",
    text: {
      ko: "겉만 수리해선 부족해. 옛 정화 관로를 깨우면 마을 전체가 다시 숨을 쉬게 될 거야.",
      en: "Surface repairs are not enough. If we wake the old purifier lines, the whole town can breathe again.",
    },
  },
  {
    id: "mechanic-low-energy",
    speaker: "mechanic",
    condition: "low-energy",
    text: {
      ko: "기력이 많이 떨어졌어. 오늘은 일찍 들어가 쉬고, 내일 다시 손을 움직이자.",
      en: "You are running on fumes. Rest early tonight and move again tomorrow.",
    },
  },
  {
    id: "mechanic-has-turnip",
    speaker: "mechanic",
    condition: "has-turnip",
    text: {
      ko: "새벽 무를 가져왔네. 그걸 주면 정화 등불 부품을 맞출 수 있어. 농장 회복에도 도움이 될 거야.",
      en: "You brought a dawn turnip. Trade it in and I can fit a purifier lantern part for you.",
    },
  },
];
