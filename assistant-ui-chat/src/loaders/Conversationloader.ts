import type { LoaderFunctionArgs } from "react-router-dom";
import { conversationsClient } from "../api";
import type { ConversationDetail } from "../types";
import { AxiosError } from "axios";

export interface ConversationLoaderData {
  conversation: ConversationDetail | null;
  error: string | null;
}

export async function conversationLoader({
  params,
}: LoaderFunctionArgs): Promise<ConversationLoaderData> {
  const { threadId } = params;

  if (!threadId) {
    return { conversation: null, error: null };
  }

  try {
    const conversation = await conversationsClient.getById(threadId);
    return { conversation, error: null };
  } catch (e) {
    if (e instanceof AxiosError) {
      if (e.response?.status === 404) {
        return { conversation: null, error: "Conversation not found" };
      }
      const message = e.response?.data?.message || e.message;
      return { conversation: null, error: message };
    }

    const message = e instanceof Error ? e.message : "Failed to load conversation";
    return { conversation: null, error: message };
  }
}