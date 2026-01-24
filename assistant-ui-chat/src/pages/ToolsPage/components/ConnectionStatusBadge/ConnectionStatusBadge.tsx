import type { FC } from "react";
import "./ConnectionStatusBadge.css";

export interface ConnectionStatusBadgeProps {
  isActive: boolean;
  lastTestStatus?: 'success' | 'failed' | 'never';
}

export const ConnectionStatusBadge: FC<ConnectionStatusBadgeProps> = ({ 
  isActive, 
  lastTestStatus = 'never' 
}) => {
  const getStatusInfo = () => {
    if (!isActive) {
      return { icon: '🔴', text: 'Inactive', className: 'inactive' };
    }
    
    if (lastTestStatus === 'failed') {
      return { icon: '🔴', text: 'Failed', className: 'failed' };
    }
    
    if (lastTestStatus === 'success') {
      return { icon: '🟢', text: 'Active', className: 'active' };
    }
    
    return { icon: '🟡', text: 'New', className: 'new' };
  };

  const status = getStatusInfo();

  return (
    <span className={`connection-status-badge ${status.className}`}>
      <span className="status-icon">{status.icon}</span>
      <span className="status-text">{status.text}</span>
    </span>
  );
};