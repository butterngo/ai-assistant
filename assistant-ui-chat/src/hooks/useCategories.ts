import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import { categoriesClient } from "../api";
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseCategoriesReturn {
  categories: Category[];
  loading: boolean;
  error: string | null;
  fetchAll: () => Promise<void>;
  create: (request: CreateCategoryRequest) => Promise<Category>;
  update: (id: string, request: UpdateCategoryRequest) => Promise<Category>;
  remove: (id: string) => Promise<void>;
}

// =============================================================================
// Hook
// =============================================================================

export function useCategories(): UseCategoriesReturn {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch all categories
  // ---------------------------------------------------------------------------
  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await categoriesClient.getAll();
      setCategories(data);
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
  // Create category
  // ---------------------------------------------------------------------------
  const create = useCallback(async (request: CreateCategoryRequest): Promise<Category> => {
    const newCategory = await categoriesClient.create(request);
    setCategories((prev) => [...prev, newCategory]);
    return newCategory;
  }, []);

  // ---------------------------------------------------------------------------
  // Update category
  // ---------------------------------------------------------------------------
  const update = useCallback(async (id: string, request: UpdateCategoryRequest): Promise<Category> => {
    const updated = await categoriesClient.update(id, request);
    setCategories((prev) =>
      prev.map((cat) => (cat.id === id ? updated : cat))
    );
    return updated;
  }, []);

  // ---------------------------------------------------------------------------
  // Delete category
  // ---------------------------------------------------------------------------
  const remove = useCallback(async (id: string): Promise<void> => {
    await categoriesClient.delete(id);
    setCategories((prev) => prev.filter((cat) => cat.id !== id));
  }, []);

  return {
    categories,
    loading,
    error,
    fetchAll,
    create,
    update,
    remove,
  };
}