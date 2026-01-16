// =============================================================================
// Thread Component - Full screen Claude-like layout
// =============================================================================

import {
  ActionBarPrimitive,
  BranchPickerPrimitive,
  ComposerPrimitive,
  MessagePrimitive,
  ThreadPrimitive,
} from "@assistant-ui/react";
import {
  ArrowDownIcon,
  SendIcon,
  CopyIcon,
  CheckIcon,
  RefreshCwIcon,
  PencilIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  PanelLeftIcon,
} from "lucide-react";
import type { FC } from "react";
import { MarkdownText } from "./rich-content/MarkdownText";
import "./Thread.css";

export interface ThreadProps {
  onToggleSidebar: () => void;
  sidebarOpen: boolean;
  isStreaming: boolean;
}

// =============================================================================
// Main Thread Component
// =============================================================================
export const Thread: FC<ThreadProps> = ({ onToggleSidebar, sidebarOpen, isStreaming }) => {
  return (
    <ThreadPrimitive.Root className="thread-root">
      {/* Header */}
      <header className="thread-header">
        {!sidebarOpen && (
          <button className="sidebar-toggle" onClick={onToggleSidebar} aria-label="Open sidebar">
            <PanelLeftIcon size={20} />
          </button>
        )}
        <div className="header-title">
          <span className="model-name">Claude</span>
        </div>
      </header>

      {/* Messages Area */}
      <ThreadPrimitive.Viewport className="thread-viewport">
        <ThreadPrimitive.Empty>
          <ThreadWelcome />
        </ThreadPrimitive.Empty>

        <ThreadPrimitive.Messages
          components={{
            UserMessage,
            AssistantMessage,
          }}
        />

        <ThreadPrimitive.ViewportFooter className="viewport-footer">
          <ThreadScrollToBottom />
          <Composer isStreaming={isStreaming} />
          <div className="footer-note">
            AI can make mistakes. Please verify important information.
          </div>
        </ThreadPrimitive.ViewportFooter>
      </ThreadPrimitive.Viewport>
    </ThreadPrimitive.Root>
  );
};

// =============================================================================
// Welcome Screen - Claude style
// =============================================================================
const ThreadWelcome: FC = () => {
  return (
    <div className="thread-welcome">
      <div className="welcome-logo">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2">
          <path d="M12 2L2 7l10 5 10-5-10-5z"/>
          <path d="M2 17l10 5 10-5"/>
          <path d="M2 12l10 5 10-5"/>
        </svg>
      </div>
      <h1>How can I help you today?</h1>
      <p>Ask me anything or choose a suggestion below.</p>
      
      <div className="welcome-suggestions">
        <button className="suggestion-btn">
          <span className="suggestion-icon">💡</span>
          <div className="suggestion-text">
            <strong>Explain a concept</strong>
            <span>Break down complex topics</span>
          </div>
        </button>
        <button className="suggestion-btn">
          <span className="suggestion-icon">✍️</span>
          <div className="suggestion-text">
            <strong>Help me write</strong>
            <span>Draft documents or emails</span>
          </div>
        </button>
        <button className="suggestion-btn">
          <span className="suggestion-icon">💻</span>
          <div className="suggestion-text">
            <strong>Write code</strong>
            <span>Debug or build applications</span>
          </div>
        </button>
        <button className="suggestion-btn">
          <span className="suggestion-icon">🔍</span>
          <div className="suggestion-text">
            <strong>Analyze content</strong>
            <span>Review and provide feedback</span>
          </div>
        </button>
      </div>
    </div>
  );
};

// =============================================================================
// Scroll to Bottom Button
// =============================================================================
const ThreadScrollToBottom: FC = () => {
  return (
    <ThreadPrimitive.ScrollToBottom asChild>
      <button className="scroll-to-bottom" aria-label="Scroll to bottom">
        <ArrowDownIcon size={20} />
      </button>
    </ThreadPrimitive.ScrollToBottom>
  );
};

// =============================================================================
// Composer
// =============================================================================
interface ComposerProps {
  isStreaming?: boolean;
}

const Composer: FC<ComposerProps> = ({ isStreaming = false }) => {
  return (
    <ComposerPrimitive.Root className="composer-root">
      <ComposerPrimitive.Input
        placeholder="Message AI Assistant..."
        className="composer-input"
        autoFocus
        disabled={isStreaming}
      />
      <ComposerPrimitive.Send className="composer-send">
        <SendIcon size={18} />
      </ComposerPrimitive.Send>
    </ComposerPrimitive.Root>
  );
};

// =============================================================================
// User Message - Claude style (left aligned, no bubble)
// =============================================================================
const UserMessage: FC = () => {
  return (
    <MessagePrimitive.Root className="message-root user-message">
      <div className="message-container">
        <div className="message-avatar user-avatar">V</div>
        <div className="message-content-wrapper">
          <div className="message-header">
            <span className="message-author">You</span>
          </div>
          <div className="message-content">
            <MessagePrimitive.Content />
          </div>
          <UserActionBar />
        </div>
      </div>
    </MessagePrimitive.Root>
  );
};

const UserActionBar: FC = () => {
  return (
    <ActionBarPrimitive.Root className="action-bar" hideWhenRunning autohide="not-last">
      <ActionBarPrimitive.Edit asChild>
        <button className="action-button" aria-label="Edit">
          <PencilIcon size={14} />
        </button>
      </ActionBarPrimitive.Edit>
    </ActionBarPrimitive.Root>
  );
};

// =============================================================================
// Loading Indicator - Typing dots animation
// =============================================================================
const LoadingIndicator: FC = () => {
  return (
    <div className="loading-indicator">
      <span className="loading-dot" />
      <span className="loading-dot" />
      <span className="loading-dot" />
    </div>
  );
};

// =============================================================================
// Assistant Message - Claude style (left aligned, no bubble)
// =============================================================================
const AssistantMessage: FC = () => {
  return (
    <MessagePrimitive.Root className="message-root assistant-message">
      <div className="message-container">
        <div className="message-avatar assistant-avatar">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <path d="M12 2L2 7l10 5 10-5-10-5z"/>
            <path d="M2 17l10 5 10-5"/>
            <path d="M2 12l10 5 10-5"/>
          </svg>
        </div>
        <div className="message-content-wrapper">
          <div className="message-header">
            <span className="message-author">Butter Assistant</span>
          </div>
          <div className="message-content">
          <MessagePrimitive.If hasContent={false}>
            <LoadingIndicator />
          </MessagePrimitive.If>
          <MessagePrimitive.If hasContent>
            <MessagePrimitive.Content components={{ Text: MarkdownText }} />
          </MessagePrimitive.If>
          </div>
          <div className="message-footer">
            <AssistantActionBar />
            <BranchPicker />
          </div>
        </div>
      </div>
    </MessagePrimitive.Root>
  );
};

const AssistantActionBar: FC = () => {
  return (
    <ActionBarPrimitive.Root className="action-bar" hideWhenRunning autohide="not-last">
      <ActionBarPrimitive.Copy asChild>
        <button className="action-button" aria-label="Copy">
          <MessagePrimitive.If copied>
            <CheckIcon size={14} />
          </MessagePrimitive.If>
          <MessagePrimitive.If copied={false}>
            <CopyIcon size={14} />
          </MessagePrimitive.If>
        </button>
      </ActionBarPrimitive.Copy>
      <ActionBarPrimitive.Reload asChild>
        <button className="action-button" aria-label="Regenerate">
          <RefreshCwIcon size={14} />
        </button>
      </ActionBarPrimitive.Reload>
    </ActionBarPrimitive.Root>
  );
};

// =============================================================================
// Branch Picker
// =============================================================================
const BranchPicker: FC = () => {
  return (
    <BranchPickerPrimitive.Root className="branch-picker" hideWhenSingleBranch>
      <BranchPickerPrimitive.Previous asChild>
        <button className="branch-button" aria-label="Previous">
          <ChevronLeftIcon size={14} />
        </button>
      </BranchPickerPrimitive.Previous>
      <span className="branch-state">
        <BranchPickerPrimitive.Number /> / <BranchPickerPrimitive.Count />
      </span>
      <BranchPickerPrimitive.Next asChild>
        <button className="branch-button" aria-label="Next">
          <ChevronRightIcon size={14} />
        </button>
      </BranchPickerPrimitive.Next>
    </BranchPickerPrimitive.Root>
  );
};

export default Thread;
