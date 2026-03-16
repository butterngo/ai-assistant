import { type FC, type ReactNode, createContext, useContext, useState } from "react";
import "./Tabs.css";

// =============================================================================
// Context for managing active tab
// =============================================================================

interface TabsContextValue {
  activeTab: string;
  setActiveTab: (value: string) => void;
}

const TabsContext = createContext<TabsContextValue | undefined>(undefined);

const useTabsContext = () => {
  const context = useContext(TabsContext);
  if (!context) {
    throw new Error("Tab components must be used within Tabs");
  }
  return context;
};

// =============================================================================
// Main Tabs Component
// =============================================================================

interface TabsProps {
  defaultValue: string;
  value?: string;
  onChange?: (value: string) => void;
  children: ReactNode;
  className?: string;
}

export const Tabs: FC<TabsProps> = ({
  defaultValue,
  value: controlledValue,
  onChange,
  children,
  className = "",
}) => {
  const [internalValue, setInternalValue] = useState(defaultValue);

  // Use controlled value if provided, otherwise use internal state
  const activeTab = controlledValue !== undefined ? controlledValue : internalValue;

  const setActiveTab = (newValue: string) => {
    if (controlledValue === undefined) {
      setInternalValue(newValue);
    }
    onChange?.(newValue);
  };

  return (
    <TabsContext.Provider value={{ activeTab, setActiveTab }}>
      <div className={`tabs-container ${className}`}>
        {children}
      </div>
    </TabsContext.Provider>
  );
};

// =============================================================================
// TabsList - Container for tab buttons
// =============================================================================

interface TabsListProps {
  children: ReactNode;
  className?: string;
}

export const TabsList: FC<TabsListProps> = ({ children, className = "" }) => {
  return (
    <div className={`tabs-list ${className}`} role="tablist">
      {children}
    </div>
  );
};

// =============================================================================
// TabsTrigger - Individual tab button
// =============================================================================

interface TabsTriggerProps {
  value: string;
  children: ReactNode;
  icon?: ReactNode;
  badge?: string | number;
  disabled?: boolean;
  className?: string;
}

export const TabsTrigger: FC<TabsTriggerProps> = ({
  value,
  children,
  icon,
  badge,
  disabled = false,
  className = "",
}) => {
  const { activeTab, setActiveTab } = useTabsContext();
  const isActive = activeTab === value;

  const handleClick = () => {
    if (!disabled) {
      setActiveTab(value);
    }
  };

  return (
    <button
      type="button"
      role="tab"
      aria-selected={isActive}
      aria-disabled={disabled}
      disabled={disabled}
      className={`tabs-trigger ${isActive ? "active" : ""} ${disabled ? "disabled" : ""} ${className}`}
      onClick={handleClick}
    >
      {icon && <span className="tab-icon">{icon}</span>}
      <span className="tab-label">{children}</span>
      {badge !== undefined && badge !== "" && (
        <span className="tab-badge">{badge}</span>
      )}
    </button>
  );
};

// =============================================================================
// TabsContent - Content for each tab
// =============================================================================

interface TabsContentProps {
  value: string;
  children: ReactNode;
  className?: string;
}

export const TabsContent: FC<TabsContentProps> = ({
  value,
  children,
  className = "",
}) => {
  const { activeTab } = useTabsContext();
  const isActive = activeTab === value;

  if (!isActive) return null;

  return (
    <div
      role="tabpanel"
      className={`tabs-content ${className}`}
    >
      {children}
    </div>
  );
};