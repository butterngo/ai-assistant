// =============================================================================
// Settings Layout - Left menu + content area
// =============================================================================

import { type FC } from "react";
import { Outlet, useNavigate, useLocation } from "react-router-dom";
import {
  ArrowLeftIcon,
  LayersIcon,
  WrenchIcon,
  DatabaseIcon,
  UserIcon,
  SettingsIcon,
} from "lucide-react";
import "./SettingsLayout.css";

// =============================================================================
// Menu Items Configuration
// =============================================================================

interface MenuItem {
  path: string;
  label: string;
  icon: FC<{ size?: number }>;
  // Match pattern for nested routes
  matchPattern?: RegExp;
}

const configurationItems: MenuItem[] = [
  { 
    path: "/settings/agents", 
    label: "Agents", 
    icon: LayersIcon,
    // Also highlight when viewing skills inside a category
    matchPattern: /^\/settings\/agents/,
  },
  { path: "/settings/tools", label: "Tools", icon: WrenchIcon },
  { path: "/settings/knowledge-base", label: "Knowledge Base", icon: DatabaseIcon },
];

const accountItems: MenuItem[] = [
  { path: "/settings/profile", label: "Profile", icon: UserIcon },
];

// =============================================================================
// Settings Layout Component
// =============================================================================

export const SettingsLayout: FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleBack = () => {
    navigate("/");
  };

  // Check if menu item is active
  const isActive = (item: MenuItem): boolean => {
    if (item.matchPattern) {
      return item.matchPattern.test(location.pathname);
    }
    return location.pathname === item.path;
  };

  return (
    <div className="settings-layout">
      {/* Left Menu */}
      <aside className="settings-sidebar">
        {/* Header */}
        <div className="settings-sidebar-header">
          <SettingsIcon size={20} />
          <span>Settings</span>
        </div>

        {/* Navigation */}
        <nav className="settings-nav">
          {/* Configuration Section */}
          <div className="settings-nav-section">
            <div className="settings-nav-section-header">Configuration</div>
            {configurationItems.map((item) => (
              <button
                key={item.path}
                className={`settings-nav-item ${isActive(item) ? "active" : ""}`}
                onClick={() => navigate(item.path)}
              >
                <item.icon size={18} />
                <span>{item.label}</span>
              </button>
            ))}
          </div>

          {/* Account Section */}
          <div className="settings-nav-section">
            <div className="settings-nav-section-header">Account</div>
            {accountItems.map((item) => (
              <button
                key={item.path}
                className={`settings-nav-item ${isActive(item) ? "active" : ""}`}
                onClick={() => navigate(item.path)}
              >
                <item.icon size={18} />
                <span>{item.label}</span>
              </button>
            ))}
          </div>
        </nav>

        {/* Footer */}
        <div className="settings-sidebar-footer">
          <button className="settings-back-btn" onClick={handleBack}>
            <ArrowLeftIcon size={18} />
            <span>Back to Chat</span>
          </button>
        </div>
      </aside>

      {/* Content Area */}
      <main className="settings-content">
        <Outlet />
      </main>
    </div>
  );
};

export default SettingsLayout;