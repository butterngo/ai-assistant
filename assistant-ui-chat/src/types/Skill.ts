import type { Category } from "./Category";
import type { Tool } from "./Tool";

export interface Skill {
  id: string;
  categoryId: string;
  code: string;
  name: string;
  systemPrompt: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export interface SkillWithRelations extends Skill {
  category?: Category;
  tools?: Tool[];
}

export interface CreateSkillRequest {
  categoryId: string;
  code: string;
  name: string;
  systemPrompt: string;
  description: string;
}

export interface UpdateSkillRequest {
  code?: string | null;
  name?: string | null;
  systemPrompt?: string | null;
  description?: string | null;
}

export interface RouteSkillRequest {
  query: string;
}

// Skill Router (for Qdrant routing records)
export interface SkillRouter {
  id: string;
  skillCode: string;
  skillName: string;
  userQueries: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateSkillRouterRequest {
  skillCode: string;
  skillName: string;
  userQueries: string;
}

export interface UpdateSkillRouterRequest {
  userQueries: string;
}