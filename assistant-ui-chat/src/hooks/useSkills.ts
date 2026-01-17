import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import { skillsClient } from "../api";
import type { Skill, Category, CreateSkillRequest, UpdateSkillRequest } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseSkillsReturn {
  skills: Skill[];
  category: Category;
  loading: boolean;
  error: string | null;
  fetchByCategory: (categoryId: string) => Promise<void>;
  create: (request: CreateSkillRequest) => Promise<Skill>;
  update: (id: string, request: UpdateSkillRequest) => Promise<Skill>;
  remove: (id: string) => Promise<void>;
}

// =============================================================================
// Hook
// =============================================================================

export function useSkills(categoryId: any): UseSkillsReturn {
  const [skills, setSkills] = useState<Skill[]>([]);
  const [category, setCategory] = useState<Category>({} as Category);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch skills by category
  // ---------------------------------------------------------------------------
  const fetchByCategory = useCallback(async (categoryId: string) => {
    setLoading(true);
    setError(null);

    try {
      const category = await skillsClient.getByCategory(categoryId);
      setCategory(category);
      setSkills(category.skills || []);
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
    fetchByCategory(categoryId);
  }, [categoryId]);

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
    category,
    loading,
    error,
    fetchByCategory,
    create,
    update,
    remove,
  };
}