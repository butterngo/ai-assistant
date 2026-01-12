import type { LoaderFunctionArgs } from "react-router-dom";
import { API_BASE } from "../config";
import type { ConversationDetail } from "../types/Conversation";

export interface ConversationLoaderData {
  conversation: ConversationDetail | null;
  error: string | null;
}

export async function conversationLoader({
  params,
}: LoaderFunctionArgs): Promise<ConversationLoaderData> {
  const { conversationId } = params;

  if (!conversationId) {
    return { conversation: null, error: null };
  }

  try {
    const response = await fetch(`${API_BASE}/conversations/${conversationId}`);

    if (!response.ok) {
      if (response.status === 404) {
        return { conversation: null, error: "Conversation not found" };
      }
      throw new Error(`HTTP ${response.status}`);
    }

    const conversation: ConversationDetail = await response.json();
    return { conversation, error: null };
  } catch (e) {
    const message = e instanceof Error ? e.message : "Failed to load conversation";
    console.error("Conversation loader error:", e);
    return { conversation: null, error: message };
  }
}