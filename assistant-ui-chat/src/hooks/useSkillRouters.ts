import { useState, useCallback } from "react";
import { skillRoutersClient } from "../api";
import type { SkillRouter, CreateSkillRouterRequest } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseSkillRoutersReturn {
  routers: SkillRouter[];
  loading: boolean;
  error: string | null;
  fetchBySkillCode: (skillCode: string) => Promise<void>;
  create: (request: CreateSkillRouterRequest) => Promise<SkillRouter>;
  remove: (id: string) => Promise<void>;
  clear: () => void;
}

// =============================================================================
// Hook
// =============================================================================

export function useSkillRouters(): UseSkillRoutersReturn {
  const [routers, setRouters] = useState<SkillRouter[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch routers by skill code
  // ---------------------------------------------------------------------------
  const fetchBySkillCode = useCallback(async (skillCode: string) => {
    setLoading(true);
    setError(null);
    try {
      const data = await skillRoutersClient.getBySkillCode(skillCode);
      setRouters(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load routing queries");
    } finally {
      setLoading(false);
    }
  }, []);

  // ---------------------------------------------------------------------------
  // Create router
  // ---------------------------------------------------------------------------
  const create = useCallback(async (request: CreateSkillRouterRequest): Promise<SkillRouter> => {
    const created = await skillRoutersClient.create(request);
    setRouters((prev) => [...prev, created]);
    return created;
  }, []);

  // ---------------------------------------------------------------------------
  // Remove router
  // ---------------------------------------------------------------------------
  const remove = useCallback(async (id: string): Promise<void> => {
    await skillRoutersClient.delete(id);
    setRouters((prev) => prev.filter((r) => r.id !== id));
  }, []);

  // ---------------------------------------------------------------------------
  // Clear routers (when modal closes)
  // ---------------------------------------------------------------------------
  const clear = useCallback(() => {
    setRouters([]);
    setError(null);
  }, []);

  return {
    routers,
    loading,
    error,
    fetchBySkillCode,
    create,
    remove,
    clear,
  };
}