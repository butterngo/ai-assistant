// =============================================================================
// Sidebar Component - Conversation list like Claude
// =============================================================================

import { type FC } from "react";
import { PlusIcon, MessageSquareIcon, TrashIcon, PanelLeftCloseIcon } from "lucide-react";
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

export const Sidebar: FC<SidebarProps> = ({
  conversations,
  activeId,
  isOpen,
  onToggle,
  onNewConversation,
  onSelectConversation,
  onDeleteConversation,
}) => {
  // Group conversations by date
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const yesterday = new Date(today);
  yesterday.setDate(yesterday.getDate() - 1);
  const lastWeek = new Date(today);
  lastWeek.setDate(lastWeek.getDate() - 7);

  const groupedConversations = {
    today: conversations.filter((c) => c.createdAt >= today),
    yesterday: conversations.filter((c) => c.createdAt >= yesterday && c.createdAt < today),
    lastWeek: conversations.filter((c) => c.createdAt >= lastWeek && c.createdAt < yesterday),
    older: conversations.filter((c) => c.createdAt < lastWeek),
  };

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  };

  return (
    <aside className={`sidebar ${isOpen ? "open" : "closed"}`}>
      {/* Header */}
      <div className="sidebar-header">
        <button className="new-chat-btn" onClick={onNewConversation}>
          <PlusIcon size={18} />
          <span>New chat</span>
        </button>
        <button className="toggle-btn" onClick={onToggle} aria-label="Close sidebar">
          <PanelLeftCloseIcon size={18} />
        </button>
      </div>

      {/* Conversation List */}
      <nav className="conversation-list">
        {groupedConversations.today.length > 0 && (
          <div className="conversation-group">
            <div className="group-label">Today</div>
            {groupedConversations.today.map((conv) => (
              <ConversationItem
                key={conv.id}
                conversation={conv}
                isActive={conv.id === activeId}
                onSelect={() => onSelectConversation(conv.id)}
                onDelete={() => onDeleteConversation(conv.id)}
              />
            ))}
          </div>
        )}

        {groupedConversations.yesterday.length > 0 && (
          <div className="conversation-group">
            <div className="group-label">Yesterday</div>
            {groupedConversations.yesterday.map((conv) => (
              <ConversationItem
                key={conv.id}
                conversation={conv}
                isActive={conv.id === activeId}
                onSelect={() => onSelectConversation(conv.id)}
                onDelete={() => onDeleteConversation(conv.id)}
              />
            ))}
          </div>
        )}

        {groupedConversations.lastWeek.length > 0 && (
          <div className="conversation-group">
            <div className="group-label">Previous 7 days</div>
            {groupedConversations.lastWeek.map((conv) => (
              <ConversationItem
                key={conv.id}
                conversation={conv}
                isActive={conv.id === activeId}
                onSelect={() => onSelectConversation(conv.id)}
                onDelete={() => onDeleteConversation(conv.id)}
              />
            ))}
          </div>
        )}

        {groupedConversations.older.length > 0 && (
          <div className="conversation-group">
            <div className="group-label">Older</div>
            {groupedConversations.older.map((conv) => (
              <ConversationItem
                key={conv.id}
                conversation={conv}
                isActive={conv.id === activeId}
                onSelect={() => onSelectConversation(conv.id)}
                onDelete={() => onDeleteConversation(conv.id)}
              />
            ))}
          </div>
        )}

        {conversations.length === 0 && (
          <div className="empty-state">
            <MessageSquareIcon size={32} />
            <p>No conversations yet</p>
            <span>Start a new chat to begin</span>
          </div>
        )}
      </nav>

      {/* Footer */}
      <div className="sidebar-footer">
        <div className="user-info">
          <div className="user-avatar">V</div>
          <span>Vu Ngo</span>
        </div>
      </div>
    </aside>
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
  return (
    <div
      className={`conversation-item ${isActive ? "active" : ""}`}
      onClick={onSelect}
      role="button"
      tabIndex={0}
    >
      <MessageSquareIcon size={16} className="conv-icon" />
      <div className="conv-content">
        <div className="conv-title">{conversation.title}</div>
        {conversation.preview && (
          <div className="conv-preview">{conversation.preview}</div>
        )}
      </div>
      <button
        className="delete-btn"
        onClick={(e) => {
          e.stopPropagation();
          onDelete();
        }}
        aria-label="Delete conversation"
      >
        <TrashIcon size={14} />
      </button>
    </div>
  );
};

export default Sidebar;
