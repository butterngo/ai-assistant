import { type FC, useState, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  ArrowLeftIcon,
  BrainIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  LayersIcon,
  MessageSquareIcon,
} from "lucide-react";
import { useSkills } from "../../hooks";
import { DataTable, ConfirmDialog, type Column } from "../../components";
import type { Skill } from "../../types";
import "../SettingsPage.css";
import "./AgentSkillsPage.css";

// =============================================================================
// Component
// =============================================================================

export const AgentSkillsPage: FC = () => {
  const { agentId } = useParams<string>();
  const navigate = useNavigate();

  // Skills hook
  const {
    skills,
    agent,
    loading,
    error,
    fetchByAgent,
    remove,
  } = useSkills(agentId);

  // Search state
  const [search, setSearch] = useState("");

  // Delete confirmation state
  const [deleteSkill, setDeleteSkill] = useState<Skill | null>(null);
  const [deleting, setDeleting] = useState(false);

  // ---------------------------------------------------------------------------
  // Filtered skills
  // ---------------------------------------------------------------------------
  const filteredSkills = useMemo(() => {
    if (!search) return skills;

    const searchLower = search.toLowerCase();
    return skills.filter(
      (skill) =>
        skill.name.toLowerCase().includes(searchLower) ||
        skill.code?.toLowerCase().includes(searchLower)
    );
  }, [skills, search]);

  // ---------------------------------------------------------------------------
  // Table columns
  // ---------------------------------------------------------------------------
  const columns: Column<Skill>[] = [
    {
      key: "code",
      header: "Code",
      width: "180px",
      render: (skill) => (
        <span className="code-cell">{skill.code || "-"}</span>
      ),
    },
    {
      key: "name",
      header: "Name",
      width: "250px",
      render: (skill) => (
        <div className="skill-name-cell">
          <BrainIcon size={16} />
          <span>{skill.name}</span>
        </div>
      ),
    },
    {
      key: "updatedAt",
      header: "Updated",
      width: "120px",
      render: (skill) => (
        <span className="date-cell">
          {new Date(skill.updatedAt).toLocaleDateString()}
        </span>
      ),
    },
  ];

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------
  const handleTestSkill = (skill: Skill, e: React.MouseEvent) => {
    e.stopPropagation();
    navigate(`/test-chat?skill=${skill.id}&agent=${agentId}`, {
      state: {
        skillName: skill.name,
        agentName: agent?.name,
      },
    });
  };

  const handleBack = () => {
    navigate("/settings/agents");
  };

  const handleCreate = () => {
    navigate(`/settings/agents/${agentId}/skills/new`);
  };

  const handleEdit = (skill: Skill) => {
    navigate(`/settings/agents/${agentId}/skills/${skill.id}/edit`);
  };

  const handleDeleteClick = (skill: Skill) => {
    setDeleteSkill(skill);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteSkill) return;

    setDeleting(true);
    try {
      await remove(deleteSkill.id);
      setDeleteSkill(null);
    } catch (e) {
      console.error("Failed to delete:", e);
    } finally {
      setDeleting(false);
    }
  };

  const handleRetry = () => {
    if (agentId) {
      fetchByAgent(agentId);
    }
  };

  // ---------------------------------------------------------------------------
  // Render actions
  // ---------------------------------------------------------------------------
  const renderActions = (skill: Skill) => (
    <div className="table-actions">
      <button
        className="action-btn chat-btn"
        onClick={(e) => handleTestSkill(skill, e)}
        title="Test skill in chat"
      >
        <MessageSquareIcon size={16} />
      </button>
      <button className="action-btn" onClick={() => handleEdit(skill)} title="Edit">
        <PencilIcon size={16} />
      </button>
      <button
        className="action-btn danger"
        onClick={() => handleDeleteClick(skill)}
        title="Delete"
      >
        <TrashIcon size={16} />
      </button>
    </div>
  );

  // ---------------------------------------------------------------------------
  // Agent not found
  // ---------------------------------------------------------------------------
  const agentNotFound = !loading && !agent && agentId;

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------
  return (
    <div className="settings-page">
      {/* Header */}
      <header className="settings-page-header">
        <div className="settings-page-title with-breadcrumb">
          <button className="back-btn" onClick={handleBack}>
            <ArrowLeftIcon size={20} />
          </button>
          <div className="breadcrumb">
            <LayersIcon size={20} />
            <span className="breadcrumb-parent" onClick={handleBack}>
              Agents
            </span>
            <span className="breadcrumb-separator">/</span>
            <span className="breadcrumb-current">
              {loading ? "Loading..." : agent?.name || "Unknown"}
            </span>
          </div>
        </div>
        <button
          className="btn btn-primary"
          onClick={handleCreate}
          disabled={loading || agentNotFound}
        >
          <PlusIcon size={18} />
          <span>Add Skill</span>
        </button>
      </header>

      {/* Content */}
      <div className="settings-page-content">
        {agentNotFound && (
          <div className="not-found-state">
            <LayersIcon size={48} />
            <h3>Agent not found</h3>
            <p>The agent you're looking for doesn't exist.</p>
            <button className="btn btn-secondary" onClick={handleBack}>
              <ArrowLeftIcon size={18} />
              <span>Back to Agents</span>
            </button>
          </div>
        )}

        {!agentNotFound && (
          <DataTable
            columns={columns}
            data={filteredSkills}
            keyField="id"
            loading={loading}
            error={error}
            emptyIcon={<BrainIcon size={48} />}
            emptyTitle="No skills yet"
            emptyDescription={`Create your first skill in "${agent?.name || "this agent"}".`}
            searchPlaceholder="Search by code or name..."
            searchValue={search}
            onSearchChange={setSearch}
            onRetry={handleRetry}
            actions={renderActions}
          />
        )}
      </div>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteSkill}
        title="Delete Skill"
        message={`Are you sure you want to delete "${deleteSkill?.name}"? This will also delete all associated routing queries and tools.`}
        confirmLabel="Delete"
        danger
        loading={deleting}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteSkill(null)}
      />
    </div>
  );
};