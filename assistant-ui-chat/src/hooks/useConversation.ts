import { useState, useEffect, useCallback } from "react";
import type { Conversation, Message } from "../types/Conversation";

// =============================================================================
// Types
// =============================================================================

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ConversationDetail extends Conversation {
  messages: Message[];
  messageCount: number;
}

export interface UseConversationsReturn {
  conversations: Conversation[];
  activeConversationId: string | null;
  setActiveConversationId: (id: string | null) => void;
  loading: boolean;
  error: string | null;
  fetchAll: () => Promise<void>;
  createConversation: (title?: string) => Promise<Conversation>;
  updateConversation: (id: string, updates: Partial<Conversation>) => void;
  deleteConversation: (id: string) => Promise<void>;
  addConversation: (conversation: Conversation) => void;
  getConversation: (id: string) => Conversation | undefined;
}

// =============================================================================
// Hook
// =============================================================================

export function useConversations(apiBase: string): UseConversationsReturn {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Fetch all conversations
  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${apiBase}/api/conversations?pageSize=100`);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data: PagedResult<Conversation> = await response.json();
      setConversations(data.items);
    } catch (e) {
      const message = e instanceof Error ? e.message : "Unknown error";
      setError(message);
      console.error("Failed to fetch conversations:", e);
    } finally {
      setLoading(false);
    }
  }, [apiBase]);

  // Initial fetch
  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  // Create a new conversation (via API - optional, since server creates on first message)
  const createConversation = useCallback(
    async (title?: string): Promise<Conversation> => {
      const response = await fetch(`${apiBase}/api/conversations`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title }),
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const conversation: Conversation = await response.json();
      setConversations((prev) => [conversation, ...prev]);
      setActiveConversationId(conversation.id);

      return conversation;
    },
    [apiBase]
  );

  // Add conversation to local state (used when server creates via SSE metadata)
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

  // Update conversation in local state
  const updateConversation = useCallback((id: string, updates: Partial<Conversation>) => {
    setConversations((prev) =>
      prev.map((conv) => {
        if (conv.id === id || conv.threadId === id) {
          return {
            ...conv,
            ...updates,
            updatedAt: new Date().toISOString(),
          };
        }
        return conv;
      })
    );
  }, []);

  // Delete conversation
  const deleteConversation = useCallback(
    async (id: string) => {
      const response = await fetch(`${apiBase}/api/conversations/${id}`, {
        method: "DELETE",
      });

      if (!response.ok && response.status !== 404) {
        throw new Error(`HTTP ${response.status}`);
      }

      setConversations((prev) => prev.filter((c) => c.id !== id && c.threadId !== id));

      // Clear active if deleted
      if (activeConversationId === id) {
        setActiveConversationId(null);
      }
    },
    [apiBase, activeConversationId]
  );

  // Get single conversation by ID
  const getConversation = useCallback(
    (id: string): Conversation | undefined => {
      return conversations.find((c) => c.id === id || c.threadId === id);
    },
    [conversations]
  );

  return {
    conversations,
    activeConversationId,
    setActiveConversationId,
    loading,
    error,
    fetchAll,
    createConversation,
    updateConversation,
    deleteConversation,
    addConversation,
    getConversation
  };
}