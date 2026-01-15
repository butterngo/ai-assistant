import { useState, useCallback } from "react";
import { Outlet, useNavigate, useParams } from "react-router-dom";
import { Sidebar } from "../components/Sidebar";
import { useConversations } from "../hooks/useConversation";
import "./AppLayout.css";

export function AppLayout() {
  const navigate = useNavigate();
  const { threadId } = useParams();

  const {
    conversations,
    loading,
    error,
    fetchAll,
    deleteConversation,
    addConversation,
  } = useConversations();

  const [sidebarOpen, setSidebarOpen] = useState(true);

  const activethreadId = threadId ?? null;

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
        if (id === activethreadId) {
          navigate("/");
        }
      } catch (e) {
        console.error("Failed to delete conversation", e);
      }
    },
    [deleteConversation, activethreadId, navigate]
  );

  const handleToggleSidebar = useCallback(() => {
    setSidebarOpen((prev) => !prev);
  }, []);

  const layoutContext = {
    sidebarOpen,
    onToggleSidebar: handleToggleSidebar,
    addConversation,
    navigateToConversation: (id: string) => navigate(`/conversation/${id}`),
  };

  return (
    <div className="app-layout">
      {/* Error banner */}
      {error && (
        <div className="error-banner">
          Failed to load conversations: {error}
          <button onClick={() => fetchAll()}>Retry</button>
        </div>
      )}

      <Sidebar
        conversations={conversations}
        activeId={activethreadId}
        isOpen={sidebarOpen}
        loading={loading}
        onToggle={handleToggleSidebar}
        onNewConversation={handleNewConversation}
        onSelectConversation={handleSelectConversation}
        onDeleteConversation={handleDeleteConversation}
      />

      <main className="main-content">
        {/* Outlet renders ChatPage with layout context */}
        <Outlet context={layoutContext} />
      </main>
    </div>
  );
}