import type { FC } from "react";
import { WrenchIcon } from "lucide-react";
import "./EmptyStatePage.css";

export interface EmptyStatePageProps {
  onCreateConnection: () => void;
}

export const EmptyStatePage: FC<EmptyStatePageProps> = ({ onCreateConnection }) => {
  return (
    <div className="empty-state-page">
      <div className="empty-state-icon">
        <WrenchIcon size={64} />
      </div>
      
      <h2 className="empty-state-title">No connections yet</h2>
      
      <p className="empty-state-description">
        Connect external tools like GitHub, Jira, or Azure DevOps
        <br />
        to enhance your AI agents with powerful capabilities
      </p>

      <button className="primary-btn create-btn" onClick={onCreateConnection}>
        + Create Connection
      </button>

      <div className="popular-connections">
        <h3>Popular connections:</h3>
        <div className="connection-cards">
          <div className="connection-card">
            <div className="card-icon">📦</div>
            <div className="card-name">GitHub</div>
            <button className="setup-btn" onClick={onCreateConnection}>
              Setup
            </button>
          </div>

          <div className="connection-card">
            <div className="card-icon">📊</div>
            <div className="card-name">Azure DevOps</div>
            <button className="setup-btn" onClick={onCreateConnection}>
              Setup
            </button>
          </div>

          <div className="connection-card">
            <div className="card-icon">🎫</div>
            <div className="card-name">Jira</div>
            <button className="setup-btn" onClick={onCreateConnection}>
              Setup
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};