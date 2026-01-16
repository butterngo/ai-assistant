import type { FC } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeftIcon, UserIcon } from "lucide-react";
import "../SettingsPage.css";

export const ProfilePage: FC = () => {
  const navigate = useNavigate();

  return (
    <div className="settings-page">
      <header className="settings-page-header">
        <button className="back-btn" onClick={() => navigate(-1)}>
          <ArrowLeftIcon size={20} />
        </button>
        <div className="settings-page-title">
          <UserIcon size={24} />
          <h1>Profile</h1>
        </div>
      </header>
      <main className="settings-page-content">
        <p>Profile settings coming soon...</p>
      </main>
    </div>
  );
};