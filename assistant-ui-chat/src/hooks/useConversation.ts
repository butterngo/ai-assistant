import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import { conversationsClient } from "../api";
import type { Conversation } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseConversationsReturn {
  conversations: Conversation[];
  activethreadId: string | null;
  setActivethreadId: (id: string | null) => void;
  loading: boolean;
  error: string | null;
  fetchAll: () => Promise<void>;
  deleteConversation: (id: string) => Promise<void>;
  addConversation: (conversation: Conversation) => void;
  getConversation: (id: string) => Conversation | undefined;
}

// =============================================================================
// Hook
// =============================================================================

export function useConversations(): UseConversationsReturn {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activethreadId, setActivethreadId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch all conversations
  // ---------------------------------------------------------------------------
  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await conversationsClient.getAll({ pageSize: 100 });
      setConversations(data);
    } catch (e) {
      if (e instanceof AxiosError) {
        const message = e.response?.data?.message || e.message;
        setError(message);
      } else {
        const message = e instanceof Error ? e.message : "Unknown error";
        setError(message);
      }
      console.error("Failed to fetch conversations:", e);
    } finally {
      setLoading(false);
    }
  }, []);

  // ---------------------------------------------------------------------------
  // Initial fetch
  // ---------------------------------------------------------------------------
  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  // ---------------------------------------------------------------------------
  // Add conversation to local state (used when server creates via SSE metadata)
  // ---------------------------------------------------------------------------
  const addConversation = useCallback((conversation: Conversation) => {
    setConversations((prev) => {
      // Check if already exists
      const exists = prev.some(
        (c) => c.id === conversation.id || c.threadId === conversation.threadId
      );
      if (exists) {
        return prev;
      }
      // Add to beginning of list
      return [conversation, ...prev];
    });
  }, []);

  // ---------------------------------------------------------------------------
  // Delete conversation
  // ---------------------------------------------------------------------------
  const deleteConversation = useCallback(
    async (id: string) => {
      try {
        await conversationsClient.delete(id);
      } catch (e) {
        // Ignore 404 (already deleted)
        if (e instanceof AxiosError && e.response?.status === 404) {
          // Continue to remove from local state
        } else {
          throw e;
        }
      }

      setConversations((prev) => prev.filter((c) => c.id !== id && c.threadId !== id));

      // Clear active if deleted
      if (activethreadId === id) {
        setActivethreadId(null);
      }
    },
    [activethreadId]
  );

  // ---------------------------------------------------------------------------
  // Get single conversation by ID
  // ---------------------------------------------------------------------------
  const getConversation = useCallback(
    (id: string): Conversation | undefined => {
      return conversations.find((c) => c.id === id || c.threadId === id);
    },
    [conversations]
  );

  return {
    conversations,
    activethreadId,
    setActivethreadId,
    loading,
    error,
    fetchAll,
    deleteConversation,
    addConversation,
    getConversation,
  };
}