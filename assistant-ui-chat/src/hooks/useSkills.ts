import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import { skillsClient, categoriesClient } from "../api";
import type { Skill, Category, CreateSkillRequest, UpdateSkillRequest } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseSkillsReturn {
  skills: Skill[];
  categories: Category[];
  loading: boolean;
  error: string | null;
  fetchAll: () => Promise<void>;
  fetchByCategory: (categoryId: string) => Promise<void>;
  create: (request: CreateSkillRequest) => Promise<Skill>;
  update: (id: string, request: UpdateSkillRequest) => Promise<Skill>;
  remove: (id: string) => Promise<void>;
}

// =============================================================================
// Hook
// =============================================================================

export function useSkills(): UseSkillsReturn {
  const [skills, setSkills] = useState<Skill[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch all skills and categories
  // ---------------------------------------------------------------------------
  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      // Fetch categories first
      const categoriesData = await categoriesClient.getAll();
      setCategories(categoriesData);

      // Fetch skills for all categories
      const skillsPromises = categoriesData.map((cat) =>
        skillsClient.getByCategory(cat.id)
      );
      const skillsResults = await Promise.all(skillsPromises);
      const allSkills = skillsResults.flat();
      setSkills(allSkills);
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
  // Fetch skills by category
  // ---------------------------------------------------------------------------
  const fetchByCategory = useCallback(async (categoryId: string) => {
    setLoading(true);
    setError(null);

    try {
      const data = await skillsClient.getByCategory(categoryId);
      setSkills(data);
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
    categories,
    loading,
    error,
    fetchAll,
    fetchByCategory,
    create,
    update,
    remove,
  };
}