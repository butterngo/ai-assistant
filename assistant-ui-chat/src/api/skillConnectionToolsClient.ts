import { axiosClient } from "./axiosClient";
import type { ConnectionTool, DiscoveredTool } from "../types";

/**
 * Skill-Connection Tools Junction API Client
 * 
 * Manages many-to-many relationships between Skills and Connection Tools
 */
export const skillConnectionToolsClient = {
  /**
   * Link a connection tool to a skill
   * POST /api/skill-connection-tools
   */
  async link(skillId: string, connectionToolId: string): Promise<void> {
    await axiosClient.post("/api/skill-connection-tools", { skillId, connectionToolId });
  },

  /**
   * Unlink a connection from a skill
   * DELETE /api/skill-connection-tools?skillId={}&connectionToolId={}
   */
  async unlink(skillId: string, connectionToolId: string): Promise<void> {
    await axiosClient.delete("/api/skill-connection-tools", {
      params: { skillId, connectionToolId },
    });
  },

  /**
   * Get all connections for a skill
   * GET /api/skill-connection-tools/by-skill/{skillId}/connections
   */
  async getConnectionsBySkill(skillId: string): Promise<ConnectionTool[]> {
    const response = await axiosClient.get(`/api/skill-connection-tools/by-skill/${skillId}/connections`);
    return response.data;
  },

  /**
   * Get all tools available for a skill (aggregated from all connections)
   * GET /api/skill-connection-tools/by-skill/{skillId}/tools
   */
  async getToolsForSkill(skillId: string, useCache: boolean = true): Promise<DiscoveredTool[]> {
    const response = await axiosClient.get(`/api/skill-connection-tools/by-skill/${skillId}/tools`, {
      params: { useCache },
    });
    return response.data;
  },
};