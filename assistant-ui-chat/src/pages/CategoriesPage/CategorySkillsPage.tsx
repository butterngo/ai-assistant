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
import { DataTable, SkillModal, ConfirmDialog, type Column } from "../../components";
import type { Skill, CreateSkillRequest, UpdateSkillRequest } from "../../types";
import "../SettingsPage.css";
import "./CategorySkillsPage.css";

// =============================================================================
// Component
// =============================================================================

export const CategorySkillsPage: FC = () => {
  const { categoryId } = useParams<string>();
  const navigate = useNavigate();

  const {
    skills,
    category,
    loading: skillsLoading,
    error: skillsError,
    fetchByCategory,
    create,
    update,
    remove,
  } = useSkills(categoryId);

  // Search state
  const [search, setSearch] = useState("");

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSkill, setEditingSkill] = useState<Skill | null>(null);

  // Delete confirmation state
  const [deleteSkill, setDeleteSkill] = useState<Skill | null>(null);
  const [deleting, setDeleting] = useState(false);

  // ---------------------------------------------------------------------------
  // Combined loading and error states
  // ---------------------------------------------------------------------------
  const loading = skillsLoading;
  const error =  skillsError;

  // ---------------------------------------------------------------------------
  // Filtered skills
  // ---------------------------------------------------------------------------
  const filteredSkills = useMemo(() => {
    if (!search) return skills;

    const searchLower = search.toLowerCase();
    return skills.filter(
      (skill) =>
        skill.name.toLowerCase().includes(searchLower) ||
        skill.description?.toLowerCase().includes(searchLower)
    );
  }, [skills, search]);

  // ---------------------------------------------------------------------------
  // Table columns
  // ---------------------------------------------------------------------------
  const columns: Column<Skill>[] = [
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
      key: "description",
      header: "Description",
      render: (skill) => (
        <span className="description-cell">{skill.description || "-"}</span>
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
  navigate(`/test-chat?skill=${skill.id}&category=${categoryId}`, {
    state: { 
      skillName: skill.name,
      categoryName: category?.name 
    }
  });
 };

  const handleBack = () => {
    navigate("/settings/categories");
  };

  const handleCreate = () => {
    setEditingSkill(null);
    setIsModalOpen(true);
  };

  const handleEdit = (skill: Skill) => {
    setEditingSkill(skill);
    setIsModalOpen(true);
  };

  const handleSave = async (data: CreateSkillRequest | UpdateSkillRequest) => {
    if (!categoryId) return;

    if (editingSkill) {
      await update(editingSkill.id, data);
    } else {
      await create({
        ...data,
        categoryId,
      } as CreateSkillRequest);
    }
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
    if (categoryId) {
      fetchByCategory(categoryId);
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
  // Category not found
  // ---------------------------------------------------------------------------
  const categoryNotFound = !loading && !category && categoryId;

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
              {loading ? "Loading..." : category?.name || "Unknown"}
            </span>
          </div>
        </div>
        <button
          className="btn btn-primary"
          onClick={handleCreate}
          disabled={loading || categoryNotFound}
        >
          <PlusIcon size={18} />
          <span>Add Skill</span>
        </button>
      </header>

      {/* Content */}
      <div className="settings-page-content">
        {/* Category Not Found */}
        {categoryNotFound && (
          <div className="not-found-state">
            <LayersIcon size={48} />
            <h3>Category not found</h3>
            <p>The category you're looking for doesn't exist.</p>
            <button className="btn btn-secondary" onClick={handleBack}>
              <ArrowLeftIcon size={18} />
              <span>Back to Categories</span>
            </button>
          </div>
        )}

        {/* Data Table */}
        {!categoryNotFound && (
          <DataTable
            columns={columns}
            data={filteredSkills}
            keyField="id"
            loading={loading}
            error={error}
            emptyIcon={<BrainIcon size={48} />}
            emptyTitle="No skills yet"
            emptyDescription={`Create your first skill in "${category?.name || "this category"}".`}
            searchPlaceholder="Search skills..."
            searchValue={search}
            onSearchChange={setSearch}
            onRetry={handleRetry}
            actions={renderActions}
          />
        )}
      </div>

      {/* Create/Edit Modal */}
      {category && (
        <SkillModal
          isOpen={isModalOpen}
          skill={editingSkill}
          categories={[category]}
          onClose={() => setIsModalOpen(false)}
          onSave={handleSave}
        />
      )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteSkill}
        title="Delete Skill"
        message={`Are you sure you want to delete "${deleteSkill?.name}"? This will also delete all associated tools.`}
        confirmLabel="Delete"
        danger
        loading={deleting}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteSkill(null)}
      />
    </div>
  );
};