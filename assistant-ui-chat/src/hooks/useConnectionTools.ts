import { useState, useEffect, useCallback } from "react";
import { AxiosError } from "axios";
import { connectionToolsClient, discoveredToolsClient } from "../api";
import type {
  ConnectionTool,
  CreateConnectionToolRequest,
  UpdateConnectionToolRequest,
  DiscoveredTool,
} from "../types";

// =============================================================================
// Types
// =============================================================================

export interface UseConnectionToolsReturn {
  connections: ConnectionTool[];
  loading: boolean;
  error: string | null;
  fetchAll: () => Promise<void>;
  create: (request: CreateConnectionToolRequest) => Promise<ConnectionTool>;
  update: (id: string, request: UpdateConnectionToolRequest) => Promise<ConnectionTool>;
  remove: (id: string) => Promise<void>;
  test: (id: string) => Promise<{ isConnected: boolean; message: string }>;
  getById: (id: string) => Promise<ConnectionTool>;
  
  // Tool discovery and management
  discoverTools: (id: string) => Promise<DiscoveredTool[]>;  // ✅ Fixed return type
  getDiscoveredTools: (connectionId: string) => Promise<DiscoveredTool[]>;  // ✅ Removed unused useCache param
}

// =============================================================================
// Hook
// =============================================================================

export function useConnectionTools(): UseConnectionToolsReturn {
  const [connections, setConnections] = useState<ConnectionTool[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Fetch all connection tools
  // ---------------------------------------------------------------------------
  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await connectionToolsClient.getAll();
      setConnections(data);
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
  // Create connection tool
  // ---------------------------------------------------------------------------
  const create = useCallback(
    async (request: CreateConnectionToolRequest): Promise<ConnectionTool> => {
      const newConnection = await connectionToolsClient.create(request);
      setConnections((prev) => [...prev, newConnection]);
      return newConnection;
    },
    []
  );

  // ---------------------------------------------------------------------------
  // Update connection tool
  // ---------------------------------------------------------------------------
  const update = useCallback(
    async (
      id: string,
      request: UpdateConnectionToolRequest
    ): Promise<ConnectionTool> => {
      const updated = await connectionToolsClient.update(id, request);
      setConnections((prev) =>
        prev.map((connection) => (connection.id === id ? updated : connection))
      );
      return updated;
    },
    []
  );

  // ---------------------------------------------------------------------------
  // Delete connection tool
  // ---------------------------------------------------------------------------
  const remove = useCallback(async (id: string): Promise<void> => {
    await connectionToolsClient.delete(id);
    setConnections((prev) => prev.filter((connection) => connection.id !== id));
  }, []);

  // ---------------------------------------------------------------------------
  // Get connection by ID
  // ---------------------------------------------------------------------------
  const getById = useCallback(async (id: string): Promise<ConnectionTool> => {
    const connection = await connectionToolsClient.getById(id);
    return connection;
  }, []);

  // ---------------------------------------------------------------------------
  // Test connection
  // ---------------------------------------------------------------------------
  const test = useCallback(
    async (id: string): Promise<{ isConnected: boolean; message: string }> => {
      const result = await connectionToolsClient.test(id);
      return result;
    },
    []
  );

  // ---------------------------------------------------------------------------
  // Discover tools (fresh discovery from external MCP/OpenAPI)
  // ---------------------------------------------------------------------------
  const discoverTools = useCallback(
    async (id: string): Promise<DiscoveredTool[]> => {  // ✅ Fixed return type
      const tools = await connectionToolsClient.discoverTools(id);
      return tools;
    },
    []
  );

  // ---------------------------------------------------------------------------
  // Get discovered tools (cached from database)
  // ---------------------------------------------------------------------------
  const getDiscoveredTools = useCallback(
    async (connectionId: string): Promise<DiscoveredTool[]> => {  // ✅ Removed unused param
      const tools = await discoveredToolsClient.getByConnection(connectionId);
      return tools;
    },
    []
  );

  return {
    connections,
    loading,
    error,
    fetchAll,
    create,
    update,
    remove,
    getById,
    test,
    discoverTools,
    getDiscoveredTools,
  };
}