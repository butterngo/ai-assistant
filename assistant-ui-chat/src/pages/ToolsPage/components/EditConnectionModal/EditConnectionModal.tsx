import { type FC, useState, useEffect } from "react";
import { Modal, FormField } from "../../../../components";
import type {
  ConnectionTool,
  UpdateConnectionToolRequest,
  ConnectionToolType,
} from "../../../../types";
import "./EditConnectionModal.css";

export interface EditConnectionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (id: string, data: UpdateConnectionToolRequest) => Promise<void>;
  connection: ConnectionTool | null;
  fullscreen?: boolean;
}

interface FormData {
  name: string;
  type: ConnectionToolType;
  description: string;
  command: string;
  arguments: string;
  endpoint: string;
  environmentVariables: Record<string, string>;
  isActive: boolean;
}

interface FormErrors {
  name?: string;
  command?: string;
  endpoint?: string;
}

export const EditConnectionModal: FC<EditConnectionModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  connection,
  fullscreen = false,
}) => {
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<FormData>({
    name: "",
    type: "mcp_stdio",
    description: "",
    command: "",
    arguments: "",
    endpoint: "",
    environmentVariables: {},
    isActive: true,
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [envKey, setEnvKey] = useState("");
  const [envValue, setEnvValue] = useState("");
  const [showAdvanced, setShowAdvanced] = useState(false);

  // Load connection data when modal opens
  useEffect(() => {
    if (isOpen && connection) {
      // Parse config
      const config = connection.config || {};
      const args = config.arguments || [];
      const envVars = config.environmentVariables || {};

      setFormData({
        name: connection.name,
        type: connection.type as ConnectionToolType,
        description: connection.description || "",
        command: connection.command || config.command || "",
        arguments: Array.isArray(args) ? args.join("\n") : "",
        endpoint: connection.endpoint || config.endpoint || "",
        environmentVariables: envVars,
        isActive: connection.isActive,
      });
    }
  }, [isOpen, connection]);

  // ---------------------------------------------------------------------------
  // Validation
  // ---------------------------------------------------------------------------
  const validate = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = "Name is required";
    }

    if (formData.type === "mcp_stdio" && !formData.command.trim()) {
      newErrors.command = "Command is required for MCP (stdio)";
    }

    if (
      (formData.type === "mcp_http" || formData.type === "openapi") &&
      !formData.endpoint.trim()
    ) {
      newErrors.endpoint = "Endpoint is required for this type";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------
  const handleAddEnvVar = () => {
    if (envKey.trim() && envValue.trim()) {
      setFormData((prev) => ({
        ...prev,
        environmentVariables: {
          ...prev.environmentVariables,
          [envKey]: envValue,
        },
      }));
      setEnvKey("");
      setEnvValue("");
    }
  };

  const handleRemoveEnvVar = (key: string) => {
    setFormData((prev) => {
      const newVars = { ...prev.environmentVariables };
      delete newVars[key];
      return { ...prev, environmentVariables: newVars };
    });
  };

  const handleSubmit = async () => {
    if (!connection || !validate()) return;

    setLoading(true);
    try {
      const config: any = {};

      if (formData.type === "mcp_stdio") {
        config.command = formData.command;
        config.arguments = formData.arguments
          .split("\n")
          .map((arg) => arg.trim())
          .filter(Boolean);
        config.environmentVariables = formData.environmentVariables;
      } else if (formData.type === "mcp_http" || formData.type === "openapi") {
        config.endpoint = formData.endpoint;
      }

      const request: UpdateConnectionToolRequest = {
        name: formData.name,
        type: formData.type,
        description: formData.description || undefined,
        command: formData.type === "mcp_stdio" ? formData.command : undefined,
        endpoint: formData.type !== "mcp_stdio" ? formData.endpoint : undefined,
        config: { ...config },
        isActive: formData.isActive,
      };

      await onSubmit(connection.id, request);
      handleClose();
    } catch (error) {
      console.error("Failed to update connection:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setErrors({});
    setShowAdvanced(false);
    setEnvKey("");
    setEnvValue("");
    onClose();
  };

  if (!connection) return null;

  const footer = (
    <>
      <button className="secondary-btn" onClick={handleClose} disabled={loading}>
        Cancel
      </button>
      <button className="primary-btn" onClick={handleSubmit} disabled={loading}>
        {loading ? "Saving..." : "Save Changes"}
      </button>
    </>
  );

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title="Edit Connection"
      footer={footer}
      size="xl"
      fullscreen={fullscreen}
      closeOnOverlayClick={false}
      closeOnEscape={!loading}
    >
      <div className="edit-connection-content">
        <FormField
          label="Name"
          htmlFor="edit-name"
          required
          error={errors.name}
          hint="A unique name for this connection"
        >
          <input
            id="edit-name"
            type="text"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            placeholder="e.g., GitHub"
          />
        </FormField>

        <FormField label="Type" htmlFor="edit-type">
          <input
            id="edit-type"
            type="text"
            value={formData.type}
            disabled
            style={{ opacity: 0.6, cursor: "not-allowed" }}
          />
          <p className="field-hint">Type cannot be changed after creation</p>
        </FormField>

        <FormField
          label="Description"
          htmlFor="edit-description"
          hint="Optional description for this connection"
        >
          <textarea
            id="edit-description"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            rows={3}
            placeholder="e.g., GitHub integration via MCP"
          />
        </FormField>

        <FormField label="Status" htmlFor="edit-isActive">
          <label className="checkbox-label">
            <input
              id="edit-isActive"
              type="checkbox"
              checked={formData.isActive}
              onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
            />
            <span>Active</span>
          </label>
        </FormField>

        <div className="divider" />

        <h3 className="section-heading">Configuration</h3>

        {formData.type === "mcp_stdio" && (
          <>
            <FormField
              label="Command"
              htmlFor="edit-command"
              required
              error={errors.command}
              hint="The command to execute (e.g., npx)"
            >
              <input
                id="edit-command"
                type="text"
                value={formData.command}
                onChange={(e) => setFormData({ ...formData, command: e.target.value })}
                placeholder="npx"
              />
            </FormField>

            <FormField
              label="Arguments"
              htmlFor="edit-arguments"
              hint="One argument per line"
            >
              <textarea
                id="edit-arguments"
                value={formData.arguments}
                onChange={(e) => setFormData({ ...formData, arguments: e.target.value })}
                rows={4}
                placeholder="-y&#10;@modelcontextprotocol/server-github"
              />
            </FormField>

            <div className="env-vars-section">
              <label className="section-label">Environment Variables</label>

              {Object.entries(formData.environmentVariables).length > 0 && (
                <div className="env-vars-list">
                  {Object.entries(formData.environmentVariables).map(([key, value]) => (
                    <div key={key} className="env-var-item">
                      <div className="env-var-info">
                        <strong>{key}</strong>
                        <span className="env-var-value">
                          {"•".repeat(Math.min(value.length, 20))}
                        </span>
                      </div>
                      <button
                        type="button"
                        className="remove-btn"
                        onClick={() => handleRemoveEnvVar(key)}
                      >
                        ✕
                      </button>
                    </div>
                  ))}
                </div>
              )}

              <div className="env-var-inputs">
                <input
                  type="text"
                  placeholder="Key (e.g., GITHUB_PERSONAL_ACCESS_TOKEN)"
                  value={envKey}
                  onChange={(e) => setEnvKey(e.target.value)}
                />
                <input
                  type="password"
                  placeholder="Value"
                  value={envValue}
                  onChange={(e) => setEnvValue(e.target.value)}
                />
                <button
                  type="button"
                  className="add-btn"
                  onClick={handleAddEnvVar}
                  disabled={!envKey.trim() || !envValue.trim()}
                >
                  + Add
                </button>
              </div>
            </div>
          </>
        )}

        {(formData.type === "mcp_http" || formData.type === "openapi") && (
          <FormField
            label="Endpoint"
            htmlFor="edit-endpoint"
            required
            error={errors.endpoint}
            hint="The HTTP endpoint URL"
          >
            <input
              id="edit-endpoint"
              type="url"
              value={formData.endpoint}
              onChange={(e) => setFormData({ ...formData, endpoint: e.target.value })}
              placeholder="https://api.example.com"
            />
          </FormField>
        )}

        <button
          className="toggle-advanced-btn"
          onClick={() => setShowAdvanced(!showAdvanced)}
        >
          {showAdvanced ? "▼" : "▶"} Advanced: Edit JSON Config
        </button>

        {showAdvanced && (
          <div className="json-editor">
            <FormField label="JSON Configuration" htmlFor="edit-json">
              <textarea
                id="edit-json"
                value={JSON.stringify(formData, null, 2)}
                onChange={(e) => {
                  try {
                    const parsed = JSON.parse(e.target.value);
                    setFormData(parsed);
                  } catch (err) {
                    // Invalid JSON, ignore
                  }
                }}
                rows={12}
                style={{ fontFamily: "monospace", fontSize: "13px" }}
              />
            </FormField>
          </div>
        )}
      </div>
    </Modal>
  );
};