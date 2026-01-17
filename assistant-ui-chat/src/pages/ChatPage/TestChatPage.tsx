import { useState, useCallback, useMemo } from "react";
import { useSearchParams, useNavigate, useLocation } from "react-router-dom";
import { AssistantRuntimeProvider, useLocalRuntime } from "@assistant-ui/react";
import { useChatStreamAdapter } from "../../hooks/useChatStreamAdapter";
import { Thread } from "../../components";
import { API_ENDPOINT } from "../../config";
import type { ChatMetadata, ChatDone } from "../../types";
import "./TestChatPage.css";
import { XIcon } from "lucide-react";

export function TestChatPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const location = useLocation();

  // Get test context from URL/state
  const categoryId = searchParams.get("category");
  const skillId = searchParams.get("skill");
  const categoryName = location.state?.categoryName || "Agent";
  const skillName = location.state?.skillName;

  const [isStreaming, setIsStreaming] = useState(false);

  // Generate a unique test thread ID
  const threadId = useMemo(() => {
    // Use a special prefix to identify test threads
    const prefix = "test";
    const context = skillId || categoryId || "general";
    const timestamp = Date.now();
    return `${prefix}-${context}-${timestamp}`;
  }, [skillId, categoryId]);

  // ===========================================================================
  // Handlers
  // ===========================================================================

  const handleMetadata = useCallback((metadata: ChatMetadata) => {
    console.log("📊 Test chat metadata:", metadata);
    // You can add test-specific metadata handling here
  }, []);

  const handleStart = useCallback(() => {
    console.log("🚀 Test chat stream started");
    setIsStreaming(true);
  }, []);

  const handleDone = useCallback((done: ChatDone) => {
    console.log("✅ Test chat completed:", done);
    setIsStreaming(false);
  }, []);

const handleError = useCallback((error: { error: string; code: string }) => {
    console.error("❌ Chat stream error:", error);
    setIsStreaming(false);
  }, []);

  const handleClose = () => {
    if (skillId) {
      // Return to skills page if testing a skill
      navigate(`/settings/categories/${categoryId}/skills`);
    } else {
      // Return to categories page if testing a category
      navigate("/settings/categories");
    }
  };

  // ===========================================================================
  // Chat Adapter
  // ===========================================================================

  const adapter = useChatStreamAdapter({
    api: API_ENDPOINT,
    threadId,
    onMetadata: handleMetadata,
    onStart: handleStart,
    onDone: handleDone,
    onError: handleError,
  });

  // ===========================================================================
  // Runtime
  // ===========================================================================

  const runtime = useLocalRuntime(adapter);

  // ===========================================================================
  // Test Badge Context
  // ===========================================================================

  const testContext = useMemo(() => {
    if (skillName) {
      return {
        type: "skill" as const,
        name: skillName,
        parentName: categoryName,
      };
    }
    if (categoryName) {
      return {
        type: "category" as const,
        name: categoryName,
      };
    }
    return null;
  }, [skillName, categoryName]);

  // ===========================================================================
  // Render
  // ===========================================================================

  return (
    <div className="test-chat-page">
      <AssistantRuntimeProvider runtime={runtime}>
        {/* Test Header Banner */}
        <div className="test-header">
          <div className="test-badge">
            <span className="test-icon">🧪</span>
            <div className="test-info">
              <span className="test-label">Testing:</span>
              <span className="test-name">
                {testContext?.type === "skill"
                  ? `${testContext.parentName} > ${testContext.name}`
                  : testContext?.name || "Agent"}
              </span>
            </div>
          </div>
          <button className="close-test-btn" onClick={handleClose}>
            <XIcon size={16} />
            Close Test
          </button>
        </div>

        {/* Thread */}
        <div className="test-chat-content">
          <Thread
            onToggleSidebar={() => {}}
            sidebarOpen={false}
            isStreaming={isStreaming}
          />
        </div>
      </AssistantRuntimeProvider>
    </div>
  );
}