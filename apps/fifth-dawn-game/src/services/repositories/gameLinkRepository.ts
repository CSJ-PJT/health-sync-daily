import type { GameLinkBundle } from "@health-sync/shared-types";
import { getGameSideLinkBundle } from "@health-sync/game-link-sdk";
import { FIFTH_DAWN_PRODUCT_KEY } from "@/config/gameProduct";
import { getSupabaseClient } from "@/integrations/supabase/client";

const LINK_TOKEN_KEY = "fifth_dawn_game_link_token";
const GAME_ACCOUNT_ID_KEY = "fifth_dawn_game_account_id";

export function getStoredGameLinkToken() {
  return localStorage.getItem(LINK_TOKEN_KEY) || "";
}

export function setStoredGameLinkToken(token: string) {
  localStorage.setItem(LINK_TOKEN_KEY, token);
}

export function clearStoredGameLinkToken() {
  localStorage.removeItem(LINK_TOKEN_KEY);
}

export function getStoredGameAccountId() {
  return localStorage.getItem(GAME_ACCOUNT_ID_KEY) || "";
}

export function setStoredGameAccountId(gameAccountId: string) {
  localStorage.setItem(GAME_ACCOUNT_ID_KEY, gameAccountId);
}

export function clearStoredGameAccountId() {
  localStorage.removeItem(GAME_ACCOUNT_ID_KEY);
}

export async function loadDerivedGameLinkBundle(token?: string, gameAccountId?: string): Promise<GameLinkBundle | null> {
  const linkToken = token || getStoredGameLinkToken();
  const resolvedGameAccountId = gameAccountId || getStoredGameAccountId();
  if (!linkToken || !resolvedGameAccountId) return null;

  const supabase = getSupabaseClient();
  if (!supabase) return null;

  try {
    return await getGameSideLinkBundle(supabase as never, linkToken, resolvedGameAccountId, FIFTH_DAWN_PRODUCT_KEY);
  } catch {
    return null;
  }
}
