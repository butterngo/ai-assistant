import type { FC } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeftIcon, DatabaseIcon } from "lucide-react";
import "../SettingsPage.css";

export const KnowledgeBasePage: FC = () => {
  const navigate = useNavigate();

  return (
    <div className="settings-page">
      <header className="settings-page-header">
        <button className="back-btn" onClick={() => navigate(-1)}>
          <ArrowLeftIcon size={20} />
        </button>
        <div className="settings-page-title">
          <DatabaseIcon size={24} />
          <h1>Knowledge Base</h1>
        </div>
      </header>
      <main className="settings-page-content">
        <p>Knowledge Base management coming soon...</p>
      </main>
    </div>
  );
};