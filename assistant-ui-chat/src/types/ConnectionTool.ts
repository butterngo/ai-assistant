export type ConnectionToolType = 'mcp_stdio' | 'mcp_http' | 'openapi';

export interface ConnectionTool {
  id: string;
  name: string;
  type: ConnectionToolType;
  description?: string;
  endpoint?: string;
  command?: string;
  config?: Record<string, any>;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  
  // Computed/joined data
  discoveredToolsCount?: number;
  lastTestedAt?: string;
  lastTestStatus?: 'success' | 'failed' | 'never';
}

export interface CreateConnectionToolRequest {
  name: string;
  type: ConnectionToolType;
  description?: string;
  endpoint?: string;
  command?: string;
  config?: Record<string, any>;
  isActive?: boolean;
}

export interface UpdateConnectionToolRequest {
  name?: string;
  type?: ConnectionToolType;
  description?: string;
  endpoint?: string;
  command?: string;
  config?: Record<string, any>;
  isActive?: boolean;
}