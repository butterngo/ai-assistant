// =============================================================================
// Settings Component - User Menu with Settings Popover
// =============================================================================

import { type FC, useState, useRef, useEffect } from "react";
import {
  SettingsIcon,
  LayersIcon,
  BrainIcon,
  WrenchIcon,
  DatabaseIcon,
  LogOutIcon,
  UserIcon,
} from "lucide-react";
import "./Settings.css";

// =============================================================================
// Types
// =============================================================================

export interface SettingsProps {
  userName?: string;
  userAvatar?: string;
  onOpenCategories?: () => void;
  onOpenSkills?: () => void;
  onOpenTools?: () => void;
  onOpenKnowledgeBase?: () => void;
  onOpenProfile?: () => void;
  onLogout?: () => void;
}

// =============================================================================
// Settings Component
// =============================================================================

export const Settings: FC<SettingsProps> = ({
  userName = "Vu Ngo",
  userAvatar = "V",
  onOpenCategories,
  onOpenSkills,
  onOpenTools,
  onOpenKnowledgeBase,
  onOpenProfile,
  onLogout,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  // ---------------------------------------------------------------------------
  // Close popover when clicking outside
  // ---------------------------------------------------------------------------
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  // ---------------------------------------------------------------------------
  // Close on Escape key
  // ---------------------------------------------------------------------------
  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener("keydown", handleEscape);
    }

    return () => {
      document.removeEventListener("keydown", handleEscape);
    };
  }, [isOpen]);

  // ---------------------------------------------------------------------------
  // Handle menu item click
  // ---------------------------------------------------------------------------
  const handleItemClick = (callback?: () => void) => {
    setIsOpen(false);
    callback?.();
  };

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------
  return (
    <div className="settings-menu" ref={menuRef}>
      {/* Settings Popover */}
      {isOpen && (
        <div className="settings-popover">
          {/* Configuration Section */}
          <div className="popover-section">
            <div className="popover-section-header">Configuration</div>
            <button
              className="popover-item"
              onClick={() => handleItemClick(onOpenCategories)}
            >
              <LayersIcon size={16} />
              <span>Categories</span>
            </button>
            <button
              className="popover-item"
              onClick={() => handleItemClick(onOpenSkills)}
            >
              <BrainIcon size={16} />
              <span>Skills</span>
            </button>
            <button
              className="popover-item"
              onClick={() => handleItemClick(onOpenTools)}
            >
              <WrenchIcon size={16} />
              <span>Tools</span>
            </button>
            <button
              className="popover-item"
              onClick={() => handleItemClick(onOpenKnowledgeBase)}
            >
              <DatabaseIcon size={16} />
              <span>Knowledge Base</span>
            </button>
          </div>

          {/* Divider */}
          <div className="popover-divider" />

          {/* Account Section */}
          <div className="popover-section">
            <button
              className="popover-item"
              onClick={() => handleItemClick(onOpenProfile)}
            >
              <UserIcon size={16} />
              <span>Profile</span>
            </button>
            <button
              className="popover-item popover-item-danger"
              onClick={() => handleItemClick(onLogout)}
            >
              <LogOutIcon size={16} />
              <span>Log out</span>
            </button>
          </div>
        </div>
      )}

      {/* User Info Row */}
      <div className="settings-user-row">
        <div
          className="settings-user-info"
          onClick={() => setIsOpen(!isOpen)}
          role="button"
          tabIndex={0}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              setIsOpen(!isOpen);
            }
          }}
        >
          <div className="settings-user-avatar">{userAvatar}</div>
          <span className="settings-user-name">{userName}</span>
        </div>
        <button
          className={`settings-btn ${isOpen ? "active" : ""}`}
          onClick={() => setIsOpen(!isOpen)}
          aria-label="Settings"
          title="Settings"
        >
          <SettingsIcon size={18} />
        </button>
      </div>
    </div>
  );
};

export default Settings;