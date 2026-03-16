import { type FC, useState, useEffect } from "react";
import { useConnectionTools } from "../../../../hooks/useConnectionTools";
import { 
  discoveredToolsClient, 
  skillConnectionToolsClient 
} from "../../../../api";
import { DataTable, type Column } from "../../../../components/DataTable";
import { ViewToolSchemaModal } from "./ViewToolSchemaModal";
import type { DiscoveredTool } from "../../../../types";
import "./SkillToolsTab.css";

interface SkillToolsTabProps {
  skillId: string;
}

export const SkillToolsTab: FC<SkillToolsTabProps> = ({ skillId }) => {
  const { connections, loading: connectionsLoading } = useConnectionTools();

  // State
  const [selectedConnectionId, setSelectedConnectionId] = useState<string>("");
  const [discoveredTools, setDiscoveredTools] = useState<DiscoveredTool[]>([]);
  const [linkedConnectionIds, setLinkedConnectionIds] = useState<Set<string>>(new Set());
  const [selectedToolIds, setSelectedToolIds] = useState<Set<string>>(new Set()); // 🆕 Selected tools
  const [loading, setLoading] = useState(false);
  const [viewSchemaModal, setViewSchemaModal] = useState<{
    isOpen: boolean;
    tool: DiscoveredTool | null;
  }>({ isOpen: false, tool: null });

  // Pagination and search state
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState("");
  const pageSize = 10;

  // ---------------------------------------------------------------------------
  // Load linked connections on mount
  // ---------------------------------------------------------------------------
  useEffect(() => {
    const loadLinkedConnections = async () => {
      try {
        const linked = await skillConnectionToolsClient.getConnectionsBySkill(skillId);
        const linkedIds = new Set(linked.map(c => c.id));
        console.log("Linked connection IDs:", linkedIds);
        setLinkedConnectionIds(linkedIds);
      } catch (error) {
        console.error("Failed to load linked connections:", error);
      }
    };

    loadLinkedConnections();
  }, [skillId]);

  // ---------------------------------------------------------------------------
  // Handle connection selection change
  // ---------------------------------------------------------------------------
  const handleConnectionChange = async (connectionId: string) => {
    if (!connectionId) {
      setSelectedConnectionId("");
      setDiscoveredTools([]);
      setSelectedToolIds(new Set());
      return;
    }

    setSelectedConnectionId(connectionId);
    setLoading(true);
    setCurrentPage(1);
    setSearchQuery("");
    
    try {
      const tools = await discoveredToolsClient.getByConnection(connectionId);
      setDiscoveredTools(tools);

      console.log(`Discovered tools for connection ${connectionId}:`, tools);
      
      // 🆕 Check if this connection is already linked
      if (linkedConnectionIds.has(connectionId)) {
        // Pre-select all tools from linked connection
        const allToolIds = new Set(tools.map(t => t.id));
        setSelectedToolIds(allToolIds);
      } else {
        setSelectedToolIds(new Set());
      }
    } catch (error) {
      console.error("Failed to load tools:", error);
      alert("Failed to load tools for this connection");
    } finally {
      setLoading(false);
    }
  };

  // ---------------------------------------------------------------------------
  // 🆕 Toggle individual tool selection
  // ---------------------------------------------------------------------------
  const handleToggleTool = (toolId: string) => {
    setSelectedToolIds(prev => {
      const next = new Set(prev);
      if (next.has(toolId)) {
        next.delete(toolId);
      } else {
        next.add(toolId);
      }
      return next;
    });
  };

  // ---------------------------------------------------------------------------
  // 🆕 Select all tools
  // ---------------------------------------------------------------------------
  const handleSelectAll = () => {
    const allToolIds = new Set(filteredTools.map(t => t.id));
    setSelectedToolIds(allToolIds);
  };

  // ---------------------------------------------------------------------------
  // 🆕 Deselect all tools
  // ---------------------------------------------------------------------------
  const handleDeselectAll = () => {
    setSelectedToolIds(new Set());
  };
  
  useEffect(() => {
    
    //TODO: bulk insert or delete skill-tool links based on selectedToolIds
    console.log("Selected tools updated:", selectedToolIds);
  }, [selectedToolIds]);

  // ---------------------------------------------------------------------------
  // Check if current connection is linked to skill
  // ---------------------------------------------------------------------------
  const isConnectionLinked = selectedConnectionId 
    ? linkedConnectionIds.has(selectedConnectionId) 
    : false;

  // ---------------------------------------------------------------------------
  // Open JSON schema modal
  // ---------------------------------------------------------------------------
  const openSchemaModal = (tool: DiscoveredTool) => {
    setViewSchemaModal({ isOpen: true, tool });
  };

  const closeSchemaModal = () => {
    setViewSchemaModal({ isOpen: false, tool: null });
  };

  // ---------------------------------------------------------------------------
  // Filter and paginate tools
  // ---------------------------------------------------------------------------
  const filteredTools = discoveredTools.filter(tool => {
    if (!searchQuery.trim()) return true;
    const query = searchQuery.toLowerCase();
    return (
      tool.name.toLowerCase().includes(query) ||
      tool.description.toLowerCase().includes(query)
    );
  });

  const totalCount = filteredTools.length;
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedTools = filteredTools.slice(startIndex, endIndex);

  const handleSearchChange = (value: string) => {
    setSearchQuery(value);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // ---------------------------------------------------------------------------
  // DataTable Columns Configuration
  // ---------------------------------------------------------------------------
  const columns: Column<DiscoveredTool>[] = [
    // 🆕 Checkbox column
    {
      key: "checkbox",
      header: (
        <input
          type="checkbox"
          checked={filteredTools.length > 0 && filteredTools.every(t => selectedToolIds.has(t.id))}
          onChange={(e) => {
            if (e.target.checked) {
              handleSelectAll();
            } else {
              handleDeselectAll();
            }
          }}
          aria-label="Select all tools"
        />
      ),
      width: "5%",
      render: (tool: DiscoveredTool) => (
        <input
          type="checkbox"
          checked={selectedToolIds.has(tool.id)}
          onChange={() => handleToggleTool(tool.id)}
          onClick={(e) => e.stopPropagation()} // Prevent row click
          aria-label={`Select ${tool.name}`}
        />
      ),
    },
    {
      key: "name",
      header: "Tool Name",
      width: "25%",
      render: (tool: DiscoveredTool) => (
        <span className="tool-name-cell">{tool.name}</span>
      ),
    },
    {
      key: "description",
      header: "Description",
      width: "40%",
      render: (tool: DiscoveredTool) => (
        <span className="tool-description-cell">{tool.description}</span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      width: "15%",
      render: (tool: DiscoveredTool) => (
        <button
          className="view-schema-btn-inline"
          onClick={() => openSchemaModal(tool)}
        >
          View Schema
        </button>
      ),
    },
  ];

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------

  if (connectionsLoading) {
    return (
      <div className="skill-tools-tab loading">
        <div className="spinner" />
        <p>Loading connections...</p>
      </div>
    );
  }

  return (
    <div className="skill-tools-tab">
      {/* Connection Selector */}
      <div className="connection-selector">
        <label htmlFor="connection-select">Select Connection Tool:</label>
        <select
          id="connection-select"
          value={selectedConnectionId}
          onChange={(e) => handleConnectionChange(e.target.value)}
          className="connection-dropdown"
        >
          <option value="">Select a connection tool...</option>
          {connections.map((conn) => (
            <option key={conn.id} value={conn.id}>
              🔧 {conn.name} ({conn.type})
            </option>
          ))}
        </select>
      </div>

      {/* Separator */}
      {selectedConnectionId && <div className="section-divider" />}

      {/* Tools Table */}
      {selectedConnectionId && (
        <>
          {/* Header */}
          <div className="tools-list-header">
            <div className="header-left">
              <h3>Available Tools ({discoveredTools.length})</h3>
              {isConnectionLinked && (
                <span className="linked-badge">✓ Linked to Skill</span>
              )}
            </div>
            {/* 🆕 Selection info */}
            {selectedToolIds.size > 0 && (
              <div className="selection-info">
                {selectedToolIds.size} selected
              </div>
            )}
          </div>

          {/* DataTable */}
          <DataTable
            data={paginatedTools}
            columns={columns}
            keyField="id"
            loading={loading}
            emptyTitle="No tools found"
            emptyDescription={
              searchQuery 
                ? `No tools matching "${searchQuery}"`
                : "No tools discovered yet for this connection"
            }
            searchPlaceholder="Search tools by name or description..."
            searchValue={searchQuery}
            onSearchChange={handleSearchChange}
            page={currentPage}
            pageSize={pageSize}
            totalCount={totalCount}
            onPageChange={handlePageChange}
          />
        </>
      )}

      {/* View JSON Schema Modal */}
      <ViewToolSchemaModal
        isOpen={viewSchemaModal.isOpen}
        onClose={closeSchemaModal}
        tool={viewSchemaModal.tool}
      />
    </div>
  );
};