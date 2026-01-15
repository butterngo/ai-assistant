import { axiosClient } from "./axiosClient";
import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "../types";

export const categoriesClient = {
  /**
   * Get all categories
   */
  async getAll(): Promise<Category[]> {
    const { data } = await axiosClient.get<Category[]>("/api/categories");
    return data;
  },

  /**
   * Get category by ID
   */
  async getById(id: string): Promise<Category> {
    const { data } = await axiosClient.get<Category>(`/api/categories/${id}`);
    return data;
  },

  /**
   * Create a new category
   */
  async create(request: CreateCategoryRequest): Promise<Category> {
    const { data } = await axiosClient.post<Category>(
      "/api/categories",
      request
    );
    return data;
  },

  /**
   * Update an existing category
   */
  async update(id: string, request: UpdateCategoryRequest): Promise<Category> {
    const { data } = await axiosClient.put<Category>(
      `/api/categories/${id}`,
      request
    );
    return data;
  },

  /**
   * Delete a category
   */
  async delete(id: string): Promise<void> {
    await axiosClient.delete(`/api/categories/${id}`);
  },
};