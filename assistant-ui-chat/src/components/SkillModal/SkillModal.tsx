import { type FC, useState, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import {
  XIcon,
  EyeIcon,
  EditIcon,
  MaximizeIcon,
  MinimizeIcon,
  FileTextIcon,
  RouteIcon,
} from "lucide-react";
import { FormField } from "../Form";
import { SkillRoutersSection } from "./SkillRoutersSection";
import type { Skill, Agent, CreateSkillRequest, UpdateSkillRequest, SkillRouter } from "../../types";
import "./SkillModal.css";

// =============================================================================
// Types
// =============================================================================

interface SkillModalProps {
  isOpen: boolean;
  skill?: Skill | null;
  agents: Agent[];
  routers: SkillRouter[];
  routersLoading: boolean;
  routersError: string | null;
  onClose: () => void;
  onSave: (data: CreateSkillRequest | UpdateSkillRequest) => Promise<void>;
  onAddRouter: (userQuery: string) => Promise<void>;
  onRemoveRouter: (id: string) => Promise<void>;
  onRefreshRouters: () => void;
}

type EditorMode = "edit" | "preview" | "split";
type TabType = "prompt" | "routing";

// =============================================================================
// Component
// =============================================================================

export const SkillModal: FC<SkillModalProps> = ({
  isOpen,
  skill,
  agents,
  routers,
  routersLoading,
  routersError,
  onClose,
  onSave,
  onAddRouter,
  onRemoveRouter,
  onRefreshRouters,
}) => {
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [systemPrompt, setSystemPrompt] = useState("");
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [editorMode, setEditorMode] = useState<EditorMode>("split");
  const [isFullscreen, setIsFullscreen] = useState(true);
  const [activeTab, setActiveTab] = useState<TabType>("prompt");

  const isEdit = !!skill;
  const isNewSkill = !skill;
  const singleAgent = agents.length === 1;

  // ---------------------------------------------------------------------------
  // Reset form when modal opens
  // ---------------------------------------------------------------------------
  useEffect(() => {
    if (isOpen) {
      setCode(skill?.code || "");
      setName(skill?.name || "");
      setSystemPrompt(skill?.systemPrompt || "");
      setErrors({});
      setEditorMode("split");
      setIsFullscreen(true);
      setActiveTab("prompt");
    }
  }, [isOpen, skill]);

  // ---------------------------------------------------------------------------
  // Handle escape key and save shortcut
  // ---------------------------------------------------------------------------
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      }
      if ((e.ctrlKey || e.metaKey) && e.key === "s") {
        e.preventDefault();
        handleSave();
      }
    };

    if (isOpen) {
      document.addEventListener("keydown", handleKeyDown);
      document.body.style.overflow = "hidden";
    }

    return () => {
      document.removeEventListener("keydown", handleKeyDown);
      document.body.style.overflow = "";
    };
  }, [isOpen, onClose]);

  // ---------------------------------------------------------------------------
  // Validate form
  // ---------------------------------------------------------------------------
  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!code.trim()) newErrors.code = "Code is required";
    if (!name.trim()) newErrors.name = "Name is required";
    if (!systemPrompt.trim()) newErrors.systemPrompt = "System prompt is required";

    setErrors(newErrors);
    
    // Switch to prompt tab if there's a system prompt error
    if (newErrors.systemPrompt) {
      setActiveTab("prompt");
    }
    
    return Object.keys(newErrors).length === 0;
  };

  // ---------------------------------------------------------------------------
  // Handle save
  // ---------------------------------------------------------------------------
  const handleSave = async () => {
    if (!validate()) return;

    setSaving(true);

    try {
      await onSave({
        code: code.trim(),
        name: name.trim(),
        systemPrompt: systemPrompt.trim(),
      });
      onClose();
    } catch (e) {
      setErrors({ form: e instanceof Error ? e.message : "Failed to save" });
    } finally {
      setSaving(false);
    }
  };

  if (!isOpen) return null;

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------
  return (
    <div className="skill-modal-overlay">
      <div className={`skill-modal ${isFullscreen ? "fullscreen" : ""}`}>
        {/* Header */}
        <header className="skill-modal-header">
          <div className="skill-modal-title">
            <h2>{isEdit ? "Edit Skill" : "New Skill"}</h2>
            {singleAgent && (
              <span className="agent-badge">{agents[0].name}</span>
            )}
          </div>
          <div className="skill-modal-actions">
            {/* Editor mode buttons - only show on prompt tab */}
            {activeTab === "prompt" && (
              <>
                <button
                  className={`mode-btn ${editorMode === "edit" ? "active" : ""}`}
                  onClick={() => setEditorMode("edit")}
                  title="Edit mode"
                >
                  <EditIcon size={18} />
                </button>
                <button
                  className={`mode-btn ${editorMode === "split" ? "active" : ""}`}
                  onClick={() => setEditorMode("split")}
                  title="Split mode"
                >
                  <EditIcon size={14} />
                  <EyeIcon size={14} />
                </button>
                <button
                  className={`mode-btn ${editorMode === "preview" ? "active" : ""}`}
                  onClick={() => setEditorMode("preview")}
                  title="Preview mode"
                >
                  <EyeIcon size={18} />
                </button>
                <div className="action-divider" />
              </>
            )}
            <button
              className="icon-btn"
              onClick={() => setIsFullscreen(!isFullscreen)}
              title={isFullscreen ? "Exit fullscreen" : "Fullscreen"}
            >
              {isFullscreen ? <MinimizeIcon size={18} /> : <MaximizeIcon size={18} />}
            </button>
            <button className="icon-btn" onClick={onClose} title="Close">
              <XIcon size={20} />
            </button>
          </div>
        </header>

        {/* Body */}
        <div className="skill-modal-body">
          {errors.form && <div className="form-error">{errors.form}</div>}

          {/* Basic Info Section - Always visible */}
          <div className="skill-modal-info">
            <FormField label="Code" htmlFor="skill-code" required error={errors.code}>
              <input
                id="skill-code"
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder="e.g., ecommerce-search"
                disabled={isEdit}
              />
            </FormField>
            <FormField label="Name" htmlFor="skill-name" required error={errors.name}>
              <input
                id="skill-name"
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter skill name"
              />
            </FormField>
          </div>

          {/* Tabs */}
          <div className="skill-modal-tabs">
            <button
              className={`tab-btn ${activeTab === "prompt" ? "active" : ""}`}
              onClick={() => setActiveTab("prompt")}
            >
              <FileTextIcon size={16} />
              <span>System Prompt</span>
              {errors.systemPrompt && <span className="tab-error-dot" />}
            </button>
            <button
              className={`tab-btn ${activeTab === "routing" ? "active" : ""}`}
              onClick={() => setActiveTab("routing")}
            >
              <RouteIcon size={16} />
              <span>Routing Queries</span>
              <span className="tab-count">{routers.length}</span>
            </button>
          </div>

          {/* Tab Content */}
          <div className="skill-modal-tab-content">
            {/* Tab 1: System Prompt */}
            {activeTab === "prompt" && (
              <div className="skill-modal-editor">
                <div className="editor-header">
                  <label>
                    System Prompt (Instructions) <span className="required-mark">*</span>
                  </label>
                  {errors.systemPrompt && (
                    <span className="editor-error">{errors.systemPrompt}</span>
                  )}
                </div>

                <div className={`editor-container mode-${editorMode}`}>
                  {/* Editor Panel */}
                  {(editorMode === "edit" || editorMode === "split") && (
                    <div className="editor-panel">
                      <div className="panel-header">
                        <span>Markdown</span>
                        <span className="char-count">{systemPrompt.length} characters</span>
                      </div>
                      <textarea
                        value={systemPrompt}
                        onChange={(e) => setSystemPrompt(e.target.value)}
                        placeholder="Write your system prompt using Markdown...

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
- Avoid technical jargon"
                        spellCheck={false}
                      />
                    </div>
                  )}

                  {/* Preview Panel */}
                  {(editorMode === "preview" || editorMode === "split") && (
                    <div className="preview-panel">
                      <div className="panel-header">
                        <span>Preview</span>
                      </div>
                      <div className="preview-content">
                        {systemPrompt ? (
                          <ReactMarkdown remarkPlugins={[remarkGfm]}>
                            {systemPrompt}
                          </ReactMarkdown>
                        ) : (
                          <p className="preview-placeholder">
                            Start typing to see the preview...
                          </p>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Tab 2: Routing Queries */}
            {activeTab === "routing" && (
              <SkillRoutersSection
                routers={routers}
                loading={routersLoading}
                error={routersError}
                isNewSkill={isNewSkill}
                onAdd={onAddRouter}
                onRemove={onRemoveRouter}
                onRefresh={onRefreshRouters}
              />
            )}
          </div>
        </div>

        {/* Footer */}
        <footer className="skill-modal-footer">
          <div className="footer-hint">
            <kbd>Ctrl</kbd> + <kbd>S</kbd> to save • <kbd>Esc</kbd> to close
          </div>
          <div className="footer-actions">
            <button className="btn btn-secondary" onClick={onClose} disabled={saving}>
              Cancel
            </button>
            <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
              {saving ? "Saving..." : isEdit ? "Update Skill" : "Create Skill"}
            </button>
          </div>
        </footer>
      </div>
    </div>
  );
};