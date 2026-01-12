import { useState, useCallback, useEffect } from "react";
import { Outlet, useNavigate, useParams } from "react-router-dom";
import { Sidebar } from "../components/Sidebar";
import { useConversations } from "../hooks/useConversation";
import { API_BASE } from "../config";
import "./AppLayout.css";

export function AppLayout() {
  const navigate = useNavigate();
  const { conversationId } = useParams();
  
  const {
    conversations,
    loading,
    error,
    fetchAll,
    updateConversation,
    deleteConversation,
    addConversation,
  } = useConversations(API_BASE);

  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Sync active conversation with URL
  const activeConversationId = conversationId ?? null;

  // ==========================================================================
  // Handlers
  // ==========================================================================

  const handleNewConversation = useCallback(() => {
    // Navigate to root = new conversation
    navigate("/");
  }, [navigate]);

  const handleSelectConversation = useCallback(
    (id: string) => {
      // Navigate to conversation URL - loader will fetch messages
      navigate(`/conversation/${id}`);
    },
    [navigate]
  );

  const handleDeleteConversation = useCallback(
    async (id: string) => {
      try {
        await deleteConversation(id);
        // If we deleted the active conversation, go to new conversation
        if (id === activeConversationId) {
          navigate("/");
        }
      } catch (e) {
        console.error("Failed to delete conversation", e);
      }
    },
    [deleteConversation, activeConversationId, navigate]
  );

  const handleToggleSidebar = useCallback(() => {
    setSidebarOpen((prev) => !prev);
  }, []);

  // ==========================================================================
  // Context value for child components
  // ==========================================================================

  const layoutContext = {
    sidebarOpen,
    onToggleSidebar: handleToggleSidebar,
    addConversation,
    updateConversation,
    navigateToConversation: (id: string) => navigate(`/conversation/${id}`),
  };

  // ==========================================================================
  // Render
  // ==========================================================================

  return (
    <div className="app-layout">
      {/* Error banner */}
      {error && (
        <div className="error-banner">
          <span>Failed to load conversations: {error}</span>
          <button onClick={() => fetchAll()}>Retry</button>
        </div>
      )}

      <Sidebar
        conversations={conversations}
        activeId={activeConversationId}
        isOpen={sidebarOpen}
        loading={loading}
        onToggle={handleToggleSidebar}
        onNewConversation={handleNewConversation}
        onSelectConversation={handleSelectConversation}
        onDeleteConversation={handleDeleteConversation}
      />

      <main className={`main-content ${sidebarOpen ? "" : "sidebar-closed"}`}>
        {/* Outlet renders ChatPage with layout context */}
        <Outlet context={layoutContext} />
      </main>
    </div>
  );
}

// =============================================================================
// Layout Context Hook
// =============================================================================

import { useOutletContext } from "react-router-dom";
import type { Conversation } from "../types/Conversation";

export interface LayoutContextType {
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
  addConversation: (conversation: Conversation) => void;
  updateConversation: (id: string, updates: Partial<Conversation>) => void;
  navigateToConversation: (id: string) => void;
}

export function useLayoutContext() {
  return useOutletContext<LayoutContextType>();
}