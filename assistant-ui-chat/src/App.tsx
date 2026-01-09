import { useState, useCallback } from "react";
import { AssistantRuntimeProvider, useLocalRuntime } from "@assistant-ui/react";
import { useChatStreamAdapter, type ChatMetadata, type ChatDone } from "./hooks/useChatStreamAdapter";
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
    addConversation, // You'll need to add this to useConversations
  } = useConversations(API_BASE);

  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [isStreaming, setIsStreaming] = useState(false);

  // Handlers
  const handleNewConversation = useCallback(() => {
    // Clear active conversation to start fresh
    setActiveConversationId(null);
  }, [setActiveConversationId]);

  const handleSelectConversation = useCallback(
    (id: string) => {
      setActiveConversationId(id);
    },
    [setActiveConversationId]
  );

  const handleDeleteConversation = useCallback(
    async (id: string) => {
      try {
        await deleteConversation(id);
        // If we deleted the active conversation, clear selection
        if (id === activeConversationId) {
          setActiveConversationId(null);
        }
      } catch (e) {
        console.error("Failed to delete conversation", e);
      }
    },
    [deleteConversation, activeConversationId, setActiveConversationId]
  );

  const handleToggleSidebar = useCallback(() => {
    setSidebarOpen((prev) => !prev);
  }, []);

  const handleMetadata = useCallback(
    (metadata: ChatMetadata) => {
      console.log("📩 Metadata received:", metadata);

      if (metadata.isNewConversation) {
        // Add new conversation to the list
        addConversation({
          id: metadata.conversationId,
          threadId: metadata.conversationId,
          title: metadata.title,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        });
      }

      // Set as active conversation
      setActiveConversationId(metadata.conversationId);
    },
    [addConversation, setActiveConversationId]
  );

  // Handle streaming start
  const handleStart = useCallback(() => {
    setIsStreaming(true);
  }, []);

  // Handle done event (final event from server)
  const handleDone = useCallback(
    (done: ChatDone) => {
      console.log("✅ Stream completed:", done);
      setIsStreaming(false);

      // Update conversation title if provided
      if (done.title) {
        updateConversation(done.conversationId, { title: done.title });
      }
    },
    [updateConversation]
  );

  // Handle errors
  const handleError = useCallback((error: { error: string; code: string }) => {
    console.error("❌ Chat stream error:", error);
    setIsStreaming(false);
  }, []);


  const adapter = useChatStreamAdapter({
    api: API_ENDPOINT,
    conversationId: activeConversationId, // null = new conversation
    onMetadata: handleMetadata,
    onStart: handleStart,
    onDone: handleDone,
    onError: handleError,
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
            <Thread
              onToggleSidebar={handleToggleSidebar}
              sidebarOpen={sidebarOpen}
              isStreaming={isStreaming}
            />
          )}
        </main>
      </div>
    </AssistantRuntimeProvider>
  );
}

export default App;