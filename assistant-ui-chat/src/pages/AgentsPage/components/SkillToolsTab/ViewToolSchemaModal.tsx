import { type FC, useState } from "react";
import { CopyIcon, CheckIcon } from "lucide-react";
import { Modal } from "../../../../components";
import type { DiscoveredTool } from "../../../../types";
import "./ViewToolSchemaModal.css";

interface ViewToolSchemaModalProps {
  isOpen: boolean;
  onClose: () => void;
  tool: DiscoveredTool | null;
}

export const ViewToolSchemaModal: FC<ViewToolSchemaModalProps> = ({
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
      title={`${tool.name} - JSON Schema`}
      size="xl"
    >
      <div className="view-tool-schema-modal">
        {/* Tool Info */}
        <div className="schema-info">
          <h4>{tool.name}</h4>
          <p>{tool.description}</p>
        </div>

        {/* Copy Button */}
        <button className="copy-json-btn" onClick={handleCopyJson}>
          {copied ? (
            <>
              <CheckIcon size={16} />
              Copied!
            </>
          ) : (
            <>
              <CopyIcon size={16} />
              Copy JSON
            </>
          )}
        </button>

        {/* JSON Display */}
        <div className="json-viewer">
          <pre>
            <code>{JSON.stringify(tool.toolSchema, null, 2)}</code>
          </pre>
        </div>

        {/* Metadata */}
        <div className="schema-metadata">
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
            <span className={`status-badge ${tool.isAvailable ? "available" : "unavailable"}`}>
              {tool.isAvailable ? "Available" : "Unavailable"}
            </span>
          </div>
        </div>
      </div>
    </Modal>
  );
};