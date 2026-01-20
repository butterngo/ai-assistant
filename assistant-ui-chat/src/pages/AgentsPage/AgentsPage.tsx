import { type FC, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  LayersIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  ChevronRightIcon,
  BrainIcon,
  MessageSquareIcon,
} from "lucide-react";
import { useAgents } from "../../hooks";
import { AgentModal, ConfirmDialog } from "../../components";
import type { Agent, CreateAgentRequest, UpdateAgentRequest } from "../../types";
import "../SettingsPage.css";
import "./AgentsPage.css";

// =============================================================================
// Component
// =============================================================================

export const AgentsPage: FC = () => {
  const navigate = useNavigate();
  const { agents, loading, error, create, update, remove, fetchAll } = useAgents();

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingAgent, setEditingAgent] = useState<Agent | null>(null);

  // Delete confirmation state
  const [deleteAgent, setDeleteAgent] = useState<Agent | null>(null);
  const [deleting, setDeleting] = useState(false);

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------

  const handleChat = (agent: Agent, e: React.MouseEvent) => {
  e.stopPropagation();
  navigate(`/test-chat?agents=${agent.id}`, {
    state: { categoryName: agent.name }
    });
  };

  const handleCreate = () => {
    setEditingAgent(null);
    setIsModalOpen(true);
  };

  const handleEdit = (e: React.MouseEvent, agent: Agent) => {
    e.stopPropagation();
    setEditingAgent(agent);
    setIsModalOpen(true);
  };

  const handleSave = async (data: CreateAgentRequest | UpdateAgentRequest) => {
    if (editingAgent) {
      await update(editingAgent.id, data);
    } else {
      await create(data as CreateAgentRequest);
    }
  };

  const handleDeleteClick = (e: React.MouseEvent, agent: Agent) => {
    e.stopPropagation();
    setDeleteAgent(agent);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteAgent) return;
    setDeleting(true);
    try {
      await remove(deleteAgent.id);
      setDeleteAgent(null);
    } catch (e) {
      console.error("Failed to delete:", e);
    } finally {
      setDeleting(false);
    }
  };

  const handleViewSkills = (agent: Agent) => {
    navigate(`/settings/agents/${agent.id}/skills`);
  };

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------

  return (
    <div className="settings-page">
      {/* Header */}
      <header className="settings-page-header">
        <div className="settings-page-title">
          <LayersIcon size={24} />
          <h1>Agents</h1>
        </div>
        <button className="btn btn-primary" onClick={handleCreate}>
          <PlusIcon size={18} />
          <span>Add Agent</span>
        </button>
      </header>

      {/* Content */}
      <div className="settings-page-content">
        {/* Error State */}
        {error && (
          <div className="error-banner">
            <span>{error}</span>
            <button onClick={fetchAll}>Retry</button>
          </div>
        )}

        {/* Loading State */}
        {loading && (
          <div className="loading-state">
            <div className="loading-spinner" />
            <p>Loading agents...</p>
          </div>
        )}

        {/* Empty State */}
        {!loading && !error && agents.length === 0 && (
          <div className="empty-state">
            <LayersIcon size={48} />
            <h3>No categories yet</h3>
            <p>Create your first agent to organize your skills.</p>
            <button className="btn btn-primary" onClick={handleCreate}>
              <span>Add Category</span>
            </button>
          </div>
        )}

        {/* Categories List */}
        {!loading && agents.length > 0 && (
          <div className="categories-list">
            {agents.map((agent : Agent) => (
              <div
                key={agent.id}
                className="category-card clickable"
                onClick={() => handleViewSkills(agent)}
              >
                <div className="category-card-main">
                  <div className="category-icon">
                    <LayersIcon size={20} />
                  </div>
                  <div className="category-info">
                    <h3>{agent.name}</h3>
                    {agent.description && <p>{agent.description}</p>}
                  </div>
                  <div className="category-meta">
                    <div className="skill-count">
                      <BrainIcon size={14} />
                      <span>{agent.skillCount} skills</span>
                    </div>
                    <ChevronRightIcon size={20} className="chevron-icon" />
                  </div>
                </div>
                <div className="category-card-actions">
                  <button
                    className="icon-btn chat-btn"
                    onClick={(e) => handleChat(agent, e)}
                    title="Test in chat"
                  >
                    <MessageSquareIcon size={16} />
                  </button>
                  <button
                    className="action-btn"
                    onClick={(e) => handleEdit(e, agent)}
                    title="Edit"
                  >
                    <PencilIcon size={16} />
                  </button>
                  <button
                    className="action-btn danger"
                    onClick={(e) => handleDeleteClick(e, agent)}
                    title="Delete"
                  >
                    <TrashIcon size={16} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Create/Edit Modal */}
      <AgentModal
        isOpen={isModalOpen}
        agent={editingAgent}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSave}
      />

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteAgent}
        title="Delete Agent"
        message={`Are you sure you want to delete "${deleteAgent?.name}"? This will also delete all skills inside this agent.`}
        confirmLabel="Delete"
        danger
        loading={deleting}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteAgent(null)}
      />
    </div>
  );
};
