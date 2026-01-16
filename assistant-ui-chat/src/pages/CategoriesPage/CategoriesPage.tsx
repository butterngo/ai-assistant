import { type FC, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  LayersIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  ChevronRightIcon,
  BrainIcon,
} from "lucide-react";
import { useCategories } from "../../hooks";
import { CategoryModal, ConfirmDialog } from "../../components";
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from "../../types";
import "../SettingsPage.css";
import "./CategoriesPage.css";

// =============================================================================
// Component
// =============================================================================

export const CategoriesPage: FC = () => {
  const navigate = useNavigate();
  const { categories, loading, error, create, update, remove, fetchAll } = useCategories();

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);

  // Delete confirmation state
  const [deleteCategory, setDeleteCategory] = useState<Category | null>(null);
  const [deleting, setDeleting] = useState(false);

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------

  const handleCreate = () => {
    setEditingCategory(null);
    setIsModalOpen(true);
  };

  const handleEdit = (e: React.MouseEvent, category: Category) => {
    e.stopPropagation();
    setEditingCategory(category);
    setIsModalOpen(true);
  };

  const handleSave = async (data: CreateCategoryRequest | UpdateCategoryRequest) => {
    if (editingCategory) {
      await update(editingCategory.id, data);
    } else {
      await create(data as CreateCategoryRequest);
    }
  };

  const handleDeleteClick = (e: React.MouseEvent, category: Category) => {
    e.stopPropagation();
    setDeleteCategory(category);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteCategory) return;

    setDeleting(true);
    try {
      await remove(deleteCategory.id);
      setDeleteCategory(null);
    } catch (e) {
      console.error("Failed to delete:", e);
    } finally {
      setDeleting(false);
    }
  };

  const handleViewSkills = (category: Category) => {
    navigate(`/settings/categories/${category.id}/skills`);
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
            <p>Loading categories...</p>
          </div>
        )}

        {/* Empty State */}
        {!loading && !error && categories.length === 0 && (
          <div className="empty-state">
            <LayersIcon size={48} />
            <h3>No categories yet</h3>
            <p>Create your first category to organize your skills.</p>
            <button className="btn btn-primary" onClick={handleCreate}>
              <span>Add Category</span>
            </button>
          </div>
        )}

        {/* Categories List */}
        {!loading && categories.length > 0 && (
          <div className="categories-list">
            {categories.map((category) => (
              <div
                key={category.id}
                className="category-card clickable"
                onClick={() => handleViewSkills(category)}
              >
                <div className="category-card-main">
                  <div className="category-icon">
                    <LayersIcon size={20} />
                  </div>
                  <div className="category-info">
                    <h3>{category.name}</h3>
                    {category.description && <p>{category.description}</p>}
                  </div>
                  <div className="category-meta">
                    <div className="skill-count">
                      <BrainIcon size={14} />
                      <span>{(category as CategoryWithSkillCount).skillCount ?? 0} skills</span>
                    </div>
                    <ChevronRightIcon size={20} className="chevron-icon" />
                  </div>
                </div>
                <div className="category-card-actions">
                  <button
                    className="action-btn"
                    onClick={(e) => handleEdit(e, category)}
                    title="Edit"
                  >
                    <PencilIcon size={16} />
                  </button>
                  <button
                    className="action-btn danger"
                    onClick={(e) => handleDeleteClick(e, category)}
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
      <CategoryModal
        isOpen={isModalOpen}
        category={editingCategory}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSave}
      />

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteCategory}
        title="Delete Category"
        message={`Are you sure you want to delete "${deleteCategory?.name}"? This will also delete all skills inside this category.`}
        confirmLabel="Delete"
        danger
        loading={deleting}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteCategory(null)}
      />
    </div>
  );
};

// Type extension for skill count
interface CategoryWithSkillCount extends Category {
  skillCount?: number;
}