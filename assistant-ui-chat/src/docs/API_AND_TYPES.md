# API Client & Types Documentation

## Overview

This document describes the API client layer and TypeScript types used in the ICHIBA SENSEI application.

---

## Table of Contents

- [Folder Structure](#folder-structure)
- [API Clients](#api-clients)
  - [axiosClient](#axiosclient)
  - [conversationsClient](#conversationsclient)
  - [categoriesClient](#categoriesclient)
  - [skillsClient](#skillsclient)
  - [toolsClient](#toolsclient)
- [Types](#types)
  - [Conversation](#conversation)
  - [Category](#category)
  - [Skill](#skill)
  - [Tool](#tool)
  - [ChatRequest](#chatrequest)
  - [PagedResult](#pagedresult)
- [Configuration Pages](#configuration-pages)
- [Usage Examples](#usage-examples)

---

## Folder Structure
```
src/
├── api/
│   ├── index.ts              # Re-exports all clients
│   ├── axiosClient.ts        # Base axios instance
│   ├── conversationsClient.ts
│   ├── categoriesClient.ts
│   ├── skillsClient.ts
│   └── toolsClient.ts
│
├── types/
│   ├── index.ts              # Re-exports all types
│   ├── Conversation.ts
│   ├── Category.ts
│   ├── Skill.ts
│   ├── Tool.ts
│   ├── ChatRequest.ts
│   └── PagedResult.ts
│
└── pages/
    ├── CategoriesPage.tsx
    ├── SkillsPage.tsx
    ├── ToolsPage.tsx
    └── KnowledgeBasePage.tsx
```

---

## API Clients

### axiosClient

Base axios instance with interceptors for all API calls.

**File:** `src/api/axiosClient.ts`
```typescript
import axios from "axios";
import { API_BASE } from "../config";

export const axiosClient = axios.create({
  baseURL: API_BASE,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});
```

**Features:**
- Base URL configuration
- Request/response interceptors
- Global error handling
- Timeout configuration

---

### conversationsClient

Manages conversation threads.

**File:** `src/api/conversationsClient.ts`

| Method | Endpoint | Description |
|--------|----------|-------------|
| `getAll(params?)` | `GET /api/conversations` | Get paginated conversations |
| `getById(id)` | `GET /api/conversations/:id` | Get conversation with messages |
| `delete(id)` | `DELETE /api/conversations/:id` | Delete conversation |

**Usage:**
```typescript
import { conversationsClient } from "../api";

// Get all conversations
const conversations = await conversationsClient.getAll({ page: 1, pageSize: 20 });

// Get single conversation with messages
const detail = await conversationsClient.getById("uuid-here");

// Delete conversation
await conversationsClient.delete("uuid-here");
```

---

### categoriesClient

Manages skill categories (full CRUD).

**File:** `src/api/categoriesClient.ts`

| Method | Endpoint | Description |
|--------|----------|-------------|
| `getAll()` | `GET /api/categories` | Get all categories |
| `getById(id)` | `GET /api/categories/:id` | Get category by ID |
| `create(request)` | `POST /api/categories` | Create new category |
| `update(id, request)` | `PUT /api/categories/:id` | Update category |
| `delete(id)` | `DELETE /api/categories/:id` | Delete category |

**Usage:**
```typescript
import { categoriesClient } from "../api";

// Get all
const categories = await categoriesClient.getAll();

// Create
const newCategory = await categoriesClient.create({
  name: "Frontend",
  description: "Frontend development skills",
});

// Update
const updated = await categoriesClient.update("uuid", {
  name: "Frontend Development",
});

// Delete
await categoriesClient.delete("uuid");
```

---

### skillsClient

Manages skills within categories.

**File:** `src/api/skillsClient.ts`

| Method | Endpoint | Description |
|--------|----------|-------------|
| `getById(id)` | `GET /api/skills/:id` | Get skill by ID |
| `getByCategory(categoryId)` | `GET /api/skills/by-category/:categoryId` | Get skills in category |
| `create(request)` | `POST /api/skills` | Create new skill |
| `update(id, request)` | `PUT /api/skills/:id` | Update skill |
| `delete(id)` | `DELETE /api/skills/:id` | Delete skill |
| `route(request)` | `POST /api/skills/route` | Route query to best skill |

**Usage:**
```typescript
import { skillsClient } from "../api";

// Get skills by category
const skills = await skillsClient.getByCategory("category-uuid");

// Create skill
const newSkill = await skillsClient.create({
  categoryId: "category-uuid",
  name: "React Development",
  systemPrompt: "You are a React expert...",
  description: "Helps with React development",
});

// Route query to best skill
const matchedSkill = await skillsClient.route({
  query: "How do I create a React component?",
});
```

---

### toolsClient

Manages tools attached to skills.

**File:** `src/api/toolsClient.ts`

| Method | Endpoint | Description |
|--------|----------|-------------|
| `getById(id)` | `GET /api/tools/:id` | Get tool by ID |
| `getBySkill(skillId)` | `GET /api/tools/by-skill/:skillId` | Get tools for skill |
| `create(request)` | `POST /api/tools` | Create new tool |
| `update(id, request)` | `PUT /api/tools/:id` | Update tool |
| `delete(id)` | `DELETE /api/tools/:id` | Delete tool |

**Usage:**
```typescript
import { toolsClient } from "../api";

// Get tools for a skill
const tools = await toolsClient.getBySkill("skill-uuid");

// Create tool
const newTool = await toolsClient.create({
  skillId: "skill-uuid",
  name: "Weather API",
  type: "api",
  endpoint: "https://api.weather.com/v1",
  description: "Fetches weather data",
  isPrefetch: false,
});
```

---

## Types

### Conversation

**File:** `src/types/Conversation.ts`
```typescript
export interface Conversation {
  id: string;
  threadId: string;
  title: string | null;
  createdAt: string;
  updatedAt: string;
  userId?: string | null;
}

export interface Message {
  id: string;
  messageId: string;
  role: string;
  content: string;
  createdAt: string;
  sequenceNumber: number;
}

export interface ConversationDetail extends Conversation {
  messages: Message[];
  messageCount: number;
}

export interface GetConversationsParams {
  page?: number;
  pageSize?: number;
}
```

---

### Category

**File:** `src/types/Category.ts`
```typescript
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
```

---

### Skill

**File:** `src/types/Skill.ts`
```typescript
export interface Skill {
  id: string;
  categoryId: string;
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
  name: string;
  systemPrompt: string;
  description: string;
}

export interface UpdateSkillRequest {
  name?: string | null;
  systemPrompt?: string | null;
  description?: string | null;
}

export interface RouteSkillRequest {
  query: string;
}
```

---

### Tool

**File:** `src/types/Tool.ts`
```typescript
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
```

---

### ChatRequest

**File:** `src/types/ChatRequest.ts`
```typescript
export interface ChatRequest {
  message: string;
  conversationId?: string;
}

export interface ChatMetadata {
  conversationId: string;
  title?: string | null;
  isNewConversation?: boolean;
}

export interface ChatData {
  conversationId: string;
  text: string;
}

export interface ChatDone {
  conversationId: string;
  title?: string | null;
}

export interface ChatError {
  error: string;
  code: string;
}
```

---

### PagedResult

**File:** `src/types/PagedResult.ts`
```typescript
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

---

## Configuration Pages

Settings pages for managing the skill system.

### Routes

| Path | Page | Description |
|------|------|-------------|
| `/settings/categories` | CategoriesPage | Manage skill categories |
| `/settings/skills` | SkillsPage | Manage skills |
| `/settings/tools` | ToolsPage | Manage tools |
| `/settings/knowledge-base` | KnowledgeBasePage | Manage knowledge base |
| `/settings/profile` | ProfilePage | User profile settings |

### Domain Model
```
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│  Category   │ 1──N │    Skill    │ 1──N │    Tool     │
├─────────────┤      ├─────────────┤      ├─────────────┤
│ id          │      │ id          │      │ id          │
│ name        │      │ categoryId  │      │ skillId     │
│ description │      │ name        │      │ name        │
│ createdAt   │      │ systemPrompt│      │ type        │
│ updatedAt   │      │ description │      │ endpoint    │
└─────────────┘      │ createdAt   │      │ config      │
                     │ updatedAt   │      │ isPrefetch  │
                     └─────────────┘      │ createdAt   │
                                          │ updatedAt   │
                                          └─────────────┘
```

### Example: Category → Skill → Tool
```
Category: "E-commerce"
  └── Skill: "Product Search"
        ├── systemPrompt: "You help users find products..."
        └── Tools:
              ├── Tool: "Product API" (type: api, isPrefetch: true)
              └── Tool: "Price Checker" (type: api, isPrefetch: false)
```

---

## Usage Examples

### Import API Clients
```typescript
import {
  conversationsClient,
  categoriesClient,
  skillsClient,
  toolsClient,
} from "../api";
```

### Import Types
```typescript
import type {
  Conversation,
  ConversationDetail,
  Category,
  Skill,
  Tool,
  PagedResult,
} from "../types";
```

### Full CRUD Example (Categories)
```typescript
import { categoriesClient } from "../api";
import type { Category, CreateCategoryRequest } from "../types";

// List
const categories: Category[] = await categoriesClient.getAll();

// Create
const createRequest: CreateCategoryRequest = {
  name: "Customer Support",
  description: "Skills for customer support",
};
const newCategory = await categoriesClient.create(createRequest);

// Read
const category = await categoriesClient.getById(newCategory.id);

// Update
const updated = await categoriesClient.update(newCategory.id, {
  description: "Updated description",
});

// Delete
await categoriesClient.delete(newCategory.id);
```

### Error Handling
```typescript
import { AxiosError } from "axios";
import { categoriesClient } from "../api";

try {
  const category = await categoriesClient.getById("invalid-uuid");
} catch (e) {
  if (e instanceof AxiosError) {
    if (e.response?.status === 404) {
      console.log("Category not found");
    } else {
      console.log("API error:", e.message);
    }
  }
}
```

---

## API Base URL

Configured in `src/config.ts`:
```typescript
export const API_BASE = "http://localhost:5050";
export const API_ENDPOINT = `${API_BASE}/chat/stream`;
```

---

## Notes

1. All API clients use the shared `axiosClient` instance
2. Types match the backend OpenAPI specification
3. `PagedResult<T>` is used for paginated endpoints
4. `isPrefetch` on Tool indicates if data should be fetched before chat
5. `route()` on skillsClient finds the best matching skill for a query