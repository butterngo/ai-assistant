import type { Skill } from "./Skill";

export interface Agent {
  id: string;
  code: string;
  name: string;
  description: string | null;
  skillCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface AgentWithSkills extends Agent {
  skills: Skill[];
}

export interface CreateAgentRequest {
  code: string;
  name: string;
  description?: string | null;
}

export interface UpdateAgentRequest {
  code?: string | null;
  name?: string | null;
  description?: string | null;
}