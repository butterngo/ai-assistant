import { FC } from "react";
import { useNavigate } from "react-router-dom";
import { XIcon, ExternalLinkIcon } from "lucide-react";
import type { Skill } from "../../types";
import "./SkillInstructionsModal.css";

interface SkillInstructionsModalProps {
  skill: Skill;
  agentId?: string | null;
  onClose: () => void;
}

export const SkillInstructionsModal: FC<SkillInstructionsModalProps> = ({
  skill,
  agentId,
  onClose,
}) => {
  const navigate = useNavigate();

  const handleEdit = () => {
    navigate(`/settings/agents/${agentId}/skills`, {
      state: { editSkillId: skill.id }
    });
  };

  return (
    <div className="skill-modal-overlay" onClick={onClose}>
      <div className="skill-modal" onClick={(e) => e.stopPropagation()}>
        <div className="skill-modal-header">
          <h3>{skill.name}</h3>
          <button 
            className="skill-modal-close"
            onClick={onClose}
          >
            <XIcon size={20} />
          </button>
        </div>
        
        <div className="skill-modal-content">
          <div className="skill-modal-section">
            <label>Description</label>
            <p>{skill.description || "No description provided"}</p>
          </div>
          
          <div className="skill-modal-section">
            <label>Instructions</label>
            <pre className="skill-instructions">
              {skill.systemPrompt || "No instructions provided"}
            </pre>
          </div>
        </div>
        
        <div className="skill-modal-footer">
          <button 
            className="btn btn-secondary"
            onClick={onClose}
          >
            Close
          </button>
          <button 
            className="btn btn-primary"
            onClick={handleEdit}
          >
            <ExternalLinkIcon size={16} />
            Edit Skill
          </button>
        </div>
      </div>
    </div>
  );
};