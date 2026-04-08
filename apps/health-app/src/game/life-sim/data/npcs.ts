import type { LifeSimDialogueLine, LifeSimNpcDefinition } from "@/game/life-sim/types";

export const lifeSimNpcs: LifeSimNpcDefinition[] = [
  {
    id: "archivist",
    name: { ko: "기록관리인 아리아", en: "Archivist Aria" },
    mapId: "village",
    x: 10,
    y: 5,
  },
  {
    id: "mechanic",
    name: { ko: "정비사 도윤", en: "Mechanic Doyun" },
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
      ko: "봉인된 기록에 따르면 이 마을은 더 거대한 지하 체계 위에 세워졌어요. 누군가 많은 페이지를 지워 버렸죠.",
      en: "The sealed records say this village was built over a much larger buried system. Someone erased many of the pages.",
    },
  },
  {
    id: "archivist-first-day",
    speaker: "archivist",
    condition: "first-day",
    text: {
      ko: "새 농장지기군요. 가장 어두운 순간이 지나야 새벽이 또렷하게 보이기 시작해요.",
      en: "So you are the new farmer. Dawn is always darkest just before it arrives.",
    },
  },
  {
    id: "archivist-mine",
    speaker: "archivist",
    condition: "mine-visited",
    text: {
      ko: "광맥 아래에서 본 문양을 기억해 두세요. 단순한 잔해가 아니라 오래된 그림자 행정 구조의 흔적일지도 몰라요.",
      en: "Remember the marks you saw in the mine. They may be the sigils of an old shadow administration, not mere rubble.",
    },
  },
  {
    id: "mechanic-default",
    speaker: "mechanic",
    condition: "default",
    text: {
      ko: "지상의 수리만으로는 부족해. 예전 정화 관로를 깨우려면 마을 전체가 다시 숨을 쉬게 해야 해.",
      en: "Surface repairs are not enough. If we wake the old purifier lines, the whole town can breathe again.",
    },
  },
  {
    id: "mechanic-low-energy",
    speaker: "mechanic",
    condition: "low-energy",
    text: {
      ko: "기운이 많이 빠졌네. 오늘은 일찍 쉬고, 내일 회복된 몸으로 다시 움직이자.",
      en: "You are running on fumes. Sleep early and trust tomorrow's recovery.",
    },
  },
  {
    id: "mechanic-has-turnip",
    speaker: "mechanic",
    condition: "has-turnip",
    text: {
      ko: "순무를 하나 건네주면 정화 등불 부품을 맞춰 줄 수 있어. 농장의 회복이 마을 수리에 딱 필요했거든.",
      en: "You brought a dawn turnip. Hand it over and I can fit a purifier lantern part for you.",
    },
  },
];
