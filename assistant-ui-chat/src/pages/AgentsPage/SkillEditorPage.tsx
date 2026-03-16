import { type FC, useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeftIcon, SaveIcon, FileTextIcon, RouteIcon, WrenchIcon } from "lucide-react";
import { useSkills, useSkillRouters } from "../../hooks";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "../../components/Tabs";
import { MarkdownEditor, SkillRoutersSection, SkillToolsTab } from "./components";
import type { CreateSkillRequest, UpdateSkillRequest } from "../../types";
import "./SkillEditorPage.css";

// =============================================================================
// Component
// =============================================================================

export const SkillEditorPage: FC = () => {
  const { agentId, skillId } = useParams<{ agentId: string; skillId?: string }>();
  const navigate = useNavigate();
  
  const isEditMode = !!skillId;
  
  // Skills hook
  const {
    skills,
    agent,
    loading: skillsLoading,
    create,
    update,
  } = useSkills(agentId);

  // Routers hook
  const {
    routers,
    loading: routersLoading,
    error: routersError,
    fetchBySkillCode,
    create: createRouter,
    remove: removeRouter,
  } = useSkillRouters();

  // Form state
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [systemPrompt, setSystemPrompt] = useState("");
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [activeTab, setActiveTab] = useState("prompt");

  const currentSkill = skills.find((s) => s.id === skillId);

  // ---------------------------------------------------------------------------
  // Load skill data if editing
  // ---------------------------------------------------------------------------
  useEffect(() => {
    if (isEditMode && currentSkill) {
      setCode(currentSkill.code || "");
      setName(currentSkill.name || "");
      setSystemPrompt(currentSkill.systemPrompt || "");
      
      // Fetch routers
      if (currentSkill.code) {
        fetchBySkillCode(currentSkill.code);
      }
    }
  }, [isEditMode, currentSkill, fetchBySkillCode]);

  // ---------------------------------------------------------------------------
  // Validate
  // ---------------------------------------------------------------------------
  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!code.trim()) newErrors.code = "Code is required";
    if (!name.trim()) newErrors.name = "Name is required";
    if (!systemPrompt.trim()) newErrors.systemPrompt = "System prompt is required";

    setErrors(newErrors);
    
    if (newErrors.systemPrompt) {
      setActiveTab("prompt");
    }
    
    return Object.keys(newErrors).length === 0;
  };

  // ---------------------------------------------------------------------------
  // Save handler
  // ---------------------------------------------------------------------------
  const handleSave = async () => {
    if (!validate() || !agentId) return;

    setSaving(true);

    try {
      const data = {
        code: code.trim(),
        name: name.trim(),
        systemPrompt: systemPrompt.trim(),
      } as UpdateSkillRequest;

      if (isEditMode && currentSkill) {
        await update(currentSkill.id, data);
      } else {
        await create({
          ...data,
          agentId,
        } as CreateSkillRequest);
      }
      
      // Navigate back
      navigate(`/settings/agents/${agentId}/skills`);
    } catch (e) {
      setErrors({ form: e instanceof Error ? e.message : "Failed to save" });
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------------------------
  // Router handlers
  // ---------------------------------------------------------------------------
  const handleAddRouter = async (userQueries: string) => {
    if (!currentSkill) return;
    
    await createRouter({
      skillCode: currentSkill.code,
      skillName: currentSkill.name,
      userQueries
    });
  };

  const handleRemoveRouter = async (id: string) => {
    await removeRouter(id);
  };

  const handleRefreshRouters = () => {
    if (currentSkill?.code) {
      fetchBySkillCode(currentSkill.code);
    }
  };

  // ---------------------------------------------------------------------------
  // Back handler
  // ---------------------------------------------------------------------------
  const handleBack = () => {
    navigate(`/settings/agents/${agentId}/skills`);
  };

  // ---------------------------------------------------------------------------
  // Keyboard shortcuts
  // ---------------------------------------------------------------------------
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === "s") {
        e.preventDefault();
        handleSave();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [code, name, systemPrompt, agentId, isEditMode, currentSkill]);

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------
  return (
    <div className="skill-editor-page">
      {/* Header */}
      <header className="skill-editor-header">
        <div className="header-left">
          <button className="back-btn" onClick={handleBack}>
            <ArrowLeftIcon size={20} />
          </button>
          <div className="breadcrumb">
            <span className="breadcrumb-parent" onClick={handleBack}>
              {agent?.name || "Agent"}
            </span>
            <span className="breadcrumb-separator">/</span>
            <span className="breadcrumb-current">
              {isEditMode ? "Edit Skill" : "New Skill"}
            </span>
          </div>
        </div>
        <button 
          className="btn btn-primary" 
          onClick={handleSave}
          disabled={saving || skillsLoading}
        >
          <SaveIcon size={18} />
          <span>{saving ? "Saving..." : isEditMode ? "Update" : "Create"}</span>
        </button>
      </header>

      {/* Body */}
      <div className="skill-editor-body">
        {errors.form && <div className="form-error">{errors.form}</div>}

        {/* Basic Info */}
        <div className="skill-basic-info">
          <div className="form-field">
            <label htmlFor="skill-code">
              Code <span className="required">*</span>
              {errors.code && <span className="field-error">{errors.code}</span>}
            </label>
            <input
              id="skill-code"
              type="text"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="e.g., ecommerce-search"
              disabled={isEditMode}
            />
          </div>

          <div className="form-field">
            <label htmlFor="skill-name">
              Name <span className="required">*</span>
              {errors.name && <span className="field-error">{errors.name}</span>}
            </label>
            <input
              id="skill-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter skill name"
            />
          </div>
        </div>

        {/* Tabs - NEW IMPLEMENTATION */}
        <Tabs value={activeTab} onChange={setActiveTab}>
          <TabsList>
            {/* System Prompt Tab */}
            <TabsTrigger 
              value="prompt" 
              icon={<FileTextIcon size={16} />}
            >
              System Prompt
              {errors.systemPrompt && <span className="error-indicator">●</span>}
            </TabsTrigger>

            {/* Routing Queries Tab */}
            <TabsTrigger 
              value="routing" 
              icon={<RouteIcon size={16} />}
              badge={routers.length}
              disabled={!isEditMode}
            >
              Routing Queries
            </TabsTrigger>

            {/* Tools Tab - NEW */}
            <TabsTrigger 
              value="tools" 
              icon={<WrenchIcon size={16} />}
              badge={currentSkill?.connectedToolsCount || 0}
              disabled={!isEditMode}
            >
              Tools
            </TabsTrigger>
          </TabsList>

          {/* System Prompt Content */}
          <TabsContent value="prompt">
            <MarkdownEditor
              value={systemPrompt}
              onChange={setSystemPrompt}
              label="System Prompt (Instructions)"
              required
              error={errors.systemPrompt}
              placeholder={`Write your system prompt using Markdown...

# Example Structure

## Role
You are a helpful assistant that...

## Instructions
1. First, understand the user's request
2. Then, provide a clear response
3. Always be polite and professional

## Constraints
- Keep responses concise
- Use simple language
- Avoid technical jargon`}
            />
          </TabsContent>

          {/* Routing Queries Content */}
          <TabsContent value="routing">
            <SkillRoutersSection
              routers={routers}
              loading={routersLoading}
              error={routersError}
              isNewSkill={!isEditMode}
              onAdd={handleAddRouter}
              onRemove={handleRemoveRouter}
              onRefresh={handleRefreshRouters}
            />
          </TabsContent>

          {/* Tools Content - NEW */}
          <TabsContent value="tools">
            {isEditMode && currentSkill ? (
              <SkillToolsTab skillId={currentSkill.id} />
            ) : (
              <div className="tab-disabled-message">
                <p>Save the skill first to manage tools</p>
              </div>
            )}
          </TabsContent>
        </Tabs>
      </div>

      {/* Footer hint */}
      <footer className="skill-editor-footer">
        <div className="footer-hint">
          <kbd>Ctrl</kbd> + <kbd>S</kbd> to save
        </div>
      </footer>
    </div>
  );
};