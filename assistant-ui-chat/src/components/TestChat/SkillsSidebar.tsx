import { FC } from "react";
import { useNavigate } from "react-router-dom";
import { 
  BrainIcon,
  ExternalLinkIcon,
  EyeIcon
} from "lucide-react";
import type { Skill } from "../../types";
import "./SkillsSidebar.css";

interface SkillsSidebarProps {
  skills: Skill[];
  loading: boolean;
  activeSkillId?: string | null;
  agentId?: string | null;
  onViewSkill: (skill: Skill) => void;
}

export const SkillsSidebar: FC<SkillsSidebarProps> = ({
  skills,
  loading,
  activeSkillId,
  agentId,
  onViewSkill,
}) => {
  const navigate = useNavigate();

  const handleEditSkill = (skill: Skill) => {
    navigate(`/settings/agents/${agentId}/skills`, {
      state: { editSkillId: skill.id }
    });
  };

  return (
    <aside className="skills-sidebar">
      <div className="skills-sidebar-header">
        <div className="sidebar-title">
          <BrainIcon size={18} />
          <span>Skills</span>
          {!loading && (
            <span className="skill-count">({skills.length})</span>
          )}
        </div>
      </div>

      <div className="skills-sidebar-content">
        {loading ? (
          <div className="sidebar-loading">
            <div className="loading-spinner" />
            <p>Loading skills...</p>
          </div>
        ) : skills.length === 0 ? (
          <div className="sidebar-empty">
            <BrainIcon size={32} />
            <p>No skills yet</p>
            <span>Add skills to this agent</span>
          </div>
        ) : (
          <div className="skills-list">
            {skills.map((skill) => (
              <div 
                key={skill.id} 
                className={`skill-item ${activeSkillId === skill.id ? 'active' : ''}`}
              >
                <div className="skill-item-header">
                  <div className="skill-item-icon">
                    <BrainIcon size={16} />
                  </div>
                  <div className="skill-item-info">
                    <h4 className="skill-item-name">{skill.name}</h4>
                    <p className="skill-item-description">
                      {skill.description || "No description"}
                    </p>
                  </div>
                </div>
                
                <div className="skill-item-actions">
                  <button
                    className="skill-action-btn"
                    onClick={() => onViewSkill(skill)}
                    title="View instructions"
                  >
                    <EyeIcon size={14} />
                  </button>
                  <button
                    className="skill-action-btn"
                    onClick={() => handleEditSkill(skill)}
                    title="Edit skill"
                  >
                    <ExternalLinkIcon size={14} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="skills-sidebar-footer">
        <p className="sidebar-hint">
          💡 Skills are loaded automatically when testing
        </p>
      </div>
    </aside>
  );
};