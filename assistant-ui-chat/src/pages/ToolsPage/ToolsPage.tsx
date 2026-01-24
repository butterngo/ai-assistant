import { useState, type FC } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeftIcon, WrenchIcon } from "lucide-react";
import { useConnectionTools } from "../../hooks";
import { CreateConnectionModal } from "./components/CreateConnectionModal";
import { EditConnectionModal } from "./components/EditConnectionModal";
import { ConnectionStatusBadge } from "./components/ConnectionStatusBadge";
import { DeleteConfirmationModal } from "./components/DeleteConfirmationModal";
import { EmptyStatePage } from "./components/EmptyStatePage";
import { DataTable, type Column } from "../../components";
import type { 
  ConnectionTool, 
  CreateConnectionToolRequest,
  UpdateConnectionToolRequest 
} from "../../types";
import "./ToolsPage.css";

export const ToolsPage: FC = () => {
  const navigate = useNavigate();
  const { connections, loading, error, create, update, remove, fetchAll } = useConnectionTools();
  
  const [searchQuery, setSearchQuery] = useState("");
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editModal, setEditModal] = useState<{ isOpen: boolean; connection: ConnectionTool | null }>({
    isOpen: false,
    connection: null,
  });
  const [deleteModal, setDeleteModal] = useState<{ isOpen: boolean; connection: ConnectionTool | null }>({
    isOpen: false,
    connection: null,
  });
  const [deleteLoading, setDeleteLoading] = useState(false);

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------
  const handleCreate = async (data: CreateConnectionToolRequest) => {
    try {
      await create(data);
      console.log("✅ Connection created successfully");
    } catch (error) {
      console.error("❌ Failed to create connection:", error);
      throw error;
    }
  };

  const handleEdit = (connection: ConnectionTool) => {
    setEditModal({ isOpen: true, connection });
  };

  const handleUpdate = async (id: string, data: UpdateConnectionToolRequest) => {
    try {
      await update(id, data);
      console.log("✅ Connection updated successfully");
    } catch (error) {
      console.error("❌ Failed to update connection:", error);
      throw error;
    }
  };

  const handleDelete = (connection: ConnectionTool) => {
    setDeleteModal({ isOpen: true, connection });
  };

  const handleConfirmDelete = async () => {
    if (!deleteModal.connection) return;

    setDeleteLoading(true);
    try {
      await remove(deleteModal.connection.id);
      console.log("✅ Connection deleted successfully");
      setDeleteModal({ isOpen: false, connection: null });
    } catch (error) {
      console.error("❌ Failed to delete connection:", error);
    } finally {
      setDeleteLoading(false);
    }
  };

  // ---------------------------------------------------------------------------
  // Table Configuration
  // ---------------------------------------------------------------------------
  const columns: Column<ConnectionTool>[] = [
    {
      key: "name",
      header: "Name",
      width: "200px",
      render: (item) => (
        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
          <ConnectionStatusBadge 
            isActive={item.isActive} 
            lastTestStatus={item.lastTestStatus}
          />
          <span>{item.name}</span>
        </div>
      ),
    },
    {
      key: "type",
      header: "Type",
      width: "150px",
    },
    {
      key: "status",
      header: "Status",
      width: "100px",
      render: (item) => (item.isActive ? "Active" : "Inactive"),
    },
    {
      key: "tools",
      header: "Tools",
      width: "80px",
      render: (item) => item.discoveredToolsCount || "-",
    },
    {
      key: "lastTested",
      header: "Last Tested",
      width: "150px",
      render: (item) => {
        if (!item.lastTestedAt) return "Never";
        
        const date = new Date(item.lastTestedAt);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
        
        if (diffHours < 1) return "Just now";
        if (diffHours < 24) return `${diffHours} hours ago`;
        const diffDays = Math.floor(diffHours / 24);
        return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
      },
    },
  ];

  // Filter connections based on search
  const filteredConnections = connections.filter((conn) =>
    conn.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------
  
  // Show empty state if no connections
  if (!loading && connections.length === 0) {
    return (
      <div className="settings-page">
        <header className="settings-page-header">
          <button className="back-btn" onClick={() => navigate(-1)}>
            <ArrowLeftIcon size={20} />
          </button>
          <div className="settings-page-title">
            <WrenchIcon size={24} />
            <h1>Tools</h1>
          </div>
        </header>
        <main className="settings-page-content">
          <EmptyStatePage onCreateConnection={() => setIsCreateModalOpen(true)} />
        </main>

        <CreateConnectionModal
          isOpen={isCreateModalOpen}
          onClose={() => setIsCreateModalOpen(false)}
          onSubmit={handleCreate}
        />
      </div>
    );
  }

  return (
    <div className="settings-page">
      <header className="settings-page-header">
        <button className="back-btn" onClick={() => navigate(-1)}>
          <ArrowLeftIcon size={20} />
        </button>
        <div className="settings-page-title">
          <WrenchIcon size={24} />
          <h1>Tools</h1>
        </div>
        <button className="primary-btn" onClick={() => setIsCreateModalOpen(true)}>
          + New Tool
        </button>
      </header>

      <main className="settings-page-content">
        <DataTable
          columns={columns}
          data={filteredConnections}
          keyField="id"
          loading={loading}
          error={error}
          emptyIcon={<WrenchIcon size={48} />}
          emptyTitle="No connections found"
          emptyDescription="Try adjusting your search or create a new connection"
          searchPlaceholder="Search connections..."
          searchValue={searchQuery}
          onSearchChange={setSearchQuery}
          onRetry={fetchAll}
          actions={(item) => (
            <div style={{ display: "flex", gap: "8px" }}>
              <button 
                className="action-btn"
                 onClick={() => navigate(`/settings/tools/${item.id}`)}
              >
                View
              </button>
              <button 
                className="action-btn"
                onClick={() => handleEdit(item)}
              >
                Edit
              </button>
              <button 
                className="action-btn"
                onClick={() => console.log("Test", item.id)}
              >
                Test
              </button>
              <button 
                className="action-btn danger"
                onClick={() => handleDelete(item)}
              >
                Delete
              </button>
            </div>
          )}
        />
      </main>

      {/* Modals */}
      <CreateConnectionModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleCreate}
      />

      <EditConnectionModal
        isOpen={editModal.isOpen}
        onClose={() => setEditModal({ isOpen: false, connection: null })}
        onSubmit={handleUpdate}
        connection={editModal.connection}
      />

      <DeleteConfirmationModal
        isOpen={deleteModal.isOpen}
        onClose={() => setDeleteModal({ isOpen: false, connection: null })}
        onConfirm={handleConfirmDelete}
        connectionName={deleteModal.connection?.name || ""}
        toolsCount={deleteModal.connection?.discoveredToolsCount}
        loading={deleteLoading}
      />
    </div>
  );
};