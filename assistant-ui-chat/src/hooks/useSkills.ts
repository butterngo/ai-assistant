import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import { skillsClient } from "../api";
import type { Skill, Agent, CreateSkillRequest, UpdateSkillRequest } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseSkillsReturn {
  skills: Skill[];
  agent: Agent;
  loading: boolean;
  error: string | null;
  fetchByAgent: (agentId: string) => Promise<void>;
  create: (request: CreateSkillRequest) => Promise<Skill>;
  update: (id: string, request: UpdateSkillRequest) => Promise<Skill>;
  remove: (id: string) => Promise<void>;
}

// =============================================================================
// Hook
// =============================================================================

export function useSkills(agentId?: string): UseSkillsReturn {
  const [skills, setSkills] = useState<Skill[]>([]);
  const [agent, setAgent] = useState<Agent>({} as Agent);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch skills by agent
  // ---------------------------------------------------------------------------
  const fetchByAgent = useCallback(async (agentId: string) => {
    setLoading(true);
    setError(null);

    try {
      const data = await skillsClient.getByAgent(agentId);
      setAgent(data);
      setSkills(data.skills || []);
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
    if(agentId == null) return;
    fetchByAgent(agentId);
  }, [agentId]);

  // ---------------------------------------------------------------------------
  // Create skill
  // ---------------------------------------------------------------------------
  const create = useCallback(async (request: CreateSkillRequest): Promise<Skill> => {
    const newSkill = await skillsClient.create(request);
    setSkills((prev) => [...prev, newSkill]);
    return newSkill;
  }, []);

  // ---------------------------------------------------------------------------
  // Update skill
  // ---------------------------------------------------------------------------
  const update = useCallback(async (id: string, request: UpdateSkillRequest): Promise<Skill> => {
    const updated = await skillsClient.update(id, request);
    setSkills((prev) => prev.map((skill) => (skill.id === id ? updated : skill)));
    return updated;
  }, []);

  // ---------------------------------------------------------------------------
  // Delete skill
  // ---------------------------------------------------------------------------
  const remove = useCallback(async (id: string): Promise<void> => {
    await skillsClient.delete(id);
    setSkills((prev) => prev.filter((skill) => skill.id !== id));
  }, []);

  return {
    skills,
    agent,
    loading,
    error,
    fetchByAgent,
    create,
    update,
    remove,
  };
}