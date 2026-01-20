# Category to Agent Rename - Complete Change List

## Overview
This document lists ALL files, methods, and lines that must be changed to rename "Category" to "Agent" throughout the codebase.

**Total Changes Required:** 380+ individual changes across 40+ files

---

## Files That Need Renaming

### Backend Files
1. `Agent.Core\Entities\CategoryEntity.cs` → **`AgentEntity.cs`**
2. `Agent.Core\Implementations\Services\CategoryService.cs` → **`AgentService.cs`**
3. `Agent.Core\Abstractions\Services\ICategoryService.cs` → **`IAgentService.cs`**
4. `Agent.Api\Endpoints\CategoryEndPoint.cs` → **`AgentEndPoint.cs`**
5. `Agent.Api\Models\CategoryRequest.cs` → **`AgentRequest.cs`**

### Frontend Files
6. `assistant-ui-chat\src\api\categoriesClient.ts` → **`agentsClient.ts`**
7. `assistant-ui-chat\src\types\Category.ts` → **`Agent.ts`**
8. `assistant-ui-chat\src\hooks\useCategories.ts` → **`useAgents.ts`**
9. `assistant-ui-chat\src\pages\CategoriesPage\CategoriesPage.tsx` → **`AgentsPage\AgentsPage.tsx`**
10. `assistant-ui-chat\src\pages\CategoriesPage\CategoriesPage.css` → **`AgentsPage\AgentsPage.css`**
11. `assistant-ui-chat\src\pages\CategoriesPage\CategorySkillsPage.tsx` → **`AgentsPage\AgentSkillsPage.tsx`**
12. `assistant-ui-chat\src\pages\CategoriesPage\CategorySkillsPage.css` → **`AgentsPage\AgentSkillsPage.css`**
13. `assistant-ui-chat\src\components\CategoryModal\CategoryModal.tsx` → **`AgentModal\AgentModal.tsx`**
14. `assistant-ui-chat\src\components\CategoryModal\CategoryModal.css` → **`AgentModal\AgentModal.css`**
15. `assistant-ui-chat\src\components\CategoryModal\index.ts` → **`AgentModal\index.ts`**

**Note:** When renaming directories, also rename the folder:
- `CategoriesPage\` → `AgentsPage\`
- `CategoryModal\` → `AgentModal\`

---

## Backend Files - Detailed Changes

### 1. Agent.Core\ConfigurationServices.cs
**Line 108:**
```csharp
// BEFORE:
services.AddScoped<ICategoryService, CategoryService>();

// AFTER:
services.AddScoped<IAgentService, AgentService>();
```

---

### 2. Agent.Core\Entities\CategoryEntity.cs → AgentEntity.cs
**Line 7:**
```csharp
// BEFORE:
public class CategoryEntity

// AFTER:
public class AgentEntity
```

**Action:** Rename file to `AgentEntity.cs`

---

### 3. Agent.Core\Implementations\Services\CategoryService.cs → AgentService.cs
| Line | Before | After |
|------|--------|-------|
| 8 | `public class CategoryService : ICategoryService` | `public class AgentService : IAgentService` |
| 12 | `public CategoryService(IDbContextFactory<ChatDbContext> dbContextFactory)` | `public AgentService(IDbContextFactory<ChatDbContext> dbContextFactory)` |
| 17 | `public async Task<CategoryEntity> CreateAsync(` | `public async Task<AgentEntity> CreateAsync(` |
| 23 | `var entity = new CategoryEntity` | `var entity = new AgentEntity` |
| 39 | `public async Task<CategoryEntity> UpdateAsync(` | `public async Task<AgentEntity> UpdateAsync(` |
| 48 | `?? throw new InvalidOperationException($"Category {id} not found");` | `?? throw new InvalidOperationException($"Agent {id} not found");` |
| 60 | `public async Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)` | `public async Task<AgentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)` |
| 67 | `public async Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken ct = default)` | `public async Task<IEnumerable<AgentEntity>> GetAllAsync(CancellationToken ct = default)` |
| 79 | `?? throw new InvalidOperationException($"Category {id} not found");` | `?? throw new InvalidOperationException($"Agent {id} not found");` |

**Action:** Rename file to `AgentService.cs`

---

### 4. Agent.Core\Entities\SkillEntity.cs
| Line | Before | After |
|------|--------|-------|
| 16 | `public Guid CategoryId { get; set; }` | `public Guid AgentId { get; set; }` |
| 39 | `[ForeignKey(nameof(CategoryId))]` | `[ForeignKey(nameof(AgentId))]` |
| 41 | `public CategoryEntity Category { get; set; } = null!;` | `public AgentEntity Agent { get; set; } = null!;` |

**Note:** Line 15 `[Column("category_id")]` should remain as-is since it references the database column name (will be updated in migration)

---

### 5. Agent.Core\Abstractions\Services\ISkillService.cs
| Line | Before | After |
|------|--------|-------|
| 7 | `Task<SkillEntity> CreateAsync(Guid categoryId,` | `Task<SkillEntity> CreateAsync(Guid agentId,` |
| 20 | `Task<CategoryEntity> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);` | `Task<AgentEntity> GetByAgentAsync(Guid agentId, CancellationToken ct = default);` |

---

### 6. Agent.Core\Abstractions\Services\ICategoryService.cs → IAgentService.cs
| Line | Before | After |
|------|--------|-------|
| 5 | `public interface ICategoryService` | `public interface IAgentService` |
| 7 | `Task<CategoryEntity> CreateAsync(string catCode, string name, string? description = null, CancellationToken ct = default);` | `Task<AgentEntity> CreateAsync(string catCode, string name, string? description = null, CancellationToken ct = default);` |
| 8 | `Task<CategoryEntity> UpdateAsync(Guid id, string? catCode, string? name = null, string? description = null, CancellationToken ct = default);` | `Task<AgentEntity> UpdateAsync(Guid id, string? catCode, string? name = null, string? description = null, CancellationToken ct = default);` |
| 9 | `Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);` | `Task<AgentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);` |
| 10 | `Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken ct = default);` | `Task<IEnumerable<AgentEntity>> GetAllAsync(CancellationToken ct = default);` |

**Action:** Rename file to `IAgentService.cs`

---

### 7. Agent.Core\Implementations\Services\SkillService.cs
| Line | Before | After |
|------|--------|-------|
| 24 | `Guid categoryId,` | `Guid agentId,` |
| 30 | `var category = await _dbContext.Categories` | `var agent = await _dbContext.Agents` |
| 31 | `.FirstOrDefaultAsync(c => c.Id == categoryId, ct)` | `.FirstOrDefaultAsync(c => c.Id == agentId, ct)` |
| 32 | `?? throw new InvalidOperationException($"Category {categoryId} not found");` | `?? throw new InvalidOperationException($"Agent {agentId} not found");` |
| 37 | `CategoryId = categoryId,` | `AgentId = agentId,` |
| 59 | `.Include(s => s.Category)` | `.Include(s => s.Agent)` |
| 77 | `.Include(s => s.Category)` | `.Include(s => s.Agent)` |
| 82 | `public async Task<CategoryEntity> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)` | `public async Task<AgentEntity> GetByAgentAsync(Guid agentId, CancellationToken ct = default)` |
| 84 | `var category = await _dbContext.Categories.Include(x=>x.Skills)` | `var agent = await _dbContext.Agents.Include(x=>x.Skills)` |
| 85 | `.FirstOrDefaultAsync(c => c.Id == categoryId, ct)` | `.FirstOrDefaultAsync(c => c.Id == agentId, ct)` |
| 86 | `?? throw new InvalidOperationException($"Category {categoryId} not found");` | `?? throw new InvalidOperationException($"Agent {agentId} not found");` |
| 88 | `return category;` | `return agent;` |

---

### 8. Agent.Core\Implementations\Persistents\Postgresql\ChatDbContext.cs
**Line 14:**
```csharp
// BEFORE:
public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

// AFTER:
public DbSet<AgentEntity> Agents => Set<AgentEntity>();
```

---

### 9. Agent.Api\Program.cs
**Line 82:**
```csharp
// BEFORE:
app.MapCategoryEndPoints();

// AFTER:
app.MapAgentEndPoints();
```

---

### 10. Agent.Api\Endpoints\CategoryEndPoint.cs → AgentEndPoint.cs
| Line | Before | After |
|------|--------|-------|
| 7 | `public static class CategoryEndPoint` | `public static class AgentEndPoint` |
| 9 | `public static IEndpointRouteBuilder MapCategoryEndPoints(this IEndpointRouteBuilder endpoints)` | `public static IEndpointRouteBuilder MapAgentEndPoints(this IEndpointRouteBuilder endpoints)` |
| 11 | `var group = endpoints.MapGroup("/api/categories")` | `var group = endpoints.MapGroup("/api/agents")` |
| 15 | `.WithName("CreateCategory")` | `.WithName("CreateAgent")` |
| 16 | `.WithSummary("Create a new category")` | `.WithSummary("Create a new agent")` |
| 17 | `.Produces<CategoryEntity>(StatusCodes.Status201Created)` | `.Produces<AgentEntity>(StatusCodes.Status201Created)` |
| 23 | `.Produces<IEnumerable<CategoryEntity>>(StatusCodes.Status200OK);` | `.Produces<IEnumerable<AgentEntity>>(StatusCodes.Status200OK);` |
| 26 | `.WithName("GetCategoryById")` | `.WithName("GetAgentById")` |
| 27 | `.WithSummary("Get category by ID")` | `.WithSummary("Get agent by ID")` |
| 28 | `.Produces<CategoryEntity>(StatusCodes.Status200OK)` | `.Produces<AgentEntity>(StatusCodes.Status200OK)` |
| 32 | `.WithName("UpdateCategory")` | `.WithName("UpdateAgent")` |
| 33 | `.WithSummary("Update an existing category")` | `.WithSummary("Update an existing agent")` |
| 34 | `.Produces<CategoryEntity>(StatusCodes.Status200OK)` | `.Produces<AgentEntity>(StatusCodes.Status200OK)` |
| 38 | `.WithName("DeleteCategory")` | `.WithName("DeleteAgent")` |
| 39 | `.WithSummary("Delete a category")` | `.WithSummary("Delete an agent")` |
| 47 | `CreateCategoryRequest request,` | `CreateAgentRequest request,` |
| 48 | `ICategoryService service,` | `IAgentService service,` |
| 57 | `UpdateCategoryRequest request,` | `UpdateAgentRequest request,` |
| 58 | `ICategoryService service,` | `IAgentService service,` |
| 66 | `ICategoryService service,` | `IAgentService service,` |
| 75 | `ICategoryService service,` | `IAgentService service,` |
| 85 | `ICategoryService service,` | `IAgentService service,` |

**Action:** Rename file to `AgentEndPoint.cs`

---

### 11. Agent.Api\Models\CategoryRequest.cs → AgentRequest.cs
| Line | Before | After |
|------|--------|-------|
| 3 | `public record CreateCategoryRequest(string Code, string Name, string? Description = null);` | `public record CreateAgentRequest(string Code, string Name, string? Description = null);` |
| 4 | `public record UpdateCategoryRequest(string? Code, string? Name = null, string? Description = null);` | `public record UpdateAgentRequest(string? Code, string? Name = null, string? Description = null);` |

**Action:** Rename file to `AgentRequest.cs`

---

### 12. Agent.Api\Endpoints\SkillEndPoints.cs
| Line | Before | After |
|------|--------|-------|
| 30 | `group.MapGet("/by-category/{categoryId:guid}", GetByCategoryAsync)` | `group.MapGet("/by-agent/{agentId:guid}", GetByAgentAsync)` |
| 31 | `.WithName("GetSkillsByCategory")` | `.WithName("GetSkillsByAgent")` |
| 32 | `.WithSummary("Get all skills by category")` | `.WithSummary("Get all skills by agent")` |
| 67 | `request.CategoryId,` | `request.AgentId,` |
| 99 | `private static async Task<IResult> GetByCategoryAsync(` | `private static async Task<IResult> GetByAgentAsync(` |
| 100 | `Guid categoryId,` | `Guid agentId,` |
| 104 | `var category = await service.GetByCategoryAsync(categoryId, ct);` | `var agent = await service.GetByAgentAsync(agentId, ct);` |
| 106 | `return Results.Ok(category);` | `return Results.Ok(agent);` |

---

### 13. Agent.Api\Models\SkillRequest.cs
**Line 5:**
```csharp
// BEFORE:
Guid CategoryId,

// AFTER:
Guid AgentId,
```

---

## Frontend Files - Detailed Changes

### 14. assistant-ui-chat\src\api\categoriesClient.ts → agentsClient.ts

**ALL OCCURRENCES - Replace throughout entire file:**
- `Category` → `Agent`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`
- `/api/categories` → `/api/agents`
- "category" → "agent" (in comments)

**Action:** Rename file to `agentsClient.ts`

---

### 15. assistant-ui-chat\src\types\Category.ts → Agent.ts

**ALL OCCURRENCES - Replace throughout entire file:**
- `Category` → `Agent`
- `CategoryWithSkills` → `AgentWithSkills`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`

**Action:** Rename file to `Agent.ts`

---

### 16. assistant-ui-chat\src\hooks\useCategories.ts → useAgents.ts

**ALL OCCURRENCES - Replace throughout entire file:**
- `Category` → `Agent`
- `categories` → `agents`
- `category` → `agent`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`
- `categoriesClient` → `agentsClient`
- `useCategories` → `useAgents`
- `setCategories` → `setAgents`
- `newCategory` → `newAgent`

**Action:** Rename file to `useAgents.ts`

---

### 17. assistant-ui-chat\src\hooks\useSkills.ts

| Variable/Parameter | Before | After |
|-------------------|--------|-------|
| Import | `Category` | `Agent` |
| Interface property | `category: Category;` | `agent: Agent;` |
| Method name | `fetchByCategory` | `fetchByAgent` |
| Parameter | `categoryId` | `agentId` |
| State variable | `category` | `agent` |
| Setter | `setCategory` | `setAgent` |
| API call | `getByCategory(categoryId)` | `getByAgent(agentId)` |

---

### 18. assistant-ui-chat\src\types\Skill.ts

| Line | Before | After |
|------|--------|-------|
| 1 | `import type { Category } from "./Category";` | `import type { Agent } from "./Agent";` |
| 6 | `categoryId: string;` | `agentId: string;` |
| 16 | `category?: Category;` | `agent?: Agent;` |
| 21 | `categoryId: string;` | `agentId: string;` |

---

### 19. assistant-ui-chat\src\types\index.ts

**Line 1:**
```typescript
// BEFORE:
export * from "./Category";

// AFTER:
export * from "./Agent";
```

---

### 20. assistant-ui-chat\src\api\skillsClient.ts

| Line | Before | After |
|------|--------|-------|
| 4 | `CategoryWithSkills,` | `AgentWithSkills,` |
| 25 | `async getByCategory(categoryId: string): Promise<CategoryWithSkills>` | `async getByAgent(agentId: string): Promise<AgentWithSkills>` |
| 26 | `const { data } = await axiosClient.get<CategoryWithSkills>(` | `const { data } = await axiosClient.get<AgentWithSkills>(` |
| 27 | `\`/api/skills/by-category/${categoryId}\`` | `\`/api/skills/by-agent/${agentId}\`` |

---

### 21. assistant-ui-chat\src\api\index.ts

**Update import:**
```typescript
// BEFORE:
export { categoriesClient } from "./categoriesClient";

// AFTER:
export { agentsClient } from "./agentsClient";
```

---

### 22. assistant-ui-chat\src\router.tsx

| Line | Before | After |
|------|--------|-------|
| 8 | `CategorySkillsPage,` | `AgentSkillsPage,` |
| 53 | `// Skills inside a category` | `// Skills inside an agent` |
| 54 | `path: "categories/:categoryId/skills",` | `path: "agents/:agentId/skills",` |
| 55 | `element: <CategorySkillsPage />,` | `element: <AgentSkillsPage />,` |

---

### 23. assistant-ui-chat\src\pages\index.ts

**Line 4:**
```typescript
// BEFORE:
export { CategorySkillsPage } from "./CategoriesPage/CategorySkillsPage";

// AFTER:
export { AgentSkillsPage } from "./AgentsPage/AgentSkillsPage";
```

---

### 24. assistant-ui-chat\src\components\index.ts

**Line 18:**
```typescript
// BEFORE:
export { CategoryModal } from "./CategoryModal";

// AFTER:
export { AgentModal } from "./AgentModal";
```

---

### 25. assistant-ui-chat\src\pages\CategoriesPage\CategoriesPage.tsx → AgentsPage\AgentsPage.tsx

**MAJOR REPLACEMENTS throughout entire file:**

**Imports:**
- `CategoryModal` → `AgentModal`
- `Category` → `Agent`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`

**State Variables:**
- `editingCategory` → `editingAgent`
- `setEditingCategory` → `setEditingAgent`
- `deleteCategory` → `deleteAgent`
- `setDeleteCategory` → `setDeleteAgent`

**Function Parameters:**
- `category: Category` → `agent: Agent`
- `category.id` → `agent.id`
- `category.name` → `agent.name`
- `categoryName` → `agentName`

**CSS Classes:**
- `.category-card` → `.agent-card`
- `.category-card-main` → `.agent-card-main`
- `.category-icon` → `.agent-icon`
- `.category-info` → `.agent-info`
- `.category-meta` → `.agent-meta`
- `.category-card-actions` → `.agent-card-actions`

**Text/Labels:**
- "category" → "agent"
- "Category" → "Agent"
- "categories" → "agents"

**Routes:**
- `/settings/categories/${category.id}/skills` → `/settings/agents/${agent.id}/skills`
- `?category=${category.id}` → `?agent=${agent.id}`

**Actions:**
1. Rename file to `AgentsPage.tsx`
2. Move to `AgentsPage\` directory

---

### 26. assistant-ui-chat\src\pages\CategoriesPage\CategoriesPage.css → AgentsPage\AgentsPage.css

**Replace ALL CSS class names:**
- `.category-card` → `.agent-card`
- `.category-card-main` → `.agent-card-main`
- `.category-icon` → `.agent-icon`
- `.category-info` → `.agent-info`
- `.category-meta` → `.agent-meta`
- `.category-card-actions` → `.agent-card-actions`

**Actions:**
1. Rename file to `AgentsPage.css`
2. Move to `AgentsPage\` directory

---

### 27. assistant-ui-chat\src\pages\CategoriesPage\CategorySkillsPage.tsx → AgentsPage\AgentSkillsPage.tsx

**MAJOR REPLACEMENTS:**

**Component name:**
- `CategorySkillsPage` → `AgentSkillsPage`

**Parameters:**
- `categoryId` → `agentId`

**Variables:**
- `category` → `agent`
- `fetchByCategory` → `fetchByAgent`
- `categoryNotFound` → `agentNotFound`
- `categoryName` → `agentName`

**Routes:**
- `/test-chat?skill=${skill.id}&category=${categoryId}` → `/test-chat?skill=${skill.id}&agent=${agentId}`
- `/settings/categories` → `/settings/agents`
- `/settings/categories/:categoryId/skills` → `/settings/agents/:agentId/skills`

**Text:**
- "Category not found" → "Agent not found"
- "category" → "agent" (in strings)

**Props:**
- `categories={[category]}` → `agents={[agent]}`

**Actions:**
1. Rename file to `AgentSkillsPage.tsx`
2. Move to `AgentsPage\` directory
3. Update import: `CategorySkillsPage.css` → `AgentSkillsPage.css`

---

### 28. assistant-ui-chat\src\pages\CategoriesPage\CategorySkillsPage.css → AgentsPage\AgentSkillsPage.css

**Actions:**
1. Rename file to `AgentSkillsPage.css`
2. Move to `AgentsPage\` directory

---

### 29. assistant-ui-chat\src\pages\ChatPage\TestChatPage.tsx

**Replacements:**

**Query params:**
- `"category"` → `"agent"`
- `categoryId` → `agentId`
- `categoryName` → `agentName`

**Variables:**
- `category` → `agent`
- `fetchByCategory` → `fetchByAgent`

**Routes:**
- `/settings/categories/${categoryId}/skills` → `/settings/agents/${agentId}/skills`

**Headers:**
- `"X-Agent-Id" = categoryId` → `"X-Agent-Id" = agentId`

**Type:**
- `type: "category"` → `type: "agent"`

**Props:**
- `categoryId={categoryId}` → `agentId={agentId}`

---

### 30. assistant-ui-chat\src\components\CategoryModal\CategoryModal.tsx → AgentModal\AgentModal.tsx

**MAJOR REPLACEMENTS:**

**Types:**
- `Category` → `Agent`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`
- `CategoryModalProps` → `AgentModalProps`
- `CategoryModal` → `AgentModal`

**Props:**
- `category` → `agent`

**IDs:**
- `category-code` → `agent-code`
- `category-name` → `agent-name`
- `category-description` → `agent-description`

**Labels:**
- "Category" → "Agent"
- "category" → "agent"

**Actions:**
1. Rename file to `AgentModal.tsx`
2. Move to `AgentModal\` directory
3. Update import: `CategoryModal.css` → `AgentModal.css`

---

### 31. assistant-ui-chat\src\components\CategoryModal\CategoryModal.css → AgentModal\AgentModal.css

**Actions:**
1. Rename file to `AgentModal.css`
2. Move to `AgentModal\` directory

---

### 32. assistant-ui-chat\src\components\CategoryModal\index.ts → AgentModal\index.ts

**Lines 1-2:**
```typescript
// BEFORE:
export { CategoryModal } from "./CategoryModal";
export type { CategoryModalProps } from "./CategoryModal";

// AFTER:
export { AgentModal } from "./AgentModal";
export type { AgentModalProps } from "./AgentModal";
```

**Actions:**
1. Rename file (keep as `index.ts`)
2. Move to `AgentModal\` directory

---

### 33. assistant-ui-chat\src\components\SkillModal\SkillModal.tsx

**Line 15:**
```typescript
// BEFORE:
import type { Skill, Category, CreateSkillRequest, UpdateSkillRequest, SkillRouter } from "../../types";

// AFTER:
import type { Skill, Agent, CreateSkillRequest, UpdateSkillRequest, SkillRouter } from "../../types";
```

**Line 25:**
```typescript
// BEFORE:
categories: Category[];

// AFTER:
agents: Agent[];
```

**Line 67:**
```typescript
// BEFORE:
const singleCategory = categories.length === 1;

// AFTER:
const singleAgent = agents.length === 1;
```

**Line 163-164:**
```typescript
// BEFORE:
{singleCategory && (
  <span className="category-badge">{categories[0].name}</span>

// AFTER:
{singleAgent && (
  <span className="agent-badge">{agents[0].name}</span>
```

---

### 34. assistant-ui-chat\src\components\SkillModal\SkillModal.css

**Line 78:**
```css
/* BEFORE: */
.category-badge {

/* AFTER: */
.agent-badge {
```

---

### 35. assistant-ui-chat\src\components\TestChat\SkillInstructionsModal.tsx

**Line 9:**
```typescript
// BEFORE:
categoryId?: string | null;

// AFTER:
agentId?: string | null;
```

**Line 15:**
```typescript
// BEFORE:
categoryId,

// AFTER:
agentId,
```

**Line 21:**
```typescript
// BEFORE:
navigate(`/settings/categories/${categoryId}/skills`, {

// AFTER:
navigate(`/settings/agents/${agentId}/skills`, {
```

---

### 36. assistant-ui-chat\src\components\TestChat\SkillsSidebar.tsx

**Line 15:**
```typescript
// BEFORE:
categoryId?: string | null;

// AFTER:
agentId?: string | null;
```

**Line 23:**
```typescript
// BEFORE:
categoryId,

// AFTER:
agentId,
```

**Line 29:**
```typescript
// BEFORE:
navigate(`/settings/categories/${categoryId}/skills`, {

// AFTER:
navigate(`/settings/agents/${agentId}/skills`, {
```

---

### 37. assistant-ui-chat\src\layout\SettingsLayout.tsx

**Line 34 (comment):**
```typescript
// BEFORE:
// Also highlight when viewing skills inside a category

// AFTER:
// Also highlight when viewing skills inside an agent
```

---

## Database Migration Files

### 38. migration\002_category_skill_tool.sql

| Line | Before | After |
|------|--------|-------|
| 1 | `-- Categories` | `-- Agents` |
| 2 | `CREATE TABLE categories (` | `CREATE TABLE agents (` |
| 15 | `category_id UUID NOT NULL REFERENCES categories(id),` | `agent_id UUID NOT NULL REFERENCES agents(id),` |
| 21 | `UNIQUE(category_id, code)` | `UNIQUE(agent_id, code)` |
| 38 | `INSERT INTO categories (id, code, name, description, created_at, updated_at)` | `INSERT INTO agents (id, code, name, description, created_at, updated_at)` |

**IMPORTANT:** Create a new migration file `003_rename_category_to_agent.sql`:
```sql
-- Rename categories table to agents
ALTER TABLE categories RENAME TO agents;

-- Update foreign key column name in skills table
ALTER TABLE skills RENAME COLUMN category_id TO agent_id;
```

---

## Documentation Files

### 39. assistant-ui-chat\src\docs\API_AND_TYPES.md

**ALL OCCURRENCES - Replace throughout:**
- `Category` → `Agent`
- `category` → `agent`
- `categories` → `agents`
- `CategoryWithSkills` → `AgentWithSkills`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`
- `/api/categories` → `/api/agents`

---

## Recommended Execution Order

### Phase 1: Database (CRITICAL - Do First)
1. ✅ Create new migration: `003_rename_category_to_agent.sql`
2. ✅ Run migration on development database
3. ✅ Verify data integrity

### Phase 2: Backend Core (Bottom-up approach)
4. ✅ Rename `CategoryEntity.cs` → `AgentEntity.cs`
5. ✅ Update `SkillEntity.cs` (foreign key references)
6. ✅ Rename `ICategoryService.cs` → `IAgentService.cs`
7. ✅ Rename `CategoryService.cs` → `AgentService.cs`
8. ✅ Update `SkillService.cs`
9. ✅ Update `ChatDbContext.cs`
10. ✅ Update `ConfigurationServices.cs`

### Phase 3: Backend API
11. ✅ Rename `CategoryRequest.cs` → `AgentRequest.cs`
12. ✅ Rename `CategoryEndPoint.cs` → `AgentEndPoint.cs`
13. ✅ Update `SkillEndPoints.cs`
14. ✅ Update `SkillRequest.cs`
15. ✅ Update `Program.cs`

### Phase 4: Frontend Types & API Clients
16. ✅ Rename `Category.ts` → `Agent.ts`
17. ✅ Update `Skill.ts`
18. ✅ Update `index.ts` (types)
19. ✅ Rename `categoriesClient.ts` → `agentsClient.ts`
20. ✅ Update `skillsClient.ts`
21. ✅ Update `index.ts` (api)

### Phase 5: Frontend Hooks
22. ✅ Rename `useCategories.ts` → `useAgents.ts`
23. ✅ Update `useSkills.ts`
24. ✅ Update `index.ts` (hooks)

### Phase 6: Frontend Components
25. ✅ Rename `CategoryModal/` → `AgentModal/`
   - Rename all files inside
   - Update index.ts
26. ✅ Update `SkillModal.tsx`
27. ✅ Update `SkillModal.css`
28. ✅ Update `TestChat/SkillInstructionsModal.tsx`
29. ✅ Update `TestChat/SkillsSidebar.tsx`
30. ✅ Update `components/index.ts`

### Phase 7: Frontend Pages
31. ✅ Rename `CategoriesPage/` → `AgentsPage/`
   - Rename `CategoriesPage.tsx` → `AgentsPage.tsx`
   - Rename `CategoriesPage.css` → `AgentsPage.css`
   - Rename `CategorySkillsPage.tsx` → `AgentSkillsPage.tsx`
   - Rename `CategorySkillsPage.css` → `AgentSkillsPage.css`
32. ✅ Update `TestChatPage.tsx`
33. ✅ Update `pages/index.ts`

### Phase 8: Frontend Routing & Layout
34. ✅ Update `router.tsx`
35. ✅ Update `SettingsLayout.tsx`

### Phase 9: Documentation
36. ✅ Update `API_AND_TYPES.md`
37. ✅ Update `PROJECT_DOCUMENTATION.md` (if exists)

### Phase 10: Testing
38. ✅ Run backend build: `dotnet build`
39. ✅ Run backend tests (if any)
40. ✅ Run frontend build: `npm run build`
41. ✅ Test all functionality manually
42. ✅ Verify database queries work correctly

---

## Search & Replace Patterns (Use with Caution)

### Backend (C#) - Case Sensitive
- `CategoryEntity` → `AgentEntity`
- `ICategoryService` → `IAgentService`
- `CategoryService` → `AgentService`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`
- `CategoryId` → `AgentId`
- `categoryId` → `agentId`
- `GetByCategoryAsync` → `GetByAgentAsync`
- `MapCategoryEndPoints` → `MapAgentEndPoints`
- `.Categories` → `.Agents`
- `"Category ` → `"Agent `

### Frontend (TypeScript/TSX) - Case Sensitive
- `Category` → `Agent` (type/interface)
- `category` → `agent` (variable)
- `categories` → `agents` (array variable)
- `CategoryWithSkills` → `AgentWithSkills`
- `CreateCategoryRequest` → `CreateAgentRequest`
- `UpdateCategoryRequest` → `UpdateAgentRequest`
- `CategoryModal` → `AgentModal`
- `CategoryModalProps` → `AgentModalProps`
- `CategorySkillsPage` → `AgentSkillsPage`
- `useCategories` → `useAgents`
- `categoriesClient` → `agentsClient`
- `fetchByCategory` → `fetchByAgent`
- `getByCategory` → `getByAgent`
- `setCategories` → `setAgents`
- `setCategory` → `setAgent`
- `editingCategory` → `editingAgent`
- `deleteCategory` → `deleteAgent`
- `categoryNotFound` → `agentNotFound`
- `categoryName` → `agentName`
- `categoryId` → `agentId`

### URL Routes
- `/api/categories` → `/api/agents`
- `/settings/categories` → `/settings/agents`
- `/by-category/` → `/by-agent/`
- `?category=` → `?agent=`

### CSS Classes
- `.category-card` → `.agent-card`
- `.category-icon` → `.agent-icon`
- `.category-info` → `.agent-info`
- `.category-meta` → `.agent-meta`
- `.category-badge` → `.agent-badge`

### Database
- `categories` (table) → `agents`
- `category_id` (column) → `agent_id`

---

## Post-Rename Checklist

### Functionality Testing
- [ ] Create new agent
- [ ] Edit existing agent
- [ ] Delete agent
- [ ] View agents list
- [ ] Create skill within agent
- [ ] Edit skill
- [ ] Delete skill
- [ ] View skills within agent
- [ ] Test chat with agent
- [ ] Test chat with specific skill
- [ ] Navigate between pages
- [ ] Verify all routes work

### Code Quality
- [ ] No TypeScript errors
- [ ] No C# build errors
- [ ] All imports resolve correctly
- [ ] No broken references
- [ ] CSS classes applied correctly

### Database
- [ ] Migration applied successfully
- [ ] Foreign keys intact
- [ ] Existing data preserved
- [ ] Queries execute correctly

### Documentation
- [ ] README updated (if needed)
- [ ] API documentation updated
- [ ] Code comments updated

---

## Rollback Plan

If issues occur, rollback in reverse order:

1. Revert frontend changes (git)
2. Revert backend changes (git)
3. Rollback database migration:
   ```sql
   ALTER TABLE agents RENAME TO categories;
   ALTER TABLE skills RENAME COLUMN agent_id TO category_id;
   ```

---

## Summary Statistics

- **Total Files to Rename:** 15 files
- **Total Files to Modify:** 40+ files
- **Total Line Changes:** 380+ lines
- **Estimated Time:** 3-4 hours with testing
- **Risk Level:** Medium (database schema change)
- **Recommended Approach:** Create feature branch, test thoroughly before merging

---

**Last Updated:** 2026-01-20
