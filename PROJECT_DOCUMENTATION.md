# AI Assistant Project Documentation

## Table of Contents
1. [Backend Interfaces](#backend-interfaces)
2. [Frontend Components](#frontend-components)
3. [Frontend Pages](#frontend-pages)

---

# Backend Interfaces

## 1. IAgent
**File:** `Agent.Core\Abstractions\IAgent.cs`

### Purpose
Core interface defining the contract for all AI agent specialists in the system.

### Properties

```csharp
public Guid Id { get; }
```
**What is this?** Unique identifier for the agent instance.
**What does it do?** Provides a stable GUID reference to identify specific agent implementations (GeneralAgent, BackendDeveloperAgent, etc.).

```csharp
public string Name { get; }
```
**What is this?** Human-readable name of the agent.
**What does it do?** Returns the display name for the agent (e.g., "General Assistant", "Backend Developer").

### Methods

```csharp
IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string userMessage,
    CancellationToken cancellationToken = default);
```
**What is this?** Asynchronous streaming method for real-time agent responses.
**What does it do?** Processes user messages and yields response updates as they're generated (streaming), enabling real-time UI updates via SSE. Returns an async enumerable of response updates.

```csharp
Task<AgentRunResponse> RunAsync(string userMessage,
    CancellationToken cancellationToken = default);
```
**What is this?** Asynchronous non-streaming method for agent responses.
**What does it do?** Processes user messages and returns a complete response once finished. Used when streaming is not required.

---

## 2. IAgentManager
**File:** `Agent.Core\Abstractions\IAgentManager.cs`

### Purpose
Manages agent lifecycle, conversation threads, and agent selection/creation.

### Methods

```csharp
Task<(IAgent agent, ChatThreadEntity thread, bool isNewConversation)> GetOrCreateAsync(
    Guid? agentId,
    Guid? threadId,
    string userMessage,
    ChatMessageStoreEnum chatMessageStore = ChatMessageStoreEnum.Postgresql,
    CancellationToken ct = default);
```
**What is this?** Agent and thread orchestration method.
**What does it do?**
- Retrieves or creates an agent instance based on agentId
- Retrieves or creates a conversation thread based on threadId
- Determines if this is a new conversation or existing one
- Sets up the appropriate message storage (PostgreSQL or in-memory)
- Returns a tuple containing the agent, thread entity, and new conversation flag

```csharp
Task<object> DryRunAsync(string userMessage, CancellationToken ct = default);
```
**What is this?** Test/validation method for agent processing.
**What does it do?** Performs a dry run of message processing without persisting results, used for testing and validation purposes.

---

## 3. IIntentClassificationService
**File:** `Agent.Core\Abstractions\LLM\IIntentClassificationService.cs`

### Purpose
Service for classifying user intent from messages using AI.

### Methods

```csharp
Task<IntentClassificationResult> IntentAsync(string userMessage, CancellationToken cancellationToken);
```
**What is this?** Intent classification method using LLM and vector search.
**What does it do?** Analyzes user messages to determine intent, helping route queries to appropriate agents or skills. Returns classification results with confidence scores.

---

## 4. ISemanticKernelBuilder
**File:** `Agent.Core\Abstractions\LLM\ISemanticKernelBuilder.cs`

### Purpose
Factory interface for building LLM clients and embedding generators.

### Methods

```csharp
IChatClient Build(LLMProviderType provider = LLMProviderType.AzureOpenAI);
```
**What is this?** Chat client factory method.
**What does it do?** Creates and configures an IChatClient instance for the specified LLM provider (Azure OpenAI or Anthropic), used by agents to communicate with language models.

```csharp
IEmbeddingGenerator<string, Embedding<float>> GetEmbeddingGenerator(
    LLMProviderType provider = LLMProviderType.AzureOpenAI);
```
**What is this?** Embedding generator factory method.
**What does it do?** Creates an embedding generator for converting text to vector embeddings, used for semantic search in Qdrant vector database.

---

## 5. IChatDbContext
**File:** `Agent.Core\Abstractions\Persistents\IChatDbContext.cs`

### Purpose
Marker interface for Entity Framework chat database context.

### Properties/Methods
**What is this?** Empty marker interface.
**What does it do?** Serves as a contract identifier for dependency injection of the PostgreSQL chat database context. The actual implementation (ChatDbContext) contains DbSet properties for ChatThreadEntity and ChatMessageEntity.

---

## 6. IChatMessageStoreFactory
**File:** `Agent.Core\Abstractions\Persistents\IChatMessageStoreFactory.cs`

### Purpose
Factory for creating chat message stores with conversation state.

### Methods

```csharp
ChatMessageStore Create(
    JsonElement serializedState,
    JsonSerializerOptions? options = null,
    ChatMessageStoreEnum chatMessageStore = ChatMessageStoreEnum.Postgresql);
```
**What is this?** Message store factory method for resuming conversations.
**What does it do?** Creates a ChatMessageStore from serialized state (JSON), allowing conversations to be resumed with full history. Supports both PostgreSQL and in-memory storage backends.

---

## 7. IVectorRecord
**File:** `Agent.Core\Abstractions\Persistents\IQdrantRepository.cs`

### Purpose
Base interface for vector database records.

### Properties

```csharp
ReadOnlyMemory<float>? Embedding { get; set; }
```
**What is this?** Vector embedding storage property.
**What does it do?** Stores the float array representation of text embeddings for semantic search. Used by Qdrant vector database operations.

---

## 8. IQdrantRepository<TRecord>
**File:** `Agent.Core\Abstractions\Persistents\IQdrantRepository.cs`

**Generic Constraint:** `where TRecord : QdrantRecordBase`

### Purpose
Generic repository interface for Qdrant vector database operations.

### Methods

```csharp
Task UpsertAsync(TRecord record, CancellationToken cancellationToken = default);
```
**What is this?** Insert or update method for vector records.
**What does it do?** Inserts a new record or updates an existing one in the Qdrant collection. Handles embedding generation and storage.

```csharp
Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
```
**What is this?** Vector record deletion method.
**What does it do?** Removes a record from the Qdrant collection by its unique identifier.

```csharp
Task<IEnumerable<TRecord>> SearchAsync(
    string query,
    int top = 5,
    float? similarityThreshold = null,
    CancellationToken cancellationToken = default);
```
**What is this?** Semantic search method (basic).
**What does it do?** Performs vector similarity search on the query text, returning the top N most similar records above the similarity threshold.

```csharp
Task<IEnumerable<TRecord>> SearchAsync(
    string query,
    int top = 5,
    float? similarityThreshold = null,
    VectorSearchOptions<TRecord>? options = null,
    CancellationToken cancellationToken = default);
```
**What is this?** Semantic search method (advanced).
**What does it do?** Performs vector similarity search with additional filtering and search options, allowing more complex queries.

```csharp
Task<TRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
```
**What is this?** Direct record retrieval method.
**What does it do?** Fetches a single record by its unique identifier without performing semantic search.

---

## 9. ICategoryService
**File:** `Agent.Core\Abstractions\Services\ICategoryService.cs`

### Purpose
Service for managing skill categories (agent groupings).

### Methods

```csharp
Task<CategoryEntity> CreateAsync(
    string catCode,
    string name,
    string? description = null,
    CancellationToken ct = default);
```
**What is this?** Category creation method.
**What does it do?** Creates a new category entity with unique code, name, and optional description. Used to organize skills into logical groups.

```csharp
Task<CategoryEntity> UpdateAsync(
    Guid id,
    string? catCode,
    string? name = null,
    string? description = null,
    CancellationToken ct = default);
```
**What is this?** Category modification method.
**What does it do?** Updates an existing category's code, name, or description. Optional parameters allow partial updates.

```csharp
Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Category retrieval method.
**What does it do?** Fetches a single category by its unique identifier, returns null if not found.

```csharp
Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken ct = default);
```
**What is this?** Category listing method.
**What does it do?** Retrieves all categories in the system, used to populate category selection UI.

```csharp
Task DeleteAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Category removal method.
**What does it do?** Deletes a category and potentially cascades to associated skills (depending on DB constraints).

---

## 10. ISkillRouterService
**File:** `Agent.Core\Abstractions\Services\ISkillRouterService.cs`

### Purpose
Service for managing skill routing queries (example user queries for intent classification).

### Methods

```csharp
Task<IEnumerable<SkillRoutingRecord>> GetBySkillCodeAsync(
    string skillCode,
    CancellationToken ct = default);
```
**What is this?** Router query retrieval by skill method.
**What does it do?** Fetches all routing example queries for a specific skill, used to train intent classification.

```csharp
Task<SkillRoutingRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Single router query retrieval method.
**What does it do?** Fetches a specific routing record by unique identifier.

```csharp
Task<SkillRoutingRecord> CreateAsync(
    string skillCode,
    string skillName,
    string userQueries,
    CancellationToken ct = default);
```
**What is this?** Router query creation method.
**What does it do?** Creates a new routing example with user query text, skill code/name, and generates embeddings for vector search. Used to add training examples for skill routing.

```csharp
Task<SkillRoutingRecord?> UpdateAsync(
    Guid id,
    string userQueries,
    CancellationToken ct = default);
```
**What is this?** Router query modification method.
**What does it do?** Updates an existing routing query text and regenerates embeddings for improved routing accuracy.

```csharp
Task DeleteAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Single router query deletion method.
**What does it do?** Removes a specific routing record from the vector database.

```csharp
Task DeleteBySkillCodeAsync(string skillCode, CancellationToken ct = default);
```
**What is this?** Bulk router query deletion method.
**What does it do?** Removes all routing records associated with a specific skill code, used when deleting skills.

---

## 11. ISkillService
**File:** `Agent.Core\Abstractions\Services\ISkillService.cs`

### Purpose
Service for managing skills (AI capabilities with system prompts).

### Methods

```csharp
Task<SkillEntity> CreateAsync(
    Guid categoryId,
    string skillCode,
    string name,
    string systemPrompt,
    CancellationToken ct = default);
```
**What is this?** Skill creation method.
**What does it do?** Creates a new skill entity with category association, unique code, name, and system prompt instructions for the AI.

```csharp
Task<SkillEntity> UpdateAsync(
    Guid id,
    string? skillCode = null,
    string? name = null,
    string? systemPrompt = null,
    CancellationToken ct = default);
```
**What is this?** Skill modification method.
**What does it do?** Updates an existing skill's code, name, or system prompt. Optional parameters enable partial updates.

```csharp
Task<SkillEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Skill retrieval method.
**What does it do?** Fetches a single skill by its unique identifier, returns null if not found.

```csharp
Task<CategoryEntity> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
```
**What is this?** Category-based skill retrieval method.
**What does it do?** Fetches a category entity with all associated skills included (eager loading), used to display skills within a category.

```csharp
Task DeleteAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Skill removal method.
**What does it do?** Deletes a skill and potentially cascades to associated tools and routing records.

```csharp
Task<SkillEntity?> RouteAsync(string query, CancellationToken ct = default);
```
**What is this?** Intelligent skill routing method.
**What does it do?** Uses vector similarity search on routing queries to determine which skill best matches the user's query. Returns the matched skill or null if no good match found.

---

## 12. IToolService
**File:** `Agent.Core\Abstractions\Services\IToolService.cs`

### Purpose
Service for managing tools/functions that skills can use (OpenAPI, MCP connections).

### Methods

```csharp
Task<ToolEntity> CreateAsync(
    Guid skillId,
    string name,
    string type,
    string endpoint,
    string? description = null,
    JsonDocument? config = null,
    bool isPrefetch = false,
    CancellationToken ct = default);
```
**What is this?** Tool creation method.
**What does it do?** Creates a new tool entity associated with a skill, specifying tool type (OpenAPI, MCP), endpoint, configuration JSON, and prefetch behavior. Enables skills to call external APIs or functions.

```csharp
Task<ToolEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Tool retrieval method.
**What does it do?** Fetches a single tool by its unique identifier, returns null if not found.

```csharp
Task<IEnumerable<ToolEntity>> GetBySkillAsync(Guid skillId, CancellationToken ct = default);
```
**What is this?** Skill-based tool listing method.
**What does it do?** Retrieves all tools associated with a specific skill, used to configure agent capabilities.

```csharp
Task<ToolEntity> UpdateAsync(
    Guid id,
    string? name = null,
    string? type = null,
    string? endpoint = null,
    string? description = null,
    JsonDocument? config = null,
    bool? isPrefetch = null,
    CancellationToken ct = default);
```
**What is this?** Tool modification method.
**What does it do?** Updates an existing tool's properties. Optional parameters allow partial updates.

```csharp
Task DeleteAsync(Guid id, CancellationToken ct = default);
```
**What is this?** Tool removal method.
**What does it do?** Deletes a tool entity from the database.

---

# Frontend Components

## Layout Components

### 1. ChatRuntime
**File:** `assistant-ui-chat\src\components\ChatRuntime.tsx`

**What is this?** Core chat orchestration component managing SSE streaming and runtime context.

**What does it do?**
- Manages chat stream adapter for API communication via `/chat/stream` endpoint
- Handles SSE events: metadata, stream start/done, and errors
- Supports both new and existing conversations
- Provides AssistantRuntimeProvider context to child components
- Wraps the Thread component with runtime configuration

**Used by:**
- **Pages:** ChatPage (1 page)
- **Components:** None directly
- **Total usage:** 1 file

---

### 2. Thread
**File:** `assistant-ui-chat\src\components\Thread\Thread.tsx`

**What is this?** Full-screen Claude-like chat layout displaying conversation threads.

**What does it do?**
- Renders user and assistant messages with action bars
- Displays message composer with send button
- Shows scroll-to-bottom button when scrolled up
- Includes branch picker for message navigation
- Shows welcome screen with suggestions for new conversations
- Built on @assistant-ui/react primitives

**Used by:**
- **Pages:** ChatPage (via ChatRuntime), TestChatPage (2 pages)
- **Components:** ChatRuntime
- **Total usage:** 2 files

---

### 3. Sidebar
**File:** `assistant-ui-chat\src\components\Sidebar\Sidebar.tsx`

**What is this?** Conversation list sidebar with date-based grouping.

**What does it do?**
- Groups conversations by date (Today, Yesterday, Previous 7 days, etc.)
- Provides new chat button and toggle button
- Displays conversation items with delete functionality
- Integrates Settings component
- Manages sidebar open/close state

**Used by:**
- **Pages:** AppLayout (1 layout)
- **Components:** None directly
- **Total usage:** 1 file (via index.ts export)

---

### 4. Settings
**File:** `assistant-ui-chat\src\components\Settings\Settings.tsx`

**What is this?** User settings menu popover with navigation options.

**What does it do?**
- Displays settings popover menu
- Shows user avatar and name
- Provides navigation to Agents, Tools, Knowledge Base, Profile
- Includes logout functionality
- Supports keyboard interaction (Escape, Enter/Space)

**Used by:**
- **Pages:** 1 (via Sidebar)
- **Components:** Sidebar
- **Total usage:** 2 files

---

## Modal/Dialog Components

### 5. Modal
**File:** `assistant-ui-chat\src\components\Modal\Modal.tsx`

**What is this?** Generic reusable modal dialog base component.

**What does it do?**
- Provides modal structure with customizable size (sm, md, lg, xl)
- Includes close button (X icon)
- Handles Escape key to close
- Supports optional footer
- Acts as base for other specialized modals

**Used by:**
- **Pages:** 0 (base component)
- **Components:** Used conceptually by CategoryModal, SkillModal, SkillInstructionsModal
- **Total usage:** Base component exported

---

### 6. ConfirmDialog
**File:** `assistant-ui-chat\src\components\ConfirmDialog\ConfirmDialog.tsx`

**What is this?** Confirmation dialog for destructive actions.

**What does it do?**
- Displays confirmation message with customizable text
- Shows danger/warning styling for destructive actions
- Includes loading state with disabled buttons
- Provides Confirm and Cancel buttons
- Shows alert icon

**Used by:**
- **Pages:** CategoriesPage, CategorySkillsPage (2 pages)
- **Components:** None
- **Total usage:** 3 files

---

### 7. CategoryModal
**File:** `assistant-ui-chat\src\components\CategoryModal\CategoryModal.tsx`

**What is this?** Modal for creating and editing categories.

**What does it do?**
- Auto-detects Create vs Edit mode
- Provides form with code, name, description fields
- Validates required fields (code and name)
- Displays error messages
- Supports Escape and Ctrl+Enter shortcuts
- Uses FormField components

**Used by:**
- **Pages:** CategoriesPage (1 page)
- **Components:** None
- **Total usage:** 2 files

---

### 8. SkillModal
**File:** `assistant-ui-chat\src\components\SkillModal\SkillModal.tsx`

**What is this?** Advanced modal for creating and editing skills with markdown editor.

**What does it do?**
- Provides tabbed interface: Prompt tab and Routing Queries tab
- Includes markdown editor with modes: split, edit, preview
- Shows live markdown preview for system prompts
- Manages routing queries via SkillRoutersSection
- Supports fullscreen/windowed toggle
- Includes Ctrl+S save shortcut
- Uses FormField components

**Used by:**
- **Pages:** CategorySkillsPage (1 page)
- **Components:** SkillRoutersSection
- **Total usage:** 2 files

---

## Form & Data Display Components

### 9. FormField
**File:** `assistant-ui-chat\src\components\Form\FormField.tsx`

**What is this?** Reusable form field wrapper with label and error display.

**What does it do?**
- Wraps input elements with consistent styling
- Displays label with optional required marker (*)
- Shows error messages below field
- Includes optional hint text
- Provides accessible htmlFor linking

**Used by:**
- **Pages:** 2 (via CategoryModal and SkillModal)
- **Components:** CategoryModal, SkillModal
- **Total usage:** 3 files

---

### 10. DataTable
**File:** `assistant-ui-chat\src\components\DataTable\DataTable.tsx`

**What is this?** Generic reusable data table with search, sorting, and pagination.

**What does it do?**
- Displays data in customizable columns with render functions
- Provides search bar with onChange callback
- Supports pagination with page controls
- Handles loading, error, and empty states
- Includes actions column for row-level operations
- Generic TypeScript support for type safety

**Used by:**
- **Pages:** CategorySkillsPage (1 page)
- **Components:** None
- **Total usage:** 2 files

---

## Rich Content Components (Thread Sub-components)

### 11. MarkdownText
**File:** `assistant-ui-chat\src\components\Thread\rich-content\MarkdownText.tsx`

**What is this?** Markdown renderer for assistant messages.

**What does it do?**
- Renders markdown with GitHub Flavored Markdown support (remarkGfm)
- Custom code block rendering via CodeBlock component
- Opens links in new tabs
- Styles tables with wrapper div
- Supports images with figcaption
- Renders blockquotes with custom styling

**Used by:**
- **Pages:** 2 (ChatPage and TestChatPage via Thread)
- **Components:** Thread
- **Total usage:** 2 files

---

### 12. CodeBlock
**File:** `assistant-ui-chat\src\components\Thread\rich-content\CodeBlock.tsx`

**What is this?** Syntax-highlighted code block renderer.

**What does it do?**
- Provides syntax highlighting with Prism
- Detects and displays programming language
- Includes copy-to-clipboard button with feedback
- Shows line numbers for multi-line code
- Uses One Dark theme styling

**Used by:**
- **Pages:** 2 (ChatPage, TestChatPage via MarkdownText)
- **Components:** MarkdownText
- **Total usage:** 2 files

---

### 13. ImageContent
**File:** `assistant-ui-chat\src\components\Thread\rich-content\ImageContent.tsx`

**What is this?** Image display component with loading and error states.

**What does it do?**
- Shows loading spinner while image loads
- Displays error fallback on load failure
- Supports click-to-expand lightbox functionality
- Renders alt text and figcaption
- Handles image metadata (width, height)

**Used by:**
- **Pages:** 0 (exported for potential use)
- **Components:** None currently
- **Total usage:** 1 file (exported utility)

---

### 14. FileAttachment
**File:** `assistant-ui-chat\src\components\Thread\rich-content\FileAttachment.tsx`

**What is this?** File attachment display with file type icons.

**What does it do?**
- Shows dynamic file type icons (PDF, Word, Excel, ZIP, images, etc.)
- Formats file sizes (B, KB, MB)
- Provides download button with link opening
- Displays file metadata (name, size, type)

**Used by:**
- **Pages:** 0 (exported for potential use)
- **Components:** None currently
- **Total usage:** 1 file (exported utility)

---

### 15. ToolCallUI
**File:** `assistant-ui-chat\src\components\Thread\rich-content\ToolCallUI.tsx`

**What is this?** Display component for AI tool/function calls.

**What does it do?**
- Shows tool call status with icons (pending, running, success, error)
- Displays expandable arguments and results sections
- Formats JSON arguments and results with syntax highlighting
- Uses status-based color coding (left border)
- Provides collapsible details

**Used by:**
- **Pages:** 0 (exported for potential use)
- **Components:** None currently
- **Total usage:** 1 file (exported utility)

---

## Test/Skill Components

### 16. SkillsSidebar
**File:** `assistant-ui-chat\src\components\TestChat\SkillsSidebar.tsx`

**What is this?** Sidebar for displaying and managing skills during testing.

**What does it do?**
- Lists all skills in category with count display
- Shows loading state with spinner
- Displays empty state message
- Provides View skill button (opens SkillInstructionsModal)
- Provides Edit skill button (navigates to edit page)
- Highlights active skill being tested

**Used by:**
- **Pages:** TestChatPage (1 page)
- **Components:** None
- **Total usage:** 2 files

---

### 17. SkillInstructionsModal
**File:** `assistant-ui-chat\src\components\TestChat\SkillInstructionsModal.tsx`

**What is this?** Modal for viewing skill details in read-only format.

**What does it do?**
- Displays skill name, description, and system prompt
- Shows system prompt in formatted <pre> tag
- Provides Edit button (navigates to CategorySkillsPage)
- Includes Close button
- Read-only view for testing context

**Used by:**
- **Pages:** TestChatPage (1 page)
- **Components:** None
- **Total usage:** 2 files

---

### 18. SkillRoutersSection
**File:** `assistant-ui-chat\src\components\SkillModal\SkillRoutersSection.tsx`

**What is this?** Section for managing skill routing queries within SkillModal.

**What does it do?**
- Allows adding new routing query examples
- Supports Enter key to add queries
- Displays query list with remove buttons
- Handles loading and error states
- Shows "new skill" notice for unsaved skills
- Includes refresh button for reloading routers
- Displays query count

**Used by:**
- **Pages:** 1 (via SkillModal)
- **Components:** SkillModal
- **Total usage:** 2 files

---

## Utility Components

### 19. Icons
**File:** `assistant-ui-chat\src\components\icons.tsx`

**What is this?** Custom SVG icon components module.

**What does it do?**
- Exports ArrowDownIcon, SendIcon, CopyIcon, RefreshCwIcon, PencilIcon
- Provides simple SVG-based icons with customizable sizing
- Used alongside lucide-react icon library
- Utility component for internal use

**Used by:**
- **Pages:** Multiple (via other components)
- **Components:** Multiple components use these icons
- **Total usage:** Multiple files (utility module)

---

# Frontend Pages

## 1. ChatPage
**File:** `assistant-ui-chat\src\pages\ChatPage\ChatPage.tsx`

**What is this?** Main chat interface page for conversations.

**What does it do?**
- Displays the main chat interface using ChatRuntime component
- Loads existing conversations via router loader (ConversationLoaderData)
- Handles error and loading states for conversation loading
- Supports new conversations (no threadId) and existing ones (with threadId)
- Manages sidebar toggle and conversation state
- Integrates with React Router for navigation

**Routes:**
- `/` - New conversation (index route)
- `/conversation/:threadId` - Existing conversation

**Used by:**
- React Router in `router.tsx`
- AppLayout as main content
- Navigated from Sidebar conversation list

**Navigation Entry Points:**
- Sidebar: Click "New Chat" or conversation item
- Settings: "Back to Chat" button
- Direct URL navigation

---

## 2. TestChatPage
**File:** `assistant-ui-chat\src\pages\ChatPage\TestChatPage.tsx`

**What is this?** Standalone chat testing page for testing agents and skills.

**What does it do?**
- Allows testing entire categories (agents) or individual skills
- Displays test header banner with context (skill/category name)
- Includes toggleable skills sidebar showing all skills in category
- Provides skill instructions modal for viewing skill details
- Fetches skills based on categoryId parameter
- Manages test mode headers (X-Test-Mode, X-Agent-Id, X-Skill-Id)
- Handles skill loading errors with retry

**Route:**
- `/test-chat` (standalone, outside AppLayout)

**Query Parameters:**
- `?category={categoryId}` - Test entire category/agent
- `?skill={skillId}` - Test specific skill within category
- Also accepts location state: `categoryName`, `skillName`

**Used by:**
- React Router in `router.tsx`
- Navigated from CategoriesPage (Chat button on category card)
- Navigated from CategorySkillsPage (Chat button on skill row)

**Navigation Entry Points:**
- CategoriesPage: "Chat" button → `/test-chat?category=${categoryId}`
- CategorySkillsPage: "Chat" button → `/test-chat?skill=${skillId}&category=${categoryId}`
- Close button returns to CategorySkillsPage

---

## 3. CategoriesPage
**File:** `assistant-ui-chat\src\pages\CategoriesPage\CategoriesPage.tsx`

**What is this?** Page displaying all agent categories (labeled as "Agents" in UI).

**What does it do?**
- Displays category cards with name, description, and skill count
- Shows skill count with BrainIcon for each category
- Supports CRUD operations: Create, Update, Delete categories
- Provides "Chat" button to test entire category in TestChatPage
- Click on card navigates to CategorySkillsPage to view skills
- Includes CategoryModal for create/edit operations
- Includes ConfirmDialog for delete confirmation
- Handles error and loading states with retry option

**Route:**
- `/settings/categories`

**Used by:**
- React Router in `router.tsx`
- SettingsLayout (Settings menu navigation)
- Settings component (Agents button)

**Navigation Entry Points:**
- Settings menu: Click "Agents" button
- SettingsLayout sidebar: Click "Agents"
- Direct URL navigation

**Navigation Actions:**
- Click category card → `/settings/categories/{categoryId}/skills`
- Click "Chat" button → `/test-chat?category={categoryId}`
- Create/Edit buttons open CategoryModal

---

## 4. CategorySkillsPage
**File:** `assistant-ui-chat\src\pages\CategoriesPage\CategorySkillsPage.tsx`

**What is this?** Page displaying skills (capabilities) within a specific category.

**What does it do?**
- Displays skills in DataTable with columns: Code, Name, Updated date
- Supports CRUD operations on skills: Create, Update, Delete
- Manages skill routers (routing query examples) for each skill
- Includes search/filtering by skill code or name
- Shows SkillModal for create/edit with router management
- Provides ConfirmDialog for delete confirmation
- "Chat" button tests individual skill in TestChatPage
- Shows breadcrumb navigation to parent category
- Handles error states with retry and "category not found" state

**Route:**
- `/settings/categories/:categoryId/skills`

**URL Parameters:**
- `:categoryId` - The category ID to load skills for

**Used by:**
- React Router in `router.tsx`
- Navigated from CategoriesPage (click on category card)
- Navigated from TestChatPage (close button return)

**Navigation Entry Points:**
- CategoriesPage: Click category card
- TestChatPage: Close button returns here
- Direct URL navigation with categoryId

**Navigation Actions:**
- Click "Chat" button on skill → `/test-chat?skill={skillId}&category={categoryId}`
- Click "Back to Categories" → `/settings/categories`
- Create/Edit buttons open SkillModal

---

## 5. KnowledgeBasePage
**File:** `assistant-ui-chat\src\pages\KnowledgeBasePage\KnowledgeBasePage.tsx`

**What is this?** Placeholder page for knowledge base management (future feature).

**What does it do?**
- Displays "Knowledge Base management coming soon..." message
- Shows DatabaseIcon for visual identification
- Provides back navigation button to settings
- Part of SettingsLayout with left sidebar menu
- Reserved for future knowledge base functionality

**Route:**
- `/settings/knowledge-base`

**Used by:**
- React Router in `router.tsx`
- SettingsLayout (Settings menu navigation)
- Settings component (Knowledge Base button)

**Navigation Entry Points:**
- Settings menu: Click "Knowledge Base" button
- SettingsLayout sidebar: Click "Knowledge Base"
- Direct URL navigation

---

## 6. ProfilePage
**File:** `assistant-ui-chat\src\pages\ProfilePage\ProfilePage.tsx`

**What is this?** Placeholder page for user profile settings (future feature).

**What does it do?**
- Displays "Profile settings coming soon..." message
- Shows UserIcon for visual identification
- Provides back navigation button to settings
- Part of SettingsLayout with left sidebar menu under "Account" section
- Reserved for future profile management functionality

**Route:**
- `/settings/profile`

**Used by:**
- React Router in `router.tsx`
- SettingsLayout (Settings menu navigation)
- Settings component (Profile button)

**Navigation Entry Points:**
- Settings menu: Click "Profile" button
- SettingsLayout sidebar: Click "Profile" (Account section)
- Direct URL navigation

---

## 7. ToolsPage
**File:** `assistant-ui-chat\src\pages\ToolsPage\ToolsPage.tsx`

**What is this?** Placeholder page for tools management (future feature).

**What does it do?**
- Displays "Tools management coming soon..." message
- Shows WrenchIcon for visual identification
- Provides back navigation button to settings
- Part of SettingsLayout with left sidebar menu
- Reserved for future tool/function management functionality

**Route:**
- `/settings/tools`

**Used by:**
- React Router in `router.tsx`
- SettingsLayout (Settings menu navigation)
- Settings component (Tools button)

**Navigation Entry Points:**
- Settings menu: Click "Tools" button
- SettingsLayout sidebar: Click "Tools"
- Direct URL navigation

---

## Navigation Flow Summary

```
Application Entry Point
│
├─ Main Chat Flow
│  ├─ / (ChatPage - new conversation)
│  └─ /conversation/:threadId (ChatPage - existing conversation)
│
├─ Settings Flow (via Settings menu or SettingsLayout)
│  │
│  ├─ /settings/categories (CategoriesPage)
│  │  ├─ Click category card → /settings/categories/:categoryId/skills
│  │  └─ Chat button → /test-chat?category={id}
│  │
│  ├─ /settings/categories/:categoryId/skills (CategorySkillsPage)
│  │  ├─ Chat button → /test-chat?skill={skillId}&category={categoryId}
│  │  └─ Back to Categories → /settings/categories
│  │
│  ├─ /settings/tools (ToolsPage - placeholder)
│  ├─ /settings/knowledge-base (KnowledgeBasePage - placeholder)
│  └─ /settings/profile (ProfilePage - placeholder)
│
└─ Testing Flow (standalone)
   └─ /test-chat (TestChatPage)
      ├─ Query params: ?category={id} or ?skill={id}
      └─ Close button → returns to CategorySkillsPage
```

## Page Usage Statistics

| Page | Route | Fully Implemented | Primary Function | Navigated From |
|------|-------|-------------------|------------------|----------------|
| ChatPage | `/` or `/conversation/:threadId` | ✅ Yes | Main chat interface | Sidebar, Settings |
| TestChatPage | `/test-chat` | ✅ Yes | Test agents/skills | CategoriesPage, CategorySkillsPage |
| CategoriesPage | `/settings/categories` | ✅ Yes | Manage agent categories | Settings menu |
| CategorySkillsPage | `/settings/categories/:categoryId/skills` | ✅ Yes | Manage skills in category | CategoriesPage |
| KnowledgeBasePage | `/settings/knowledge-base` | ❌ Placeholder | Future: Knowledge base | Settings menu |
| ProfilePage | `/settings/profile` | ❌ Placeholder | Future: User profile | Settings menu |
| ToolsPage | `/settings/tools` | ❌ Placeholder | Future: Tool management | Settings menu |

---

## Component-to-Page Relationship

### Most Reused Components
1. **FormField** - Used in CategoryModal and SkillModal (2 modals)
2. **ConfirmDialog** - Used in CategoriesPage and CategorySkillsPage (2 pages)
3. **Thread** - Used in ChatPage (via ChatRuntime) and TestChatPage (2 pages)
4. **MarkdownText/CodeBlock** - Used in both chat pages via Thread (2 pages)

### Page-Specific Components
- **ChatRuntime** - Only used in ChatPage
- **SkillsSidebar, SkillInstructionsModal** - Only used in TestChatPage
- **DataTable** - Currently only used in CategorySkillsPage (highly reusable)
- **SkillRoutersSection** - Only used within SkillModal

### Utility/Export-Only Components (No Direct Usage Yet)
- **ImageContent** - Exported for future message rendering
- **FileAttachment** - Exported for future message rendering
- **ToolCallUI** - Exported for future tool call visualization
