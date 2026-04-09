import { baseDeepStakeCodexEntries, baseFiveDWorldHints } from "@/game/life-sim/data/deepStake";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import type { SettlementObjectType } from "@/game/settlement/settlementTypes";
import type {
  DeepStakeAscensionStage,
  DeepStakeBandId,
  DeepStakeChoiceId,
  DeepStakeFactionId,
  DeepStakeState,
  LifeSimState,
} from "@/game/life-sim/types";

function clamp(value: number, min = 0, max = 100) {
  return Math.max(min, Math.min(max, value));
}

export function createInitialDeepStakeState(): DeepStakeState {
  return {
    alignment: {
      luminousAffinity: 8,
      shadowAffinity: 4,
      resonanceStability: 18,
      corruptionPressure: 22,
      awakeningClarity: 12,
      compassion: 10,
      domination: 4,
      attunement: 10,
    },
    factionAffinities: {
      "luminous-companions": 6,
      "serpent-court": 0,
      "deep-archive": 4,
      dawnkeepers: 5,
      "shadow-administration": 3,
    },
    chosenPaths: [],
    supportSignals: [],
    warFoundation: {
      resonanceGroupingKey: "threshold-low",
      preferredSide: "threshold",
      territorialPressure: 18,
      futureSquadEligible: false,
    },
    ascensionStage: "three-d-world",
    trueAscensionUnlocked: false,
    fiveDResidencyUnlocked: false,
    codexEntries: baseDeepStakeCodexEntries.map((entry) => ({ ...entry })),
    worldHints: baseFiveDWorldHints.map((entry) => ({ ...entry })),
  };
}

export function getResonanceBand(state: DeepStakeState): { id: DeepStakeBandId; title: string; summary: string } {
  const { luminousAffinity, shadowAffinity, resonanceStability, corruptionPressure, awakeningClarity } = state.alignment;
  const locale = getLifeSimLocale();

  if (shadowAffinity >= 55 || corruptionPressure >= 60) {
    return {
      id: "unstable-shadow-band",
      title: t({ ko: "불안정한 그림자 대역", en: "Unstable Shadow Band" }, locale),
      summary: t(
        { ko: "압박과 지배 성향이 커서 장기적으로 붕괴 위험이 높습니다.", en: "Pressure and domination are high, raising long-term collapse risk." },
        locale,
      ),
    };
  }
  if (luminousAffinity >= 55 && resonanceStability >= 50 && awakeningClarity >= 35) {
    return {
      id: "high-luminous-band",
      title: t({ ko: "고공명 빛 대역", en: "High Luminous Band" }, locale),
      summary: t(
        { ko: "정화와 공감, 안정성이 충분히 쌓여 상위 관문을 감지할 수 있습니다.", en: "Purification, empathy, and stability are strong enough to sense higher gates." },
        locale,
      ),
    };
  }
  if (luminousAffinity >= 30 && resonanceStability >= 28) {
    return {
      id: "dawn-aligned-band",
      title: t({ ko: "새벽 정렬 대역", en: "Dawn-Aligned Band" }, locale),
      summary: t(
        { ko: "마을 회복과 정착지 확장이 공명 축을 빛 쪽으로 끌어올리고 있습니다.", en: "Village recovery and settlement growth are pulling your resonance toward dawn." },
        locale,
      ),
    };
  }
  return {
    id: "mixed-threshold-band",
    title: t({ ko: "혼합 임계 대역", en: "Mixed Threshold Band" }, locale),
    summary: t(
      { ko: "빛과 그림자 흔적이 함께 남아 있으며, 선택이 정렬을 크게 흔듭니다.", en: "Traces of light and shadow coexist, so choices can swing alignment sharply." },
      locale,
    ),
  };
}

export function getAscensionStageMeta(stage: DeepStakeAscensionStage) {
  const locale = getLifeSimLocale();
  switch (stage) {
    case "three-d-world":
      return {
        title: t({ ko: "3D 생존권", en: "3D Survival State" }, locale),
        summary: t({ ko: "생존과 회복이 우선인 초기 세계 상태입니다.", en: "Early-world state where survival and recovery come first." }, locale),
      };
    case "four-d-threshold":
      return {
        title: t({ ko: "4D 감응 문턱", en: "4D Threshold" }, locale),
        summary: t({ ko: "직접 보이지 않던 오멘과 보호 파동을 감지하기 시작합니다.", en: "You begin sensing omens and protective waves once hidden from sight." }, locale),
      };
    case "dawn-transition":
      return {
        title: t({ ko: "새벽 전이", en: "Dawn Transition" }, locale),
        summary: t({ ko: "정화와 정착지 복원이 새로운 질서로 넘어가는 길을 엽니다.", en: "Purification and settlement renewal open a path into a new order." }, locale),
      };
    case "fifth-threshold":
      return {
        title: t({ ko: "오차원 문턱", en: "Fifth Threshold" }, locale),
        summary: t({ ko: "더 높은 거주 세계와 관문 네트워크의 윤곽이 보입니다.", en: "The outline of higher residences and gate networks becomes visible." }, locale),
      };
    case "true-ascension":
      return {
        title: t({ ko: "진정한 승천", en: "True Ascension" }, locale),
        summary: t({ ko: "옛 갈등의 틀을 넘은 새로운 상태가 해금됩니다.", en: "A new state beyond the old conflict framework becomes available." }, locale),
      };
    case "five-d-residency":
      return {
        title: t({ ko: "5D 거주권", en: "5D Residency" }, locale),
        summary: t({ ko: "상위 거주지와 사회 허브, 여행 관문이 열립니다.", en: "Higher residences, social hubs, and travel gates unlock." }, locale),
      };
  }
}

function recomputeDerived(state: DeepStakeState): DeepStakeState {
  const band = getResonanceBand(state);
  const preferredSide = state.alignment.shadowAffinity > state.alignment.luminousAffinity + 12 ? "shadow" : band.id === "mixed-threshold-band" ? "threshold" : "luminous";
  const groupingKey = `${band.id}:${preferredSide}`;
  const totalResonance =
    state.alignment.luminousAffinity +
    state.alignment.resonanceStability +
    state.alignment.awakeningClarity +
    state.alignment.attunement -
    state.alignment.shadowAffinity -
    state.alignment.corruptionPressure;

  let ascensionStage: DeepStakeAscensionStage = "three-d-world";
  if (totalResonance >= 25) ascensionStage = "four-d-threshold";
  if (totalResonance >= 55) ascensionStage = "dawn-transition";
  if (totalResonance >= 85) ascensionStage = "fifth-threshold";
  if (totalResonance >= 115 && state.alignment.shadowAffinity < 35) ascensionStage = "true-ascension";
  if (totalResonance >= 140 && state.alignment.shadowAffinity < 25) ascensionStage = "five-d-residency";

  const signals = [...state.supportSignals];
  const sensedHigher = ascensionStage !== "three-d-world";
  if (sensedHigher && !signals.find((entry) => entry.id === "dawnkeepers-pulse")) {
    signals.push({
      id: "dawnkeepers-pulse",
      type: "blessing-pulse",
      sourceFactionId: "dawnkeepers",
      strength: 18,
      sensed: sensedHigher,
      note: {
        ko: "새벽 수호단의 잔잔한 보호 파동이 농장 둘레를 감싸고 있습니다.",
        en: "A gentle protective wave from the Dawnkeepers surrounds the farm.",
      },
    });
  }
  if (state.alignment.shadowAffinity >= 35 && !signals.find((entry) => entry.id === "serpent-whisper")) {
    signals.push({
      id: "serpent-whisper",
      type: "shadow-whisper",
      sourceFactionId: "serpent-court",
      strength: 20,
      sensed: sensedHigher,
      note: {
        ko: "낡은 관문 아래에서 속삭임이 올라옵니다. 쉬운 힘을 약속하지만 대가를 요구합니다.",
        en: "Whispers rise beneath the old gate, promising easy power at a cost.",
      },
    });
  }

  return {
    ...state,
    ascensionStage,
    trueAscensionUnlocked: ascensionStage === "true-ascension" || ascensionStage === "five-d-residency",
    fiveDResidencyUnlocked: ascensionStage === "five-d-residency",
    warFoundation: {
      resonanceGroupingKey: groupingKey,
      preferredSide,
      territorialPressure: clamp(Math.round((state.alignment.shadowAffinity + state.alignment.corruptionPressure) / 2)),
      futureSquadEligible: ascensionStage !== "three-d-world",
    },
    codexEntries: state.codexEntries.map((entry) =>
      entry.id === "fifth-earth" && ascensionStage !== "three-d-world" ? { ...entry, unlocked: true } : entry,
    ),
    worldHints: state.worldHints.map((entry) =>
      entry.id === "star-hub"
        ? { ...entry, unlocked: ascensionStage !== "three-d-world" }
        : entry.id === "companion-homeworld"
          ? { ...entry, unlocked: ascensionStage === "fifth-threshold" || ascensionStage === "true-ascension" || ascensionStage === "five-d-residency" }
          : entry.id === "origin-homeworld"
            ? { ...entry, unlocked: ascensionStage === "five-d-residency" }
            : entry,
    ),
    supportSignals: signals,
  };
}

export function applyDeepStakeChoice(state: LifeSimState, choiceId: DeepStakeChoiceId): LifeSimState {
  if (state.deepStake.chosenPaths.includes(choiceId)) {
    return state;
  }

  const next = {
    ...state.deepStake,
    chosenPaths: [...state.deepStake.chosenPaths, choiceId],
    alignment: { ...state.deepStake.alignment },
    factionAffinities: { ...state.deepStake.factionAffinities },
  };

  switch (choiceId) {
    case "village-recovery-vow":
      next.alignment.luminousAffinity = clamp(next.alignment.luminousAffinity + 8);
      next.alignment.compassion = clamp(next.alignment.compassion + 10);
      next.alignment.resonanceStability = clamp(next.alignment.resonanceStability + 6);
      next.factionAffinities.dawnkeepers = clamp(next.factionAffinities.dawnkeepers + 8);
      break;
    case "accept-luminous-guidance":
      next.alignment.luminousAffinity = clamp(next.alignment.luminousAffinity + 10);
      next.alignment.awakeningClarity = clamp(next.alignment.awakeningClarity + 12);
      next.alignment.attunement = clamp(next.alignment.attunement + 8);
      next.factionAffinities["luminous-companions"] = clamp(next.factionAffinities["luminous-companions"] + 12);
      break;
    case "study-deep-archive":
      next.alignment.awakeningClarity = clamp(next.alignment.awakeningClarity + 10);
      next.alignment.attunement = clamp(next.alignment.attunement + 6);
      next.alignment.resonanceStability = clamp(next.alignment.resonanceStability + 4);
      next.factionAffinities["deep-archive"] = clamp(next.factionAffinities["deep-archive"] + 10);
      break;
    case "accept-shadow-bargain":
      next.alignment.shadowAffinity = clamp(next.alignment.shadowAffinity + 14);
      next.alignment.domination = clamp(next.alignment.domination + 10);
      next.alignment.corruptionPressure = clamp(next.alignment.corruptionPressure + 8);
      next.alignment.resonanceStability = clamp(next.alignment.resonanceStability - 4);
      next.factionAffinities["serpent-court"] = clamp(next.factionAffinities["serpent-court"] + 16);
      next.factionAffinities["shadow-administration"] = clamp(next.factionAffinities["shadow-administration"] + 8);
      break;
  }

  return {
    ...state,
    deepStake: recomputeDerived(next),
  };
}

export function applySettlementStructureInfluence(state: LifeSimState, objectType: SettlementObjectType): LifeSimState {
  const next = {
    ...state.deepStake,
    alignment: { ...state.deepStake.alignment },
    factionAffinities: { ...state.deepStake.factionAffinities },
  };

  if (objectType === "beacon") {
    next.alignment.luminousAffinity = clamp(next.alignment.luminousAffinity + 4);
    next.alignment.resonanceStability = clamp(next.alignment.resonanceStability + 4);
    next.factionAffinities.dawnkeepers = clamp(next.factionAffinities.dawnkeepers + 4);
  } else if (objectType === "garden-bed" || objectType === "tree" || objectType === "flower") {
    next.alignment.compassion = clamp(next.alignment.compassion + 2);
    next.alignment.attunement = clamp(next.alignment.attunement + 2);
    next.alignment.luminousAffinity = clamp(next.alignment.luminousAffinity + 1);
  } else if (objectType === "tower" || objectType === "wall") {
    next.alignment.domination = clamp(next.alignment.domination + 3);
    next.alignment.shadowAffinity = clamp(next.alignment.shadowAffinity + 2);
    next.alignment.corruptionPressure = clamp(next.alignment.corruptionPressure + 1);
  } else if (objectType === "lamp") {
    next.alignment.resonanceStability = clamp(next.alignment.resonanceStability + 2);
  }

  return {
    ...state,
    deepStake: recomputeDerived(next),
  };
}

export function applyQuestDeepStakeInfluence(state: LifeSimState, questId: string): LifeSimState {
  const next = {
    ...state.deepStake,
    alignment: { ...state.deepStake.alignment },
    factionAffinities: { ...state.deepStake.factionAffinities },
  };

  if (questId === "first-harvest") {
    next.alignment.compassion = clamp(next.alignment.compassion + 4);
    next.alignment.luminousAffinity = clamp(next.alignment.luminousAffinity + 3);
  }
  if (questId === "mine-recon") {
    next.alignment.awakeningClarity = clamp(next.alignment.awakeningClarity + 5);
    next.factionAffinities["deep-archive"] = clamp(next.factionAffinities["deep-archive"] + 4);
  }
  if (questId === "repair-lantern") {
    next.alignment.resonanceStability = clamp(next.alignment.resonanceStability + 8);
    next.factionAffinities.dawnkeepers = clamp(next.factionAffinities.dawnkeepers + 6);
  }
  if (questId === "restore-bridge") {
    next.alignment.attunement = clamp(next.alignment.attunement + 6);
    next.alignment.luminousAffinity = clamp(next.alignment.luminousAffinity + 4);
  }
  if (questId === "north-reach") {
    next.alignment.awakeningClarity = clamp(next.alignment.awakeningClarity + 8);
    next.alignment.attunement = clamp(next.alignment.attunement + 5);
  }

  return {
    ...state,
    deepStake: recomputeDerived(next),
  };
}

export function getDeepStakeChoiceLabel(choiceId: DeepStakeChoiceId) {
  const locale = getLifeSimLocale();
  switch (choiceId) {
    case "village-recovery-vow":
      return {
        title: t({ ko: "마을 회복 서약", en: "Village Recovery Vow" }, locale),
        summary: t({ ko: "회복과 보호를 우선시해 새벽 수호단 쪽으로 기웁니다.", en: "Prioritize recovery and protection, leaning toward the Dawnkeepers." }, locale),
      };
    case "accept-luminous-guidance":
      return {
        title: t({ ko: "빛의 안내 수락", en: "Accept Luminous Guidance" }, locale),
        summary: t({ ko: "빛의 동행자와 감응해 정화와 각성을 끌어올립니다.", en: "Attune to luminous companions to raise clarity and purification." }, locale),
      };
    case "study-deep-archive":
      return {
        title: t({ ko: "심층 기록 열람", en: "Study the Deep Archive" }, locale),
        summary: t({ ko: "봉인된 기록을 연구해 각성과 공명 이해를 높입니다.", en: "Study sealed records to deepen clarity and understanding of resonance." }, locale),
      };
    case "accept-shadow-bargain":
      return {
        title: t({ ko: "그림자 거래 수락", en: "Accept the Shadow Bargain" }, locale),
        summary: t({ ko: "쉬운 힘을 얻지만 그림자 압력과 지배 성향이 커집니다.", en: "Gain easy power at the cost of rising pressure and domination." }, locale),
      };
  }
}

export function getDeepStakeSignalHint(state: DeepStakeState) {
  const locale = getLifeSimLocale();
  const visible = state.supportSignals.filter((entry) => entry.sensed);
  if (!visible.length) {
    return t({ ko: "아직 높은 차원의 흔적은 흐릿합니다. 정화와 공명을 더 쌓아야 합니다.", en: "Higher-dimensional traces are still faint. Build more purification and resonance." }, locale);
  }
  return t(visible[0].note, locale);
}
