import type { Skill } from "./Skill";

export interface Category {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CategoryWithSkills extends Category {
  skills: Skill[];
}

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
}

export interface UpdateCategoryRequest {
  name?: string | null;
  description?: string | null;
}