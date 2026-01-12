// =============================================================================
// Sidebar Component - Conversation list like Claude
// =============================================================================

import { type FC, useMemo } from "react";
import {
  PlusIcon,
  MessageSquareIcon,
  TrashIcon,
  PanelLeftCloseIcon,
  PanelLeftOpenIcon,
} from "lucide-react";
import type { Conversation } from "../types/Conversation";
import "./Sidebar.css";

interface SidebarProps {
  conversations: Conversation[];
  activeId: string | null;
  isOpen: boolean;
  onToggle: () => void;
  onNewConversation: () => void;
  onSelectConversation: (id: string) => void;
  onDeleteConversation: (id: string) => void;
}

// =============================================================================
// Date Helpers
// =============================================================================

function parseDate(dateInput: string | Date): Date {
  if (dateInput instanceof Date) return dateInput;
  return new Date(dateInput);
}

function getDateBoundaries() {
  const now = new Date();

  const today = new Date(now);
  today.setHours(0, 0, 0, 0);

  const yesterday = new Date(today);
  yesterday.setDate(yesterday.getDate() - 1);

  const lastWeek = new Date(today);
  lastWeek.setDate(lastWeek.getDate() - 7);

  const lastMonth = new Date(today);
  lastMonth.setMonth(lastMonth.getMonth() - 1);

  return { today, yesterday, lastWeek, lastMonth };
}

interface GroupedConversations {
  today: Conversation[];
  yesterday: Conversation[];
  lastWeek: Conversation[];
  lastMonth: Conversation[];
  older: Conversation[];
}

function groupConversationsByDate(conversations: Conversation[]): GroupedConversations {
  const { today, yesterday, lastWeek, lastMonth } = getDateBoundaries();

  const groups: GroupedConversations = {
    today: [],
    yesterday: [],
    lastWeek: [],
    lastMonth: [],
    older: [],
  };

  // Sort by updatedAt (most recent first)
  const sorted = [...conversations].sort((a, b) => {
    const dateA = parseDate(a.updatedAt || a.createdAt);
    const dateB = parseDate(b.updatedAt || b.createdAt);
    return dateB.getTime() - dateA.getTime();
  });

  for (const conv of sorted) {
    const date = parseDate(conv.updatedAt || conv.createdAt);

    if (date >= today) {
      groups.today.push(conv);
    } else if (date >= yesterday) {
      groups.yesterday.push(conv);
    } else if (date >= lastWeek) {
      groups.lastWeek.push(conv);
    } else if (date >= lastMonth) {
      groups.lastMonth.push(conv);
    } else {
      groups.older.push(conv);
    }
  }

  return groups;
}

// =============================================================================
// Sidebar Component
// =============================================================================

export const Sidebar: FC<SidebarProps> = ({
  conversations,
  activeId,
  isOpen,
  onToggle,
  onNewConversation,
  onSelectConversation,
  onDeleteConversation,
}) => {
  // Memoize grouped conversations
  const groupedConversations = useMemo(
    () => groupConversationsByDate(conversations),
    [conversations]
  );

  const hasConversations = conversations.length > 0;

  return (
    <aside className={`sidebar ${isOpen ? "open" : "closed"}`}>
      {/* Header */}
      <div className="sidebar-header">
        <button className="new-chat-btn" onClick={onNewConversation} title="New chat">
          <PlusIcon size={18} />
          {isOpen && <span>New chat</span>}
        </button>
        <button className="toggle-btn" onClick={onToggle} aria-label="Toggle sidebar">
          {isOpen ? <PanelLeftCloseIcon size={18} /> : <PanelLeftOpenIcon size={18} />}
        </button>
      </div>

      {/* Conversation List */}
      {isOpen && (
        <nav className="conversation-list">
          <ConversationGroup
            label="Today"
            conversations={groupedConversations.today}
            activeId={activeId}
            onSelect={onSelectConversation}
            onDelete={onDeleteConversation}
          />

          <ConversationGroup
            label="Yesterday"
            conversations={groupedConversations.yesterday}
            activeId={activeId}
            onSelect={onSelectConversation}
            onDelete={onDeleteConversation}
          />

          <ConversationGroup
            label="Previous 7 days"
            conversations={groupedConversations.lastWeek}
            activeId={activeId}
            onSelect={onSelectConversation}
            onDelete={onDeleteConversation}
          />

          <ConversationGroup
            label="Previous 30 days"
            conversations={groupedConversations.lastMonth}
            activeId={activeId}
            onSelect={onSelectConversation}
            onDelete={onDeleteConversation}
          />

          <ConversationGroup
            label="Older"
            conversations={groupedConversations.older}
            activeId={activeId}
            onSelect={onSelectConversation}
            onDelete={onDeleteConversation}
          />

          {!hasConversations && (
            <div className="empty-state">
              <MessageSquareIcon size={32} />
              <p>No conversations yet</p>
              <span>Start a new chat to begin</span>
            </div>
          )}
        </nav>
      )}

      {/* Footer */}
      {isOpen && (
        <div className="sidebar-footer">
          <div className="user-info">
            <div className="user-avatar">V</div>
            <span>Vu Ngo</span>
          </div>
        </div>
      )}
    </aside>
  );
};

// =============================================================================
// Conversation Group
// =============================================================================

interface ConversationGroupProps {
  label: string;
  conversations: Conversation[];
  activeId: string | null;
  onSelect: (id: string) => void;
  onDelete: (id: string) => void;
}

const ConversationGroup: FC<ConversationGroupProps> = ({
  label,
  conversations,
  activeId,
  onSelect,
  onDelete,
}) => {
  if (conversations.length === 0) return null;

  return (
    <div className="conversation-group">
      <div className="group-label">{label}</div>
      {conversations.map((conv) => (
        <ConversationItem
          key={conv.id}
          conversation={conv}
          isActive={conv.id === activeId || conv.threadId === activeId}
          onSelect={() => onSelect(conv.id)}
          onDelete={() => onDelete(conv.id)}
        />
      ))}
    </div>
  );
};

// =============================================================================
// Conversation Item
// =============================================================================

interface ConversationItemProps {
  conversation: Conversation;
  isActive: boolean;
  onSelect: () => void;
  onDelete: () => void;
}

const ConversationItem: FC<ConversationItemProps> = ({
  conversation,
  isActive,
  onSelect,
  onDelete,
}) => {
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      onSelect();
    }
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.stopPropagation();
    onDelete();
  };

  return (
    <div
      className={`conversation-item ${isActive ? "active" : ""}`}
      onClick={onSelect}
      onKeyDown={handleKeyDown}
      role="button"
      tabIndex={0}
      aria-selected={isActive}
    >
      <MessageSquareIcon size={16} className="conv-icon" />
      <div className="conv-content">
        <div className="conv-title">{conversation.title || "New conversation"}</div>
        {conversation.preview && <div className="conv-preview">{conversation.preview}</div>}
      </div>
      <button
        className="delete-btn"
        onClick={handleDelete}
        aria-label={`Delete conversation: ${conversation.title}`}
        title="Delete conversation"
      >
        <TrashIcon size={14} />
      </button>
    </div>
  );
};

export default Sidebar;