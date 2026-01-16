import type { Skill } from "./Skill";

export interface Category {
  id: string;
  code: string;
  name: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CategoryWithSkills extends Category {
  skills: Skill[];
}

export interface CreateCategoryRequest {
  code: string;
  name: string;
  description?: string | null;
}

export interface UpdateCategoryRequest {
  code?: string | null;
  name?: string | null;
  description?: string | null;
}