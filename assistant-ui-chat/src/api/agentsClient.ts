import { axiosClient } from "./axiosClient";
import type {
  Agent,
  CreateAgentRequest,
  UpdateAgentRequest,
} from "../types";

export const agentsClient = {
  /**
   * Get all agents
   */
  async getAll(): Promise<Agent[]> {
    const { data } = await axiosClient.get<Agent[]>("/api/agents");
    return data;
  },

  /**
   * Get agent by ID
   */
  async getById(id: string): Promise<Agent> {
    const { data } = await axiosClient.get<Agent>(`/api/agents/${id}`);
    return data;
  },

  /**
   * Create a new agent
   */
  async create(request: CreateAgentRequest): Promise<Agent> {
    const { data } = await axiosClient.post<Agent>(
      "/api/agents",
      request
    );
    return data;
  },

  /**
   * Update an existing agent
   */
  async update(id: string, request: UpdateAgentRequest): Promise<Agent> {
    const { data } = await axiosClient.put<Agent>(
      `/api/agents/${id}`,
      request
    );
    return data;
  },

  /**
   * Delete a category
   */
  async delete(id: string): Promise<void> {
    await axiosClient.delete(`/api/agents/${id}`);
  },
};