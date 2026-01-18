import { useState, useCallback, useMemo, useEffect } from "react";
import { useSearchParams, useNavigate, useLocation } from "react-router-dom";
import { AssistantRuntimeProvider, useLocalRuntime } from "@assistant-ui/react";
import { useChatStreamAdapter } from "../../hooks/useChatStreamAdapter";
import { useSkills } from "../../hooks";
import { Thread, SkillsSidebar, SkillInstructionsModal  } from "../../components";
import { API_ENDPOINT } from "../../config";
import type { ChatMetadata, ChatDone, Skill } from "../../types";
import { 
  XIcon, 
  ChevronLeftIcon, 
  ChevronRightIcon
} from "lucide-react";
import "./TestChatPage.css";

export function TestChatPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const location = useLocation();

  // Get test context from URL/state
  const categoryId = searchParams.get("category");
  const skillId = searchParams.get("skill");
  const categoryName = location.state?.categoryName || "Agent";
  const skillName = location.state?.skillName;
  
  const [threadId, setThreadId] = useState<string | null>(null);
  const [isStreaming, setIsStreaming] = useState(false);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [selectedSkill, setSelectedSkill] = useState<Skill | null>(null);

  // ===========================================================================
  // Use Skills Hook
  // ===========================================================================

  const {
    skills,
    category,
    loading: skillsLoading,
    error: skillsError,
    fetchByCategory,
  } = useSkills(categoryId);

  // Load skills when component mounts or categoryId changes
  useEffect(() => {
    if (categoryId) {
      fetchByCategory(categoryId);
    }
  }, [categoryId, fetchByCategory]);

  // Build headers with test context
  const headers = useMemo(() => {
    const hdrs: Record<string, string> = {
      "X-Test-Mode": "true",
    };

    if (categoryId) {
      hdrs["X-Agent-Id"] = categoryId;
    }

    if (skillId) {
      hdrs["X-Skill-Id"] = skillId;
    }

    return hdrs;
  }, [categoryId, skillId]);

  // ===========================================================================
  // Handlers
  // ===========================================================================

  const handleMetadata = useCallback((metadata: ChatMetadata) => {
    console.log("📊 Test chat metadata:", metadata);
    setThreadId(metadata.threadId);
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
      navigate(`/settings/categories/${categoryId}/skills`);
    } else {
      navigate("/settings/categories");
    }
  };

  const handleToggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  const handleViewSkill = (skill: Skill) => {
    setSelectedSkill(skill);
  };

  const handleCloseSkillModal = () => {
    setSelectedSkill(null);
  };

  // ===========================================================================
  // Chat Adapter
  // ===========================================================================

  const adapter = useChatStreamAdapter({
    api: API_ENDPOINT,
    threadId,
    headers,
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
    if (categoryName || category?.name) {
      return {
        type: "category" as const,
        name: categoryName || category?.name || "Agent",
      };
    }
    return null;
  }, [skillName, categoryName, category]);

  // ===========================================================================
  // Error Handling
  // ===========================================================================

  if (skillsError) {
    return (
      <div className="test-chat-page">
        <div className="test-header">
          <div className="test-header-left">
            <div className="test-badge">
              <span className="test-icon">⚠️</span>
              <div className="test-info">
                <span className="test-label">Error</span>
                <span className="test-name">Failed to load skills</span>
              </div>
            </div>
          </div>
          <button className="close-test-btn" onClick={handleClose}>
            <XIcon size={16} />
            Close
          </button>
        </div>
        <div className="test-content">
          <div className="test-chat-area">
            <div style={{ 
              display: 'flex', 
              alignItems: 'center', 
              justifyContent: 'center',
              height: '100%',
              color: '#ef4444',
              padding: '20px',
              textAlign: 'center'
            }}>
              <div>
                <p style={{ fontSize: '16px', fontWeight: 600, marginBottom: '8px' }}>
                  Failed to load skills
                </p>
                <p style={{ fontSize: '14px', color: '#8e8e8e' }}>
                  {skillsError}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // ===========================================================================
  // Render
  // ===========================================================================

  return (
    <div className="test-chat-page">
      <AssistantRuntimeProvider runtime={runtime}>
        {/* Test Header Banner */}
        <div className="test-header">
          <div className="test-header-left">
            <button 
              className="sidebar-toggle-btn"
              onClick={handleToggleSidebar}
              title={sidebarOpen ? "Hide skills" : "Show skills"}
            >
              {sidebarOpen ? <ChevronLeftIcon size={20} /> : <ChevronRightIcon size={20} />}
            </button>
            
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
          </div>
          
          <button className="close-test-btn" onClick={handleClose}>
            <XIcon size={16} />
            Close Test
          </button>
        </div>

        {/* Main Content */}
        <div className="test-content">
          {/* Skills Sidebar */}
          {sidebarOpen && (
            <SkillsSidebar
              skills={skills}
              loading={skillsLoading}
              activeSkillId={skillId}
              categoryId={categoryId}
              onViewSkill={handleViewSkill}
            />
          )}

          {/* Chat Area */}
          <div className="test-chat-area">
            <Thread
              onToggleSidebar={() => {}}
              sidebarOpen={false}
              isStreaming={isStreaming}
            />
          </div>
        </div>

        {/* Skill Instructions Modal */}
        {selectedSkill && (
          <SkillInstructionsModal
            skill={selectedSkill}
            categoryId={categoryId}
            onClose={handleCloseSkillModal}
          />
        )}
      </AssistantRuntimeProvider>
    </div>
  );
}