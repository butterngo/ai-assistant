import { useState, useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { AssistantRuntimeProvider, useLocalRuntime } from "@assistant-ui/react";
import { useChatStreamAdapter } from "../hooks/useChatStreamAdapter";
import { Thread } from "./Thread";
import { API_ENDPOINT } from "../config";
import type { Conversation, Message, ChatMetadata, ChatDone } from "../types";

// =============================================================================
// Types
// =============================================================================

export interface ChatRuntimeProps {
  threadId: string | null;
  initialMessages: Message[];
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
  addConversation: (conversation: Conversation) => void;
}

// =============================================================================
// Component
// =============================================================================

export function ChatRuntime({
  threadId,
  initialMessages,
  sidebarOpen,
  onToggleSidebar,
  addConversation,
}: ChatRuntimeProps) {
  const navigate = useNavigate();
  const [isStreaming, setIsStreaming] = useState(false);

  // ===========================================================================
  // SSE Event Handlers
  // ===========================================================================

  const handleMetadata = useCallback(
    (metadata: ChatMetadata) => {
      console.log("Metadata received:", metadata);

      if (metadata.isNewConversation) {
        const newConversation: Conversation = {
          id: metadata.threadId,
          threadId: metadata.threadId,
          title: metadata.title ?? null,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };

        addConversation(newConversation);
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

      if (done.isNewConversation) {
        navigate(`/conversation/${done.threadId}`, { replace: true });
      }
    },
    [navigate]
  );

  const handleError = useCallback((error: { error: string; code: string }) => {
    console.error("❌ Chat stream error:", error);
    setIsStreaming(false);
  }, []);

  // ===========================================================================
  // Adapter & Runtime Setup
  // ===========================================================================
  
  const adapter = useChatStreamAdapter({
    api: API_ENDPOINT,
    threadId,
    onMetadata: handleMetadata,
    onStart: handleStart,
    onDone: handleDone,
    onError: handleError,
  });

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

  // ===========================================================================
  // Render
  // ===========================================================================

  return (
    <AssistantRuntimeProvider runtime={runtime}>
      <Thread
        onToggleSidebar={onToggleSidebar}
        sidebarOpen={sidebarOpen}
        isStreaming={isStreaming}
      />
    </AssistantRuntimeProvider>
  );
}