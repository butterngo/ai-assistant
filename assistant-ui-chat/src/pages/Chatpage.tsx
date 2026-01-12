import { useState, useCallback, useMemo } from "react";
import { useParams, useLoaderData, useNavigate } from "react-router-dom";
import { AssistantRuntimeProvider, useLocalRuntime } from "@assistant-ui/react";
import { useChatStreamAdapter, type ChatMetadata, type ChatDone } from "../hooks/useChatStreamAdapter";
import { useLayoutContext } from "../layout/Applayout";
import { Thread } from "../components/Thread";
import { API_ENDPOINT } from "../config";
import type { ConversationLoaderData } from "../loaders/Conversationloader";

export function ChatPage() {
  const navigate = useNavigate();
  const { conversationId } = useParams();
  const loaderData = useLoaderData() as ConversationLoaderData | undefined;
  
  const {
    sidebarOpen,
    onToggleSidebar,
    addConversation,
    updateConversation,
  } = useLayoutContext();

  const [isStreaming, setIsStreaming] = useState(false);

  // Get initial messages from loader (for existing conversations)
  const initialMessages = useMemo(() => {
    if (!loaderData?.conversation?.messages) return [];
    return loaderData.conversation.messages;
  }, [loaderData]);

  // ==========================================================================
  // SSE Event Handlers
  // ==========================================================================

  const handleMetadata = useCallback(
    (metadata: ChatMetadata) => {
      console.log("📩 Metadata received:", metadata);

      if (metadata.isNewConversation) {
        // Add to sidebar
        addConversation({
          id: metadata.conversationId,
          threadId: metadata.conversationId,
          title: metadata.title,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        });

        // Navigate to new conversation URL (replaces current history entry)
        navigate(`/conversation/${metadata.conversationId}`, { replace: true });
      }
    },
    [addConversation, navigate]
  );

  const handleStart = useCallback(() => {
    setIsStreaming(true);
  }, []);

  const handleDone = useCallback(
    (done: ChatDone) => {
      console.log("✅ Stream completed:", done);
      setIsStreaming(false);

      // Update conversation title
      if (done.title) {
        updateConversation(done.conversationId, { title: done.title });
      }
    },
    [updateConversation]
  );

  const handleError = useCallback((error: { error: string; code: string }) => {
    console.error("❌ Chat stream error:", error);
    setIsStreaming(false);
  }, []);

  // ==========================================================================
  // Adapter & Runtime Setup
  // ==========================================================================

  const adapter = useChatStreamAdapter({
    api: API_ENDPOINT,
    conversationId: conversationId ?? null, // null = new conversation
    onMetadata: handleMetadata,
    onStart: handleStart,
    onDone: handleDone,
    onError: handleError,
  });

  // Convert messages to assistant-ui format
  const formattedInitialMessages = useMemo(
    () =>
      initialMessages.map((msg) => ({
        role: msg.role as "user" | "assistant",
        content: [{ type: "text" as const, text: msg.content }],
      })),
    [initialMessages]
  );

  const runtime = useLocalRuntime(adapter, {
    initialMessages: formattedInitialMessages,
  });

  // Use conversationId as key to force re-mount when switching conversations
  const runtimeKey = conversationId ?? "new";

  // ==========================================================================
  // Error State
  // ==========================================================================

  if (loaderData?.error) {
    return (
      <div className="chat-error">
        <h2>Error loading conversation</h2>
        <p>{loaderData.error}</p>
        <button onClick={() => navigate("/")}>Start new conversation</button>
      </div>
    );
  }

  // ==========================================================================
  // Render
  // ==========================================================================

  return (
    <AssistantRuntimeProvider runtime={runtime} key={runtimeKey}>
      <Thread
        onToggleSidebar={onToggleSidebar}
        sidebarOpen={sidebarOpen}
        isStreaming={isStreaming}
      />
    </AssistantRuntimeProvider>
  );
}