import { type FC, useState } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { EyeIcon, EditIcon } from "lucide-react";
import "./MarkdownEditor.css";

// =============================================================================
// Types
// =============================================================================

export type EditorMode = "edit" | "preview" | "split";

export interface MarkdownEditorProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  error?: string;
  label?: string;
  required?: boolean;
  showModeToggle?: boolean;
  defaultMode?: EditorMode;
}

// =============================================================================
// Component
// =============================================================================

export const MarkdownEditor: FC<MarkdownEditorProps> = ({
  value,
  onChange,
  placeholder = "Write your content using Markdown...",
  error,
  label,
  required = false,
  showModeToggle = true,
  defaultMode = "split",
}) => {
  const [editorMode, setEditorMode] = useState<EditorMode>(defaultMode);

  return (
    <div className="markdown-editor">
      {/* Header */}
      <div className="markdown-editor-header">
        <div className="header-left">
          {label && (
            <label className="editor-label">
              {label}
              {required && <span className="required-mark">*</span>}
            </label>
          )}
          {error && <span className="editor-error">{error}</span>}
        </div>
        
        {/* Mode toggle buttons */}
        {showModeToggle && (
          <div className="mode-toggle">
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
          </div>
        )}
      </div>

      {/* Editor Container */}
      <div className={`editor-container mode-${editorMode}`}>
        {/* Editor Panel */}
        {(editorMode === "edit" || editorMode === "split") && (
          <div className="editor-panel">
            <div className="panel-header">
              <span>Markdown</span>
              <span className="char-count">{value.length} characters</span>
            </div>
            <textarea
              value={value}
              onChange={(e) => onChange(e.target.value)}
              placeholder={placeholder}
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
              {value ? (
                <ReactMarkdown remarkPlugins={[remarkGfm]}>
                  {value}
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
  );
};