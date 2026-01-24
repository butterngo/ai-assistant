import { type FC, useState } from "react";
import { PlusIcon, XIcon, AlertCircleIcon, RefreshCwIcon } from "lucide-react";
import type { SkillRouter } from "../../../../types";
import "./SkillRoutersSection.css";

// =============================================================================
// Types
// =============================================================================

interface SkillRoutersSectionProps {
  routers: SkillRouter[];
  loading: boolean;
  error: string | null;
  isNewSkill: boolean;
  onAdd: (userQuery: string) => Promise<void>;
  onRemove: (id: string) => Promise<void>;
  onRefresh: () => void;
}

// =============================================================================
// Component
// =============================================================================

export const SkillRoutersSection: FC<SkillRoutersSectionProps> = ({
  routers,
  loading,
  error,
  isNewSkill,
  onAdd,
  onRemove,
  onRefresh,
}) => {
  const [newQuery, setNewQuery] = useState("");
  const [adding, setAdding] = useState(false);

  // ---------------------------------------------------------------------------
  // Add new query
  // ---------------------------------------------------------------------------
  const handleAddQuery = async () => {
    if (!newQuery.trim()) return;

    setAdding(true);
    try {
      await onAdd(newQuery.trim());
      setNewQuery("");
    } finally {
      setAdding(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleAddQuery();
    }
  };

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------
  return (
    <div className="skill-routers-section">
      <div className="section-header">
        <div className="section-title">
          <h3>Routing Queries</h3>
          <span className="query-count">{routers.length} queries</span>
        </div>
        {!isNewSkill && (
          <button
            className="icon-btn refresh-btn"
            onClick={onRefresh}
            disabled={loading}
            title="Refresh"
          >
            <RefreshCwIcon size={16} className={loading ? "spinning" : ""} />
          </button>
        )}
      </div>

      <p className="section-description">
        Add example queries that should route to this skill. Each query creates a separate
        vector embedding for better semantic matching.
      </p>

      {/* Error */}
      {error && (
        <div className="section-error">
          <AlertCircleIcon size={16} />
          <span>{error}</span>
        </div>
      )}

      {/* New Skill Notice */}
      {isNewSkill && (
        <div className="new-skill-notice">
          <AlertCircleIcon size={16} />
          <span>Save the skill first to add routing queries.</span>
        </div>
      )}

      {/* Query List */}
      {!isNewSkill && (
        <>
          <div className="routers-list">
            {loading && routers.length === 0 ? (
              <div className="loading-state">Loading queries...</div>
            ) : routers.length === 0 ? (
              <div className="empty-state">
                No routing queries yet. Add some examples below.
              </div>
            ) : (
              routers.map((router) => (
                <div key={router.id} className="router-item">
                  <span className="router-query">{router.userQueries}</span>
                  <button
                    className="remove-btn"
                    onClick={() => onRemove(router.id)}
                    title="Remove query"
                  >
                    <XIcon size={14} />
                  </button>
                </div>
              ))
            )}
          </div>

          {/* Add New Query */}
          <div className="add-query-form">
            <input
              type="text"
              value={newQuery}
              onChange={(e) => setNewQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Type a new routing query..."
              disabled={adding}
            />
            <button
              className="btn btn-secondary add-btn"
              onClick={handleAddQuery}
              disabled={!newQuery.trim() || adding}
            >
              <PlusIcon size={16} />
              <span>{adding ? "Adding..." : "Add"}</span>
            </button>
          </div>

          <p className="section-hint">
            Press <kbd>Enter</kbd> to add quickly
          </p>
        </>
      )}
    </div>
  );
};
