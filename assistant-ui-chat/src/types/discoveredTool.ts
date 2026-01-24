
export interface DiscoveredTool {
  id: string;
  connectionToolId: string;
  name: string;
  description: string;
  toolSchema: object; // Full AITool JSON
  discoveredAt: string; // ISO datetime
  lastVerifiedAt: string; // ISO datetime
  isAvailable: boolean;
}

export interface UpdateDiscoveredToolRequest {
  description?: string;
  isAvailable?: boolean;
}