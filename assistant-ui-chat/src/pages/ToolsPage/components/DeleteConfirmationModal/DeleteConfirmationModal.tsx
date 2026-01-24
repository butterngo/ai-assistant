import type { FC } from "react";
import { Modal } from "../../../../components";
import "./DeleteConfirmationModal.css";

export interface DeleteConfirmationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  connectionName: string;
  toolsCount?: number;
  skillsCount?: number;
  loading?: boolean;
}

export const DeleteConfirmationModal: FC<DeleteConfirmationModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  connectionName,
  toolsCount = 0,
  skillsCount = 0,
  loading = false,
}) => {
  const footer = (
    <>
      <button className="secondary-btn" onClick={onClose} disabled={loading}>
        Cancel
      </button>
      <button className="danger-btn" onClick={onConfirm} disabled={loading}>
        {loading ? "Deleting..." : "Delete"}
      </button>
    </>
  );

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Delete Connection"
      footer={footer}
      size="sm"
      closeOnOverlayClick={!loading}
      closeOnEscape={!loading}
    >
      <div className="delete-modal-content">
        <div className="warning-icon">⚠️</div>

        <p className="warning-text">
          Are you sure you want to delete this connection?
        </p>

        <div className="connection-info">
          <strong>Name:</strong> {connectionName}
        </div>

        <div className="impact-info">
          <p>This will:</p>
          <ul>
            <li>Remove the connection configuration</li>
            {toolsCount > 0 && <li>Delete {toolsCount} cached discovered tools</li>}
            {skillsCount > 0 && (
              <li>
                Unlink from {skillsCount} skill{skillsCount > 1 ? "s" : ""}
              </li>
            )}
          </ul>
        </div>

        <div className="warning-message">⚠️ This action cannot be undone</div>
      </div>
    </Modal>
  );
};