// src/components/rich-content/ToolCallUI.tsx
import { useState } from "react";
import "./ToolCallUI.css";

interface ToolCallProps {
  toolCall: {
    id?: string;
    name: string;
    args?: Record<string, unknown>;
    result?: unknown;
    status?: "pending" | "running" | "success" | "error";
  };
}

export function ToolCallUI({ toolCall }: ToolCallProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const { name, args, result, status = "success" } = toolCall;

  const statusIcons: Record<string, string> = {
    pending: "⏳",
    running: "🔄",
    success: "✅",
    error: "❌",
  };

  const statusColors: Record<string, string> = {
    pending: "#f59e0b",
    running: "#3b82f6",
    success: "#10b981",
    error: "#ef4444",
  };

  return (
    <div className="tool-call" style={{ borderLeftColor: statusColors[status] }}>
      <div className="tool-header" onClick={() => setIsExpanded(!isExpanded)}>
        <div className="tool-info">
          <span className="tool-icon">🔧</span>
          <span className="tool-name">{name}</span>
          <span className="tool-status">{statusIcons[status]}</span>
        </div>
        <button className="expand-btn">{isExpanded ? "▼" : "▶"}</button>
      </div>

      {isExpanded && (
        <div className="tool-details">
          {args && Object.keys(args).length > 0 && (
            <div className="tool-section">
              <div className="tool-section-header">Arguments</div>
              <pre className="tool-json">{JSON.stringify(args, null, 2)}</pre>
            </div>
          )}

          {result !== undefined && (
            <div className="tool-section">
              <div className="tool-section-header">Result</div>
              <pre className="tool-json">
                {typeof result === "string" ? result : JSON.stringify(result, null, 2)}
              </pre>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default ToolCallUI;
