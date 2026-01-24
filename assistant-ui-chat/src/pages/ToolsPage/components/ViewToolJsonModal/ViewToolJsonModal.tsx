import { type FC, useState } from "react";
import { Modal } from "../../../../components/Modal";
import type { DiscoveredTool } from "../../../../types";
import "./ViewToolJsonModal.css";

interface ViewToolJsonModalProps {
  isOpen: boolean;
  onClose: () => void;
  tool: DiscoveredTool | null;
}

export const ViewToolJsonModal: FC<ViewToolJsonModalProps> = ({
  isOpen,
  onClose,
  tool,
}) => {
  const [copied, setCopied] = useState(false);

  if (!tool) return null;

  const handleCopyJson = async () => {
    try {
      const jsonString = JSON.stringify(tool.toolSchema, null, 2);
      await navigator.clipboard.writeText(jsonString);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (error) {
      console.error("Failed to copy JSON:", error);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`Tool Schema: ${tool.name}`}
      size="xl"
    >
      <div className="view-tool-json-modal">
        {/* Tool Info Header */}
        <div className="tool-json-header">
          <div className="tool-json-info">
            <h3>{tool.name}</h3>
            <p>{tool.description}</p>
          </div>
          <button
            className="copy-json-btn"
            onClick={handleCopyJson}
          >
            {copied ? "✅ Copied!" : "📋 Copy JSON"}
          </button>
        </div>

        {/* JSON Display */}
        <div className="json-viewer">
          <pre>
            <code>{JSON.stringify(tool.toolSchema, null, 2)}</code>
          </pre>
        </div>

        {/* Metadata Footer */}
        <div className="tool-json-footer">
          <div className="metadata-item">
            <span className="metadata-label">Discovered:</span>
            <span className="metadata-value">
              {new Date(tool.discoveredAt).toLocaleString()}
            </span>
          </div>
          <div className="metadata-item">
            <span className="metadata-label">Last Verified:</span>
            <span className="metadata-value">
              {new Date(tool.lastVerifiedAt).toLocaleString()}
            </span>
          </div>
          <div className="metadata-item">
            <span className="metadata-label">Status:</span>
            <span className={`metadata-badge ${tool.isAvailable ? "available" : "unavailable"}`}>
              {tool.isAvailable ? "Available" : "Unavailable"}
            </span>
          </div>
        </div>
      </div>
    </Modal>
  );
};