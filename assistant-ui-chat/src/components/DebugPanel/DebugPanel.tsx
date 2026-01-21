import { useState } from "react";
import { ChevronDownIcon, ChevronUpIcon, TrashIcon } from "lucide-react";
import type { DebugContext } from "../../types";
import "./DebugPanel.css";

interface DebugPanelProps {
  debugContext: DebugContext | null;
}

export function DebugPanel({ debugContext }: DebugPanelProps) {
  const [debugHistory, setDebugHistory] = useState<DebugContext[]>([]);
  const [collapsedItems, setCollapsedItems] = useState<Set<number>>(new Set());

  // Append new debug context to history
  if (debugContext && !debugHistory.find(d => d.threadId === debugContext.threadId && d.userMessage === debugContext.userMessage)) {
    setDebugHistory(prev => [...prev, debugContext]);
  }

  const clearHistory = () => {
    setDebugHistory([]);
    setCollapsedItems(new Set());
  };

  const removeItem = (index: number) => {
    setDebugHistory(prev => prev.filter((_, i) => i !== index));
    setCollapsedItems(prev => {
      const newSet = new Set(prev);
      newSet.delete(index);
      return newSet;
    });
  };

  const toggleItem = (index: number) => {
    setCollapsedItems(prev => {
      const newSet = new Set(prev);
      if (newSet.has(index)) {
        newSet.delete(index);
      } else {
        newSet.add(index);
      }
      return newSet;
    });
  };

  if (debugHistory.length === 0) {
    return (
      <div className="debug-panel">
        <div className="debug-panel-header">
          <div className="debug-panel-title">
            <span className="debug-icon">🐛</span>
            <span>Debug Info</span>
            <span className="debug-count">0</span>
          </div>
        </div>
        <div className="debug-empty-state">
          <p>No debug data yet</p>
          <p className="debug-empty-hint">Send a message to see debug info</p>
        </div>
      </div>
    );
  }

  return (
    <div className="debug-panel">
      {/* Header */}
      <div className="debug-panel-header">
        <div className="debug-panel-title">
          <span className="debug-icon">🐛</span>
          <span>Debug Info</span>
          <span className="debug-count">{debugHistory.length}</span>
        </div>
        <button 
          className="debug-clear-btn" 
          onClick={clearHistory}
          title="Clear all"
        >
          <TrashIcon size={16} />
        </button>
      </div>

      {/* Content */}
      <div className="debug-panel-content">
        {debugHistory.map((debug, index) => {
          const isCollapsed = collapsedItems.has(index);
          
          return (
            <div key={index} className="debug-item">
              {/* Item Header */}
              <div 
                className="debug-item-header"
                onClick={() => toggleItem(index)}
              >
                <div className="debug-item-info">
                  <span className="debug-item-number">#{debugHistory.length - index}</span>
                  <span className="debug-item-message">{debug.userMessage}</span>
                </div>
                <div className="debug-item-actions">
                  <button
                    className="debug-item-toggle"
                    onClick={(e) => {
                      e.stopPropagation();
                      toggleItem(index);
                    }}
                    title={isCollapsed ? "Expand" : "Collapse"}
                  >
                    {isCollapsed ? <ChevronDownIcon size={16} /> : <ChevronUpIcon size={16} />}
                  </button>
                  <button
                    className="debug-item-remove"
                    onClick={(e) => {
                      e.stopPropagation();
                      removeItem(index);
                    }}
                    title="Remove"
                  >
                    ×
                  </button>
                </div>
              </div>

              {/* JSON Display */}
              {!isCollapsed && (
                <pre className="debug-json">
                  {JSON.stringify(debug, null, 2)}
                </pre>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}