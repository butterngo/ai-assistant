import { type FC, useState, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import {
  XIcon,
  EyeIcon,
  EditIcon,
  MaximizeIcon,
  MinimizeIcon,
} from "lucide-react";
import { FormField } from "../Form";
import type { Skill, Category, CreateSkillRequest, UpdateSkillRequest } from "../../types";
import "./SkillModal.css";

// =============================================================================
// Types
// =============================================================================

interface SkillModalProps {
  isOpen: boolean;
  skill?: Skill | null;
  categories: Category[];
  onClose: () => void;
  onSave: (data: CreateSkillRequest | UpdateSkillRequest) => Promise<void>;
}

type EditorMode = "edit" | "preview" | "split";

// =============================================================================
// Component
// =============================================================================

export const SkillModal: FC<SkillModalProps> = ({
  isOpen,
  skill,
  categories,
  onClose,
  onSave,
}) => {
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [systemPrompt, setSystemPrompt] = useState("");
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [editorMode, setEditorMode] = useState<EditorMode>("split");
  const [isFullscreen, setIsFullscreen] = useState(true);

  const isEdit = !!skill;
  const singleCategory = categories.length === 1;

  // ---------------------------------------------------------------------------
  // Reset form when modal opens
  // ---------------------------------------------------------------------------
  useEffect(() => {
    if (isOpen) {
      setCode(skill?.code || "");
      setName(skill?.name || "");
      setDescription(skill?.description || "");
      setSystemPrompt(skill?.systemPrompt || "");
      setErrors({});
      setEditorMode("split");
      setIsFullscreen(true);
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

    if (!name.trim()) newErrors.name = "Name is required";
    if (!description.trim()) newErrors.description = "Description is required";
    if (!systemPrompt.trim()) newErrors.systemPrompt = "System prompt is required";

    setErrors(newErrors);
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
        description: description.trim(),
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
            {singleCategory && (
              <span className="category-badge">{categories[0].name}</span>
            )}
          </div>
          <div className="skill-modal-actions">
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
          {/* Form Error */}
          {errors.form && <div className="form-error">{errors.form}</div>}

          {/* Basic Info Section */}
          <div className="skill-modal-info">
            <FormField label="Code" htmlFor="skill-code" required error={errors.code}>
              <input
                id="skill-code"
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder="Enter skill code"
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

            <FormField
              label="Description"
              htmlFor="skill-description"
              required
              error={errors.description}
              hint="Used for embedding vector generation and skill routing"
            >
              <textarea
                id="skill-description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Describe what this skill does, what queries it handles, and its capabilities. This text will be used to generate embeddings for semantic search and skill routing."
                rows={4}
                className="description-textarea"
              />
              <div className="textarea-footer">
                <span className="char-count">{description.length} characters</span>
              </div>
            </FormField>
          </div>

          {/* System Prompt Editor */}
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

export default SkillModal;