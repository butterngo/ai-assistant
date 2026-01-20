import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import { agentsClient } from "../api";
import type { Agent, CreateAgentRequest, UpdateAgentRequest } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseAgentsReturn {
  agents: Agent[];
  loading: boolean;
  error: string | null;
  fetchAll: () => Promise<void>;
  create: (request: CreateAgentRequest) => Promise<Agent>;
  update: (id: string, request: UpdateAgentRequest) => Promise<Agent>;
  remove: (id: string) => Promise<void>;
}

// =============================================================================
// Hook
// =============================================================================

export function useAgents(): UseAgentsReturn {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch all agents 
  // ---------------------------------------------------------------------------
  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await agentsClient.getAll();
      setAgents(data);
    } catch (e) {
      if (e instanceof AxiosError) {
        setError(e.response?.data?.message || e.message);
      } else {
        setError(e instanceof Error ? e.message : "Unknown error");
      }
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
  // Create agent   
  // ---------------------------------------------------------------------------
  const create = useCallback(async (request: CreateAgentRequest): Promise<Agent> => {
    const newAgent = await agentsClient.create(request);
    setAgents((prev) => [...prev, newAgent]);
    return newAgent;
  }, []);

  // ---------------------------------------------------------------------------
  // Update agent
  // ---------------------------------------------------------------------------
  const update = useCallback(async (id: string, request: UpdateAgentRequest): Promise<Agent> => {
    const updated = await agentsClient.update(id, request);
    setAgents((prev) =>
      prev.map((agent) => (agent.id === id ? updated : agent))
    );
    return updated;
  }, []);

  // ---------------------------------------------------------------------------
  // Delete category
  // ---------------------------------------------------------------------------
  const remove = useCallback(async (id: string): Promise<void> => {
    await agentsClient.delete(id);
    setAgents((prev) => prev.filter((agent) => agent.id !== id));
  }, []);

  return {
    agents,
    loading,
    error,
    fetchAll,
    create,
    update,
    remove,
  };
}