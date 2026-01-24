import { axiosClient } from "./axiosClient";
import type { DiscoveredTool, UpdateDiscoveredToolRequest } from "../types";

/**
 * Cache status response from backend
 */
export interface CacheStatusResponse {
  isStale: boolean;
  maxAge: string; // TimeSpan format (e.g., "24:00:00")
  message: string;
}

/**
 * Discovered Tools API Client
 * 
 * Manages cached AI tools discovered from connections.
 * These are stored in the database after running discovery.
 */
export const discoveredToolsClient = {
  /**
   * Get discovered tool by ID
   * GET /api/discovered-tools/{id}
   */
  async getById(id: string): Promise<DiscoveredTool> {
    const response = await axiosClient.get(`/api/discovered-tools/${id}`);
    return response.data;
  },

  /**
   * Get all discovered tools for a connection (cached)
   * GET /api/discovered-tools/by-connection/{connectionToolId}
   */
  async getByConnection(connectionToolId: string): Promise<DiscoveredTool[]> {
    const response = await axiosClient.get(`/api/discovered-tools/by-connection/${connectionToolId}`);
    return response.data;
  },

  /**
   * Get only available tools for a connection
   * GET /api/discovered-tools/by-connection/{connectionToolId}/available
   */
  async getAvailableByConnection(connectionToolId: string): Promise<DiscoveredTool[]> {
    const response = await axiosClient.get(`/api/discovered-tools/by-connection/${connectionToolId}/available`);
    return response.data;
  },

  /**
   * Get discovered tool by connection and name
   * GET /api/discovered-tools/by-connection/{connectionToolId}/by-name/{name}
   */
  async getByName(connectionToolId: string, name: string): Promise<DiscoveredTool> {
    const response = await axiosClient.get(`/api/discovered-tools/by-connection/${connectionToolId}/by-name/${name}`);
    return response.data;
  },

  /**
   * Update discovered tool (description, availability, etc.)
   * PUT /api/discovered-tools/{id}
   */
  async update(id: string, request: UpdateDiscoveredToolRequest): Promise<DiscoveredTool> {
    const response = await axiosClient.put(`/api/discovered-tools/${id}`, request);
    return response.data;
  },

  /**
   * Delete discovered tool
   * DELETE /api/discovered-tools/{id}
   */
  async delete(id: string): Promise<void> {
    await axiosClient.delete(`/api/discovered-tools/${id}`);
  },

  /**
   * Clear all cached tools for a connection
   * POST /api/discovered-tools/by-connection/{connectionToolId}/clear-cache
   */
  async clearCache(connectionToolId: string): Promise<void> {
    await axiosClient.post(`/api/discovered-tools/by-connection/${connectionToolId}/clear-cache`);
  },

  /**
   * Mark a tool as unavailable
   * PUT /api/discovered-tools/by-connection/{connectionToolId}/by-name/{toolName}/mark-unavailable
   */
  async markAsUnavailable(connectionToolId: string, toolName: string): Promise<void> {
    await axiosClient.put(`/api/discovered-tools/by-connection/${connectionToolId}/by-name/${toolName}/mark-unavailable`);
  },

  /**
   * Mark a tool as available
   * PUT /api/discovered-tools/by-connection/{connectionToolId}/by-name/{toolName}/mark-available
   */
  async markAsAvailable(connectionToolId: string, toolName: string): Promise<void> {
    await axiosClient.put(`/api/discovered-tools/by-connection/${connectionToolId}/by-name/${toolName}/mark-available`);
  },

  /**
   * Check if cache is stale (>24 hours old)
   * GET /api/discovered-tools/by-connection/{connectionToolId}/cache-status
   */
  async checkCacheStatus(connectionToolId: string): Promise<CacheStatusResponse> {
    const response = await axiosClient.get(`/api/discovered-tools/by-connection/${connectionToolId}/cache-status`);
    return response.data;
  },
};