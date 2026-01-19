import { axiosClient } from "./axiosClient";
import type { SkillRouter, CreateSkillRouterRequest } from "../types";

// =============================================================================
// Skill Routers API Client
// =============================================================================

const API_BASE = "/api/skill-routers";

export const skillRoutersClient = {
  /**
   * Get all routing queries for a skill
   * GET /api/skill-routers?skillCode={skillCode}
   */
  getBySkillCode: async (skillCode: string): Promise<SkillRouter[]> => {
    const response = await axiosClient.get<SkillRouter[]>(API_BASE, {
      params: { skillCode },
    });
    return response.data;
  },

  /**
   * Get routing query by ID
   * GET /api/skill-routers/{id}
   */
  getById: async (id: string): Promise<SkillRouter> => {
    const response = await axiosClient.get<SkillRouter>(`${API_BASE}/${id}`);
    return response.data;
  },

  /**
   * Create a new routing query
   * POST /api/skill-routers
   */
  create: async (request: CreateSkillRouterRequest): Promise<SkillRouter> => {
    const response = await axiosClient.post<SkillRouter>(API_BASE, request);
    return response.data;
  },

  /**
   * Update a routing query
   * PUT /api/skill-routers/{id}
   */
  update: async (id: string, userQuery: string): Promise<SkillRouter> => {
    const response = await axiosClient.put<SkillRouter>(`${API_BASE}/${id}`, {
      userQuery,
    });
    return response.data;
  },

  /**
   * Delete a routing query
   * DELETE /api/skill-routers/{id}
   */
  delete: async (id: string): Promise<void> => {
    await axiosClient.delete(`${API_BASE}/${id}`);
  },
};