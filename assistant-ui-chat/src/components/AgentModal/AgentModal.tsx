import { type FC, useState, useEffect } from "react";
import { XIcon } from "lucide-react";
import type { Agent, CreateAgentRequest, UpdateAgentRequest } from "../../types";
import "./AgentModal.css";
import { FormField } from "../Form";

// =============================================================================
// Types
// =============================================================================

export interface AgentModalProps {
  isOpen: boolean;
  agent?: Agent | null;
  onClose: () => void;
  onSave: (data: CreateAgentRequest | UpdateAgentRequest) => Promise<void>;
}

// =============================================================================
// Component
// =============================================================================

export const AgentModal: FC<AgentModalProps> = ({
  isOpen,
  agent,
  onClose,
  onSave,
}) => {
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const isEdit = !!agent;

  // ---------------------------------------------------------------------------
  // Reset form when modal opens
  // ---------------------------------------------------------------------------
  useEffect(() => {
    if (isOpen) {
      setCode(agent?.code || "");
      setName(agent?.name || "");
      setDescription(agent?.description || "");
      setErrors({});
    }
  }, [isOpen, agent]);

  // ---------------------------------------------------------------------------
  // Handle save
  // ---------------------------------------------------------------------------
  const handleSave = async () => {
    if (!code.trim()) {
      setErrors({ code: "Code is required" });
      return;
    }
    if (!name.trim()) {
      setErrors({ name: "Name is required" });
      return;
    }

    setSaving(true);
    setErrors({});

    try {
      await onSave({
        code: code.trim(),
        name: name.trim(),
        description: description.trim() || null,
      });
      onClose();
    } catch (e) {
      //setErrors(e instanceof Error ? e.message : "Failed to save");
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------------------------
  // Handle key down
  // ---------------------------------------------------------------------------
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Escape") {
      onClose();
    } else if (e.key === "Enter" && e.ctrlKey) {
      handleSave();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose} onKeyDown={handleKeyDown}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div className="modal-header">
          <h2>{isEdit ? "Edit Agent" : "New Agent"}</h2>
          <button className="modal-close-btn" onClick={onClose}>
            <XIcon size={20} />
          </button>
        </div>

        {/* Body */}
        <div className="modal-body">
          <FormField label="Code" htmlFor="skill-code" required error={errors.code}>
              <input
                id="Agent-code"
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder="Enter Agent code"
              />
          </FormField>

          <FormField label="Name" htmlFor="Agent-name" required error={errors.name}>
              <input
                id="Agent-name"
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter Agent name"
              />
          </FormField>
          
          <div className="form-group">
            <label htmlFor="Agent-description">Description</label>
            <textarea
              id="Agent-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Enter Agent description (optional)"
              rows={3}
            />
          </div>
        </div>

        {/* Footer */}
        <div className="modal-footer">
          <button className="btn btn-secondary" onClick={onClose} disabled={saving}>
            Cancel
          </button>
          <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
            {saving ? "Saving..." : isEdit ? "Update" : "Create"}
          </button>
        </div>
      </div>
    </div>
  );
};