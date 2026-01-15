import type { Skill } from "./Skill";

export interface Tool {
  id: string;
  skillId: string;
  name: string;
  type: string;
  endpoint: string;
  description: string | null;
  config: Record<string, unknown> | null;
  isPrefetch: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ToolWithSkill extends Tool {
  skill?: Skill;
}

export interface CreateToolRequest {
  skillId: string;
  name: string;
  type: string;
  endpoint: string;
  description?: string | null;
  config?: Record<string, unknown>;
  isPrefetch?: boolean;
}

export interface UpdateToolRequest {
  name?: string | null;
  type?: string | null;
  endpoint?: string | null;
  description?: string | null;
  config?: Record<string, unknown>;
  isPrefetch?: boolean | null;
}