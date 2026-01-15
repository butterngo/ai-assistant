import { axiosClient } from "./axiosClient";
import type {
  Tool,
  ToolWithSkill,
  CreateToolRequest,
  UpdateToolRequest,
} from "../types";

export const toolsClient = {
  /**
   * Get tool by ID
   */
  async getById(id: string): Promise<ToolWithSkill> {
    const { data } = await axiosClient.get<ToolWithSkill>(`/api/tools/${id}`);
    return data;
  },

  /**
   * Get all tools by skill
   */
  async getBySkill(skillId: string): Promise<Tool[]> {
    const { data } = await axiosClient.get<Tool[]>(
      `/api/tools/by-skill/${skillId}`
    );
    return data;
  },

  /**
   * Create a new tool
   */
  async create(request: CreateToolRequest): Promise<Tool> {
    const { data } = await axiosClient.post<Tool>("/api/tools", request);
    return data;
  },

  /**
   * Update an existing tool
   */
  async update(id: string, request: UpdateToolRequest): Promise<Tool> {
    const { data } = await axiosClient.put<Tool>(`/api/tools/${id}`, request);
    return data;
  },

  /**
   * Delete a tool
   */
  async delete(id: string): Promise<void> {
    await axiosClient.delete(`/api/tools/${id}`);
  },
};