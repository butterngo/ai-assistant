import { type FC, useState, useEffect } from "react";
import { Modal, FormField } from "../../../../components";
import type { DiscoveredTool, UpdateDiscoveredToolRequest } from "../../../../types";
import "./EditToolDescriptionModal.css";

interface EditToolDescriptionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (toolId: string, request: UpdateDiscoveredToolRequest) => Promise<void>;
  tool: DiscoveredTool | null;
}

export const EditToolDescriptionModal: FC<EditToolDescriptionModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  tool,
}) => {
  const [loading, setLoading] = useState(false);
  const [description, setDescription] = useState("");
  const [isAvailable, setIsAvailable] = useState(true);
  const [error, setError] = useState("");

  // Load tool data when modal opens
  useEffect(() => {
    if (isOpen && tool) {
      setDescription(tool.description);
      setIsAvailable(tool.isAvailable);
      setError("");
    }
  }, [isOpen, tool]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!tool) return;

    // Validation
    if (!description.trim()) {
      setError("Description is required");
      return;
    }

    setLoading(true);
    setError("");

    try {
      await onSubmit(tool.id, {
        description: description.trim(),
        isAvailable,
      });
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update tool");
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    if (!loading) {
      onClose();
    }
  };

  if (!tool) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleCancel}
      title="Edit Tool"
      size="md"
    >
      <form onSubmit={handleSubmit} className="edit-tool-form">
        {/* Tool Name (Read-only) */}
        <FormField label="Tool Name" required>
          <input
            type="text"
            value={tool.name}
            disabled
            className="form-input disabled"
          />
        </FormField>

        {/* Description */}
        <FormField 
          label="Description" 
          required
          error={error}
          hint="This only updates the display description, not the actual tool schema"
        >
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Enter tool description..."
            rows={4}
            className="form-textarea"
            disabled={loading}
          />
        </FormField>

        {/* Availability Toggle */}
        <div className="availability-toggle">
          <label className="toggle-label">
            <input
              type="checkbox"
              checked={isAvailable}
              onChange={(e) => setIsAvailable(e.target.checked)}
              disabled={loading}
              className="toggle-checkbox"
            />
            <span className="toggle-switch" />
            <span className="toggle-text">
              {isAvailable ? "Available" : "Unavailable"}
            </span>
          </label>
          <p className="toggle-hint">
            Unavailable tools won't be offered to AI agents
          </p>
        </div>

        {/* Metadata Info */}
        <div className="tool-metadata">
          <div className="metadata-row">
            <span className="metadata-label">Discovered:</span>
            <span className="metadata-value">
              {new Date(tool.discoveredAt).toLocaleString()}
            </span>
          </div>
          <div className="metadata-row">
            <span className="metadata-label">Last Verified:</span>
            <span className="metadata-value">
              {new Date(tool.lastVerifiedAt).toLocaleString()}
            </span>
          </div>
        </div>

        {/* Form Actions */}
        <div className="modal-actions">
          <button
            type="button"
            onClick={handleCancel}
            className="secondary-btn"
            disabled={loading}
          >
            Cancel
          </button>
          <button
            type="submit"
            className="primary-btn"
            disabled={loading}
          >
            {loading ? (
              <>
                <div className="btn-spinner" />
                Saving...
              </>
            ) : (
              "Save Changes"
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};