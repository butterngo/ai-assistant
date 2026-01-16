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