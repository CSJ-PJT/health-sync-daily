import type { LifeSimDialogueLine, LifeSimNpcDefinition } from "@/game/life-sim/types";

export const lifeSimNpcs: LifeSimNpcDefinition[] = [
  {
    id: "archivist",
    name: { ko: "기록관 아리아", en: "Archivist Aria" },
    defaultMapId: "village",
    schedule: [
      {
        period: "morning",
        mapId: "village",
        x: 10,
        y: 5,
        hint: {
          ko: "광장 기록 보관고 앞에서 봉인된 문서를 정리합니다.",
          en: "Sorts sealed records outside the square archive.",
        },
      },
      {
        period: "afternoon",
        mapId: "village",
        x: 12,
        y: 4,
        hint: {
          ko: "남쪽 아치 아래에서 오래된 지도와 사본을 맞춰 봅니다.",
          en: "Compares old maps and copied fragments beneath the south arch.",
        },
      },
      {
        period: "evening",
        mapId: "village",
        x: 8,
        y: 6,
        hint: {
          ko: "광장 가장자리에서 사라진 기록의 빈칸을 다시 읽고 있습니다.",
          en: "Rereads the missing passages near the edge of the square.",
        },
      },
    ],
  },
  {
    id: "mechanic",
    name: { ko: "정비공 도윤", en: "Mechanic Doyun" },
    defaultMapId: "village",
    schedule: [
      {
        period: "morning",
        mapId: "village",
        x: 5,
        y: 8,
        hint: {
          ko: "정화 작업장 옆에서 끊어진 관로를 점검합니다.",
          en: "Checks broken purifier lines near the workyard.",
        },
      },
      {
        period: "afternoon",
        mapId: "farm",
        x: 10,
        y: 6,
        hint: {
          ko: "농장 북쪽에서 무너진 다리와 배수로를 살펴봅니다.",
          en: "Inspects the collapsed bridge and drainage route by the farm.",
        },
      },
      {
        period: "evening",
        mapId: "village",
        x: 4,
        y: 9,
        hint: {
          ko: "작업대에서 등불 부품과 복구 키트를 조립합니다.",
          en: "Assembles lantern parts and repair kits at the workbench.",
        },
      },
    ],
  },
];

export const lifeSimDialogue: LifeSimDialogueLine[] = [
  {
    id: "archivist-default",
    speaker: "archivist",
    condition: "default",
    text: {
      ko: "봉인된 기록에는 이 마을 아래가 훨씬 거대한 체계였다고 적혀 있어요. 하지만 가장 중요한 쪽이 비어 있습니다.",
      en: "The sealed records say a much larger system once slept beneath this village, but the most important pages are missing.",
    },
  },
  {
    id: "archivist-first-day",
    speaker: "archivist",
    condition: "first-day",
    text: {
      ko: "새 농장 주인이군요. 가장 긴 새벽은 언제나 가장 깊은 어둠 다음에 옵니다.",
      en: "So the new farmer has arrived. The longest dawn always follows the deepest dark.",
    },
  },
  {
    id: "archivist-mine",
    speaker: "archivist",
    condition: "mine-visited",
    text: {
      ko: "광산의 문양을 봤다면 기억해 두세요. 단순한 붕괴 흔적이 아니라 오래된 그림자 행정의 자취일 수도 있어요.",
      en: "If you saw the sigils in the mine, remember them. They may be traces of an old shadow administration, not mere ruin.",
    },
  },
  {
    id: "archivist-friend",
    speaker: "archivist",
    condition: "default",
    text: {
      ko: "기록은 사실만 남기지 않아요. 지워진 무언가를 읽어내는 법도 배워야 해요.",
      en: "Records do not preserve only facts. You also have to learn how to read what was erased.",
    },
  },
  {
    id: "mechanic-default",
    speaker: "mechanic",
    condition: "default",
    text: {
      ko: "겉만 고쳐서는 부족해요. 정화 관로를 다시 깨우면 마을 전체가 숨을 돌릴 수 있을 겁니다.",
      en: "Surface repairs are not enough. If we wake the purifier lines, the whole town can breathe again.",
    },
  },
  {
    id: "mechanic-low-energy",
    speaker: "mechanic",
    condition: "low-energy",
    text: {
      ko: "기력이 많이 비어 보이네요. 오늘은 일찍 쉬고 내일 다시 움직여요.",
      en: "You look drained. Rest early tonight and move again tomorrow.",
    },
  },
  {
    id: "mechanic-has-turnip",
    speaker: "mechanic",
    condition: "has-turnip",
    text: {
      ko: "새벽 순무를 가져왔군요. 식재료로도 좋고, 정화 작업 전에 몸을 안정시키는 데도 좋아요.",
      en: "You brought a dawn turnip. Good for food, and good for steadying yourself before hard work.",
    },
  },
  {
    id: "mechanic-friend",
    speaker: "mechanic",
    condition: "default",
    text: {
      ko: "도구는 손보다 마음을 먼저 따라가요. 계속 돌아오다 보면 이 마을도 당신을 기억하게 될 겁니다.",
      en: "Tools follow intent before they follow hands. Keep returning, and this town will start remembering you too.",
    },
  },
];
