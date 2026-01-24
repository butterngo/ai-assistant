import { type FC, useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeftIcon, RefreshCwIcon, PlayIcon, SettingsIcon } from "lucide-react";
import { useConnectionTools } from "../../hooks/useConnectionTools";
import { ConnectionStatusBadge } from "./components/ConnectionStatusBadge";
import { EditConnectionModal } from "./components/EditConnectionModal";
import { DeleteConfirmationModal } from "./components/DeleteConfirmationModal";
import type { ConnectionTool,
   DiscoveredTool,
   UpdateConnectionToolRequest,
  UpdateDiscoveredToolRequest } from "../../types";

import { ViewToolJsonModal } from "./components/ViewToolJsonModal";
import { EditToolDescriptionModal } from "./components/EditToolDescriptionModal";
import { discoveredToolsClient } from "../../api";
import "./ConnectionDetailsPage.css";

export const ConnectionDetailsPage: FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  
  // ✅ Use hook instead of manual fetch
  const {
    getById,
    getDiscoveredTools,
    update,
    remove,
    test,
    discoverTools,
  } = useConnectionTools();

  // State
  const [connection, setConnection] = useState<ConnectionTool | null>(null);
  const [discoveredTools, setDiscoveredTools] = useState<DiscoveredTool[]>([]);
  const [loading, setLoading] = useState(true);
  const [testLoading, setTestLoading] = useState(false);
  const [discoverLoading, setDiscoverLoading] = useState(false);
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);
  
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const [viewJsonModal, setViewJsonModal] = useState<{
  isOpen: boolean;
  tool: DiscoveredTool | null;
}>({ isOpen: false, tool: null });

const [editToolModal, setEditToolModal] = useState<{
  isOpen: boolean;
  tool: DiscoveredTool | null;
}>({ isOpen: false, tool: null });

  // ---------------------------------------------------------------------------
  // Fetch connection details + discovered tools on mount
  // ---------------------------------------------------------------------------
  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;
      
      setLoading(true);
      try {
        // Fetch connection and discovered tools in parallel
        const [connectionData, toolsData] = await Promise.all([
          getById(id),
          getDiscoveredTools(id),
        ]);
        
        setConnection(connectionData);
        setDiscoveredTools(toolsData);
      } catch (error) {
        console.error("Failed to fetch data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [id, getById, getDiscoveredTools]);

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------
  
  /**
   * Test connection
   */
  const handleTestConnection = async () => {
    if (!id) return;
    
    setTestLoading(true);
    setTestResult(null);
    
    try {
      // Test the connection
      const result = await test(id);
      
      setTestResult({
        success: result.isConnected,
        message: result.message,
      });

      // Refetch connection to get updated lastTestedAt and lastTestStatus
      const updated = await getById(id);
      setConnection(updated);
    } catch (error) {
      setTestResult({
        success: false,
        message: error instanceof Error ? error.message : "Test failed",
      });
    } finally {
      setTestLoading(false);
    }
  };

  /**
   * Discover tools (fresh discovery from MCP/OpenAPI)
   */
  const handleDiscoverTools = async () => {
    if (!id) return;
    
    setDiscoverLoading(true);
    
    try {
      // 1. Discover fresh tools from external service (slow)
      const freshTools = await discoverTools(id);
      
      const cachedTools = await getDiscoveredTools(id);

      setDiscoveredTools(cachedTools);
      
      // 3. Refetch connection to get updated metadata
      const updated = await getById(id);
      setConnection(updated);

      // 4. Show success message
      setTestResult({
        success: true,
        message: `Successfully discovered ${freshTools.length} tools!`,
      });
    } catch (error) {
      console.error("Failed to discover tools:", error);
      setTestResult({
        success: false,
        message: error instanceof Error ? error.message : "Failed to discover tools",
      });
    } finally {
      setDiscoverLoading(false);
    }
  };

  /**
   * Update connection
   */
  const handleUpdate = async (id: string, data: UpdateConnectionToolRequest) => {
    try {
      const updated = await update(id, data);
      setConnection(updated);
      console.log("✅ Connection updated successfully");
    } catch (error) {
      console.error("❌ Failed to update connection:", error);
      throw error;
    }
  };

  /**
   * Delete connection
   */
  const handleDelete = async () => {
    if (!id) return;
    
    setDeleteLoading(true);
    try {
      await remove(id);
      console.log("✅ Connection deleted successfully");
      navigate("/settings/tools");
    } catch (error) {
      console.error("❌ Failed to delete connection:", error);
    } finally {
      setDeleteLoading(false);
    }
  };

  /**
   * View tool JSON schema
   * TODO: Phase 3 - Open ViewToolJsonModal
   */

  const handleViewJson = (tool: DiscoveredTool) => {
    setViewJsonModal({ isOpen: true, tool });
  };

  const handleEditDescription = (tool: DiscoveredTool) => {
    setEditToolModal({ isOpen: true, tool });
  };

  const handleUpdateTool = async (
  toolId: string,
  request: UpdateDiscoveredToolRequest
) => {
  try {
    await discoveredToolsClient.update(toolId, request);
    
    // Refetch discovered tools
    if (id) {
      const tools = await getDiscoveredTools(id);
      setDiscoveredTools(tools);
    }
    
    console.log("✅ Tool updated successfully");
  } catch (error) {
    console.error("❌ Failed to update tool:", error);
    throw error;
  }
};
  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------
  
  if (loading) {
    return (
      <div className="settings-page">
        <div className="loading-container">
          <div className="spinner" />
          <p>Loading connection...</p>
        </div>
      </div>
    );
  }

  if (!connection) {
    return (
      <div className="settings-page">
        <div className="error-container">
          <p>Connection not found</p>
          <button className="primary-btn" onClick={() => navigate("/settings/tools")}>
            Back to Tools
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="settings-page">
      <header className="settings-page-header">
        <button className="back-btn" onClick={() => navigate("/settings/tools")}>
          <ArrowLeftIcon size={20} />
        </button>
        <div className="settings-page-title">
          <h1>{connection.name}</h1>
        </div>
        <div className="header-actions">
          <button className="secondary-btn" onClick={() => setEditModalOpen(true)}>
            <SettingsIcon size={16} />
            Edit
          </button>
          <button className="danger-btn" onClick={() => setDeleteModalOpen(true)}>
            Delete
          </button>
        </div>
      </header>

      <main className="settings-page-content connection-details-page">
        {/* Basic Info Section */}
        <section className="details-section">
          <h2 className="section-title">Basic Info</h2>
          <div className="info-grid">
            <div className="info-item">
              <span className="info-label">Name</span>
              <span className="info-value">{connection.name}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Type</span>
              <span className="info-value">{connection.type}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Status</span>
              <span className="info-value">
                <ConnectionStatusBadge
                  isActive={connection.isActive}
                  lastTestStatus={connection.lastTestStatus}
                />
              </span>
            </div>
            <div className="info-item">
              <span className="info-label">Description</span>
              <span className="info-value">
                {connection.description || "No description"}
              </span>
            </div>
          </div>
        </section>

        {/* Connection Configuration */}
        <section className="details-section">
          <h2 className="section-title">Connection</h2>
          <div className="info-grid">
            {connection.command && (
              <div className="info-item">
                <span className="info-label">Command</span>
                <span className="info-value code">{connection.command}</span>
              </div>
            )}
            {connection.endpoint && (
              <div className="info-item">
                <span className="info-label">Endpoint</span>
                <span className="info-value code">{connection.endpoint}</span>
              </div>
            )}
            {connection.config?.arguments && (
              <div className="info-item full-width">
                <span className="info-label">Arguments</span>
                <span className="info-value code">
                  {Array.isArray(connection.config.arguments)
                    ? connection.config.arguments.join(" ")
                    : "N/A"}
                </span>
              </div>
            )}
            {connection.config?.environmentVariables && (
              <div className="info-item full-width">
                <span className="info-label">Environment Variables</span>
                <div className="env-vars-display">
                  {Object.keys(connection.config.environmentVariables).map((key) => (
                    <div key={key} className="env-var-badge">
                      {key}: •••••
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
          <button className="text-btn" onClick={() => console.log("Show config JSON")}>
            Show Full Config JSON
          </button>
        </section>

        {/* Connection Status */}
        <section className="details-section">
          <h2 className="section-title">Connection Status</h2>
          
          {testResult && (
            <div className={`test-result-banner ${testResult.success ? "success" : "error"}`}>
              <span className="result-icon">{testResult.success ? "✅" : "❌"}</span>
              <span className="result-message">{testResult.message}</span>
            </div>
          )}

          <div className="info-grid">
            <div className="info-item">
              <span className="info-label">Last Tested</span>
              <span className="info-value">
                {connection.lastTestedAt
                  ? new Date(connection.lastTestedAt).toLocaleString()
                  : "Never"}
              </span>
            </div>
            <div className="info-item">
              <span className="info-label">Last Discovery</span>
              <span className="info-value">
                {connection.updatedAt
                  ? new Date(connection.updatedAt).toLocaleString()
                  : "Never"}
              </span>
            </div>
          </div>

          <div className="action-buttons">
            <button
              className="primary-btn"
              onClick={handleTestConnection}
              disabled={testLoading}
            >
              {testLoading ? (
                <>
                  <div className="btn-spinner" />
                  Testing...
                </>
              ) : (
                <>
                  <PlayIcon size={16} />
                  Test Connection
                </>
              )}
            </button>
            <button
              className="primary-btn"
              onClick={handleDiscoverTools}
              disabled={discoverLoading}
            >
              {discoverLoading ? (
                <>
                  <div className="btn-spinner" />
                  Discovering...
                </>
              ) : (
                <>
                  <RefreshCwIcon size={16} />
                  Discover Tools
                </>
              )}
            </button>
          </div>
        </section>

        {/* Discovered Tools */}
        <section className="details-section">
          <div className="section-header">
            <h2 className="section-title">
              Discovered Tools ({discoveredTools.length})
            </h2>
            <button className="text-btn" onClick={handleDiscoverTools}>
              <RefreshCwIcon size={14} />
              Refresh Cache
            </button>
          </div>

          {discoveredTools.length === 0 ? (
            <div className="empty-tools">
              <p>No tools discovered yet</p>
              <button className="primary-btn" onClick={handleDiscoverTools}>
                Discover Tools
              </button>
            </div>
          ) : (
            <div className="tools-list">
              {discoveredTools.map((tool) => (
                <div key={tool.id} className="tool-card">
                  <div className="tool-header">
                    <span className="tool-icon">🔧</span>
                    <div className="tool-info">
                      <h3 className="tool-name">{tool.name}</h3>
                      <p className="tool-description">{tool.description}</p>
                    </div>
                  </div>
                  <div className="tool-meta">
                    <span className={`tool-status ${tool.isAvailable ? "available" : "unavailable"}`}>
                      {tool.isAvailable ? "✅ Available" : "❌ Unavailable"}
                    </span>
                    <span className="tool-verified">
                      Verified: {new Date(tool.lastVerifiedAt).toLocaleDateString()}
                    </span>
                  </div>
                  <div className="tool-actions">
                    <button
                      className="tool-action-btn"
                      onClick={() => handleViewJson(tool)}
                    >
                      View JSON
                    </button>
                    <button
                      className="tool-action-btn"
                      onClick={() => handleEditDescription(tool)}
                    >
                      Edit
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      </main>

      {/* Modals */}
      <EditConnectionModal
        isOpen={editModalOpen}
        onClose={() => setEditModalOpen(false)}
        onSubmit={handleUpdate}
        connection={connection}
      />

      <DeleteConfirmationModal
        isOpen={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        onConfirm={handleDelete}
        connectionName={connection.name}
        toolsCount={discoveredTools.length}
        loading={deleteLoading}
      />
      <ViewToolJsonModal
  isOpen={viewJsonModal.isOpen}
  onClose={() => setViewJsonModal({ isOpen: false, tool: null })}
  tool={viewJsonModal.tool}
/>

<EditToolDescriptionModal
  isOpen={editToolModal.isOpen}
  onClose={() => setEditToolModal({ isOpen: false, tool: null })}
  onSubmit={handleUpdateTool}
  tool={editToolModal.tool}
/>
    </div>
  );
};