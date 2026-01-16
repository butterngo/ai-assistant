import { type FC, useState, useEffect } from "react";
import { XIcon } from "lucide-react";
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from "../../types";
import "./CategoryModal.css";
import { FormField } from "../Form";

// =============================================================================
// Types
// =============================================================================

export interface CategoryModalProps {
  isOpen: boolean;
  category?: Category | null;
  onClose: () => void;
  onSave: (data: CreateCategoryRequest | UpdateCategoryRequest) => Promise<void>;
}

// =============================================================================
// Component
// =============================================================================

export const CategoryModal: FC<CategoryModalProps> = ({
  isOpen,
  category,
  onClose,
  onSave,
}) => {
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const isEdit = !!category;

  // ---------------------------------------------------------------------------
  // Reset form when modal opens
  // ---------------------------------------------------------------------------
  useEffect(() => {
    if (isOpen) {
      setCode(category?.code || "");
      setName(category?.name || "");
      setDescription(category?.description || "");
      setErrors({});
    }
  }, [isOpen, category]);

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
          <h2>{isEdit ? "Edit Category" : "New Category"}</h2>
          <button className="modal-close-btn" onClick={onClose}>
            <XIcon size={20} />
          </button>
        </div>

        {/* Body */}
        <div className="modal-body">
          <FormField label="Code" htmlFor="skill-code" required error={errors.code}>
              <input
                id="category-code"
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder="Enter category code"
              />
          </FormField>

          <FormField label="Name" htmlFor="category-name" required error={errors.name}>
              <input
                id="category-name"
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter category name"
              />
          </FormField>
          
          <div className="form-group">
            <label htmlFor="category-description">Description</label>
            <textarea
              id="category-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Enter category description (optional)"
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

export default CategoryModal;