import { useParams, useLoaderData, useNavigate } from "react-router-dom";
import { useLayoutContext } from "../layout/useLayoutContext";
import { ChatRuntime } from "../components/ChatRuntime";
import type { ConversationLoaderData } from "../loaders/Conversationloader";

export function ChatPage() {
  const navigate = useNavigate();
  const { threadId } = useParams();
  const loaderData = useLoaderData() as ConversationLoaderData | undefined;

  const {
    sidebarOpen,
    onToggleSidebar,
    addConversation,
  } = useLayoutContext();

  // ===========================================================================
  // Error State
  // ===========================================================================

  if (loaderData?.error) {
    return (
      <div className="chat-error">
        <h2>Error loading conversation</h2>
        <p>{loaderData.error}</p>
        <button onClick={() => navigate("/")}>Start new conversation</button>
      </div>
    );
  }

  // ===========================================================================
  // Loading State
  // ===========================================================================

  const isNewConversation = !threadId;
  const isLoadingExistingConversation = threadId && !loaderData?.conversation;

  if (isLoadingExistingConversation) {
    return (
      <div className="chat-loading">
        <p>Loading conversation...</p>
      </div>
    );
  }

  // ===========================================================================
  // Ready - Render ChatRuntime
  // ===========================================================================

  const messages = loaderData?.conversation?.messages ?? [];
  const runtimeKey = isNewConversation ? "new" : `${threadId}-${messages.length}`;

  return (
    <ChatRuntime
      key={runtimeKey}
      threadId={threadId ?? null}
      initialMessages={messages}
      sidebarOpen={sidebarOpen}
      onToggleSidebar={onToggleSidebar}
      addConversation={addConversation}
    />
  );
}