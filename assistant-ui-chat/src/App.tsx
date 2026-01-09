import { useState, useCallback } from "react";
import { AssistantRuntimeProvider, useLocalRuntime } from "@assistant-ui/react";
import { useChatStreamAdapter } from "./hooks/useChatStreamAdapter";
import { useConversations } from "./hooks/useConversation";
import { Thread } from "./components/Thread";
import { Sidebar } from "./components/Sidebar";
import { API_BASE, API_ENDPOINT } from "./config";
import "./App.css";

function App() {
  const {
    conversations,
    activeConversationId,
    setActiveConversationId,
    loading,
    error,
    fetchAll,
    createConversation,
    updateConversation,
    deleteConversation,
  } = useConversations(API_BASE);

  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Handlers
  const handleNewConversation = useCallback(async () => {
    try {
      await createConversation("New conversation");
    } catch (e) {
      console.error("Failed to create conversation", e);
    }
  }, [createConversation]);

  const handleSelectConversation = useCallback((id: string) => {
    setActiveConversationId(id);
  }, [setActiveConversationId]);

  const handleDeleteConversation = useCallback(async (id: string) => {
    try {
      await deleteConversation(id);
    } catch (e) {
      console.error("Failed to delete conversation", e);
    }
  }, [deleteConversation]);

  const handleToggleSidebar = useCallback(() => {
    setSidebarOpen((prev) => !prev);
  }, []);

  // Stable temporary conversation id for component lifetime
  const [tempId] = useState(() => `temp-${Date.now()}`);
  const currentConversationId = activeConversationId ?? tempId;

  // Use the centralized SSE adapter hook
  const adapter = useChatStreamAdapter({
    api: API_ENDPOINT,
    conversationId: currentConversationId,
    onError: (error) => {
      // Log full error for debugging purposes
      console.error("Chat stream error:", error);
    },
  });

  const runtime = useLocalRuntime(adapter);

  return (
    <AssistantRuntimeProvider runtime={runtime}>
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
          onToggle={handleToggleSidebar}
          onNewConversation={handleNewConversation}
          onSelectConversation={handleSelectConversation}
          onDeleteConversation={handleDeleteConversation}
        />
        <main className={`main-content ${sidebarOpen ? "" : "sidebar-closed"}`}>
          {loading ? (
            <div className="loading">Loading conversations…</div>
          ) : (
            <Thread onToggleSidebar={handleToggleSidebar} sidebarOpen={sidebarOpen} />
          )}
        </main>
      </div>
    </AssistantRuntimeProvider>
  );
}

export default App;
