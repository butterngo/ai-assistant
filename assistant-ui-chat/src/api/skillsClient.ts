import { axiosClient } from "./axiosClient";
import type {
  Skill,
  AgentWithSkills,
  SkillWithRelations,
  CreateSkillRequest,
  UpdateSkillRequest,
  RouteSkillRequest,
} from "../types";

export const skillsClient = {
  /**
   * Get skill by ID
   */
  async getById(id: string): Promise<SkillWithRelations> {
    const { data } = await axiosClient.get<SkillWithRelations>(
      `/api/skills/${id}`
    );
    return data;
  },

  /**
   * Get all skills by agent
   */
  async getByAgent(agentId: string): Promise<AgentWithSkills> {
    const { data } = await axiosClient.get<AgentWithSkills>(
      `/api/skills/by-agent/${agentId}`
    );
    return data;
  },

  /**
   * Create a new skill
   */
  async create(request: CreateSkillRequest): Promise<Skill> {
    const { data } = await axiosClient.post<Skill>("/api/skills", request);
    return data;
  },

  /**
   * Update an existing skill
   */
  async update(id: string, request: UpdateSkillRequest): Promise<Skill> {
    const { data } = await axiosClient.put<Skill>(`/api/skills/${id}`, request);
    return data;
  },

  /**
   * Delete a skill
   */
  async delete(id: string): Promise<void> {
    await axiosClient.delete(`/api/skills/${id}`);
  },

  /**
   * Route a query to the best matching skill
   */
  async route(request: RouteSkillRequest): Promise<Skill> {
    const { data } = await axiosClient.post<Skill>("/api/skills/route", request);
    return data;
  },
};