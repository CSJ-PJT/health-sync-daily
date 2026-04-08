import type { GameLinkBundle } from "@health-sync/shared-types";
import { getGameSideLinkBundle } from "@health-sync/game-link-sdk";
import { supabase } from "@/integrations/supabase/client";

const LINK_TOKEN_KEY = "fifth_dawn_game_link_token";

export function getStoredGameLinkToken() {
  return localStorage.getItem(LINK_TOKEN_KEY) || "";
}

export function setStoredGameLinkToken(token: string) {
  localStorage.setItem(LINK_TOKEN_KEY, token);
}

export function clearStoredGameLinkToken() {
  localStorage.removeItem(LINK_TOKEN_KEY);
}

export async function loadDerivedGameLinkBundle(token?: string): Promise<GameLinkBundle | null> {
  const linkToken = token || getStoredGameLinkToken();
  if (!linkToken) return null;

  try {
    return await getGameSideLinkBundle(supabase as never, linkToken);
  } catch {
    return null;
  }
}
