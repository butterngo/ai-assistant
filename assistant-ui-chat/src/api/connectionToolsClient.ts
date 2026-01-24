import { axiosClient } from "./axiosClient";
import type {
  ConnectionTool,
  CreateConnectionToolRequest,
  UpdateConnectionToolRequest,
} from "../types";

const API_BASE = "/api/connection-tools";

export const connectionToolsClient = {
  /**
   * Get all connection tools
   */
  async getAll(): Promise<ConnectionTool[]> {
    const response = await axiosClient.get<ConnectionTool[]>(API_BASE);
    return response.data;
  },

  /**
   * Get active connection tools only
   */
  async getActive(): Promise<ConnectionTool[]> {
    const response = await axiosClient.get<ConnectionTool[]>(`${API_BASE}/active`);
    return response.data;
  },

  /**
   * Get connection tool by ID
   */
  async getById(id: string): Promise<ConnectionTool> {
    const response = await axiosClient.get<ConnectionTool>(`${API_BASE}/${id}`);
    return response.data;
  },

  /**
   * Get connection tool by name
   */
  async getByName(name: string): Promise<ConnectionTool> {
    const response = await axiosClient.get<ConnectionTool>(`${API_BASE}/by-name/${name}`);
    return response.data;
  },

  /**
   * Create a new connection tool
   */
  async create(request: CreateConnectionToolRequest): Promise<ConnectionTool> {
    const response = await axiosClient.post<ConnectionTool>(API_BASE, request);
    return response.data;
  },

  /**
   * Update an existing connection tool
   */
  async update(
    id: string,
    request: UpdateConnectionToolRequest
  ): Promise<ConnectionTool> {
    const response = await axiosClient.put<ConnectionTool>(`${API_BASE}/${id}`, request);
    return response.data;
  },

  /**
   * Delete a connection tool
   */
  async delete(id: string): Promise<void> {
    await axiosClient.delete(`${API_BASE}/${id}`);
  },

  /**
   * Test connection
   */
  async test(id: string): Promise<{ isConnected: boolean; message: string }> {
    const response = await axiosClient.post<{ isConnected: boolean; message: string }>(
      `${API_BASE}/${id}/test`
    );
    return response.data;
  },

  /**
   * Discover tools from a connection (fresh discovery)
   */
  async discoverTools(id: string): Promise<any[]> {
    const response = await axiosClient.post<any[]>(`${API_BASE}/${id}/discover`);
    return response.data;
  },

  /**
   * Get tools for a connection (uses cache)
   */
  async getTools(id: string, useCache: boolean = true): Promise<any[]> {
    const response = await axiosClient.get<any[]>(
      `${API_BASE}/${id}/tools?useCache=${useCache}`
    );
    return response.data;
  },
};