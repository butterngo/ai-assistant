import { useCallback, useEffect, useState } from "react";
import { Conversation } from "../types/Conversation";
import { apiClient } from "../api/client";
import { API_BASE } from "../config";

type BackendConversation = {
  id: string;
  name: string;
  createdAt: string;
};

// Hook to manage conversations via the /conversations API
export function useConversations(apiBase = "") {
  // prefer explicit apiBase param, otherwise default to configured base
  const base = apiBase?.replace(/\/$/, "") || API_BASE;

  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const mapFromBackend = (b: BackendConversation): Conversation => ({
    id: b.id,
    title: b.name,
    createdAt: new Date(b.createdAt),
  });

  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = (await apiClient.get(`${base}/conversations`)) as BackendConversation[];
      setConversations((data || []).map(mapFromBackend));
    } catch (e: any) {
      setError(e?.message ?? String(e));
    } finally {
      setLoading(false);
    }
  }, [base]);

  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  const createConversation = useCallback(
    async (title: string) => {
      setError(null);
      try {
        const payload = { name: title, createdAt: new Date().toISOString() };
        const created = (await apiClient.post(`${base}/conversations`, payload)) as BackendConversation;
        setConversations((prev) => [mapFromBackend(created), ...prev]);
        setActiveConversationId(created.id);
        return created.id;
      } catch (e: any) {
        setError(e?.message ?? String(e));
        throw e;
      }
    },
    [base]
  );

  const updateConversation = useCallback(
    async (id: string, title: string) => {
      setError(null);
      try {
        const payload = { name: title };
        const updated = (await apiClient.put(`${base}/conversations/${encodeURIComponent(id)}`, payload)) as BackendConversation;
        setConversations((prev) => prev.map((c) => (c.id === id ? mapFromBackend(updated) : c)));
        return mapFromBackend(updated);
      } catch (e: any) {
        setError(e?.message ?? String(e));
        throw e;
      }
    },
    [base]
  );

  const deleteConversation = useCallback(
    async (id: string) => {
      setError(null);
      try {
        await apiClient.del(`${base}/conversations/${encodeURIComponent(id)}`);
        setConversations((prev) => prev.filter((c) => c.id !== id));
        setActiveConversationId((current) => (current === id ? null : current));
      } catch (e: any) {
        setError(e?.message ?? String(e));
        throw e;
      }
    },
    [base]
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
  } as const;
}
