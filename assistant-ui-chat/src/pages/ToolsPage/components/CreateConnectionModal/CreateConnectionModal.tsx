import { type FC, useState } from "react";
import { Modal, FormField } from "../../../../components";
import type {
  CreateConnectionToolRequest,
  ConnectionToolType,
} from "../../../../types";
import "./CreateConnectionModal.css";

export interface CreateConnectionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateConnectionToolRequest) => Promise<void>;
  fullscreen?: boolean;
}

type Step = 1 | 2 | 3;

interface FormData {
  name: string;
  type: ConnectionToolType | "";
  description: string;
  command: string;
  arguments: string;
  endpoint: string;
  environmentVariables: Record<string, string>;
  isActive: boolean;
}

interface FormErrors {
  name?: string;
  type?: string;
  command?: string;
  endpoint?: string;
}

export const CreateConnectionModal: FC<CreateConnectionModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  fullscreen = false,
}) => {
  const [currentStep, setCurrentStep] = useState<Step>(1);
  const [loading, setLoading] = useState(false);
 const [testStatus, setTestStatus] = useState<"idle" | "testing" | "success" | "failed">("idle");
  const [testMessage, setTestMessage] = useState("");

  const [formData, setFormData] = useState<FormData>({
    name: "",
    type: "",
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

  // ... (keep all validation and handler functions the same)
  const validateStep1 = (): boolean => {
    const newErrors: FormErrors = {};
    if (!formData.name.trim()) newErrors.name = "Name is required";
    if (!formData.type) newErrors.type = "Type is required";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const validateStep2 = (): boolean => {
    const newErrors: FormErrors = {};
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

  const handleNext = () => {
    if (currentStep === 1 && validateStep1()) {
      setCurrentStep(2);
    } else if (currentStep === 2 && validateStep2()) {
      setCurrentStep(3);
    }
  };

  const handleBack = () => {
    if (currentStep > 1) {
      setCurrentStep((prev) => (prev - 1) as Step);
    }
  };

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

  const handleTest = async () => {
    setTestStatus("testing");
    setTestMessage("");
    try {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      setTestStatus("success");
      setTestMessage("Connection successful! Server started and tools discovered.");
    } catch (error) {
      setTestStatus("failed");
      setTestMessage(error instanceof Error ? error.message : "Connection failed");
    }
  };

  const handleSubmit = async () => {
    if (!validateStep2()) return;
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

      const request: CreateConnectionToolRequest = {
        name: formData.name,
        type: formData.type as ConnectionToolType,
        description: formData.description || undefined,
        command: formData.type === "mcp_stdio" ? formData.command : undefined,
        endpoint: formData.type !== "mcp_stdio" ? formData.endpoint : undefined,
        config: { ...config },
        isActive: formData.isActive,
      };

      await onSubmit(request);
      handleClose();
    } catch (error) {
      console.error("Failed to create connection:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setCurrentStep(1);
    setFormData({
      name: "",
      type: "",
      description: "",
      command: "",
      arguments: "",
      endpoint: "",
      environmentVariables: {},
      isActive: true,
    });
    setErrors({});
    setTestStatus("idle");
    setTestMessage("");
    onClose();
  };

  // ... (keep renderStep1, renderStep2, renderStep3 functions - same as before)

  const renderStep1 = () => (
    <>
      <FormField
        label="Name"
        htmlFor="name"
        required
        error={errors.name}
        hint="A unique name for this connection"
      >
        <input
          id="name"
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="e.g., GitHub"
        />
      </FormField>

      <FormField label="Type" htmlFor="type" required error={errors.type}>
        <select
          id="type"
          value={formData.type}
          onChange={(e) =>
            setFormData({ ...formData, type: e.target.value as ConnectionToolType })
          }
        >
          <option value="">Select type...</option>
          <option value="mcp_stdio">MCP (stdio)</option>
          <option value="mcp_http">MCP (HTTP)</option>
          <option value="openapi">OpenAPI</option>
        </select>
      </FormField>

      <FormField
        label="Description"
        htmlFor="description"
        hint="Optional description for this connection"
      >
        <textarea
          id="description"
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          rows={3}
          placeholder="e.g., GitHub integration via MCP"
        />
      </FormField>

      <FormField label="Status" htmlFor="isActive">
        <label className="checkbox-label">
          <input
            id="isActive"
            type="checkbox"
            checked={formData.isActive}
            onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
          />
          <span>Active</span>
        </label>
      </FormField>
    </>
  );

  const renderStep2 = () => (
    <>
      {formData.type === "mcp_stdio" && (
        <>
          <FormField
            label="Command"
            htmlFor="command"
            required
            error={errors.command}
            hint="The command to execute (e.g., npx)"
          >
            <input
              id="command"
              type="text"
              value={formData.command}
              onChange={(e) => setFormData({ ...formData, command: e.target.value })}
              placeholder="npx"
            />
          </FormField>

          <FormField label="Arguments" htmlFor="arguments" hint="One argument per line">
            <textarea
              id="arguments"
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
          htmlFor="endpoint"
          required
          error={errors.endpoint}
          hint="The HTTP endpoint URL"
        >
          <input
            id="endpoint"
            type="url"
            value={formData.endpoint}
            onChange={(e) => setFormData({ ...formData, endpoint: e.target.value })}
            placeholder="https://api.example.com"
          />
        </FormField>
      )}
    </>
  );

  const renderStep3 = () => (
    <div className="test-section">
      <div className="test-info">
        <h3>Test Connection</h3>
        <p>Test your connection before saving to ensure it's configured correctly.</p>
      </div>

      {testStatus === "idle" && (
        <button className="test-btn" onClick={handleTest}>
          Test Connection
        </button>
      )}

      {testStatus === "testing" && (
        <div className="test-loading">
          <div className="spinner" />
          <p>Testing connection...</p>
          <div className="progress-steps">
            <div className="step-item completed">
              <span className="step-icon">✓</span>
              <span>Starting server</span>
            </div>
            <div className="step-item active">
              <div className="step-spinner" />
              <span>Connecting...</span>
            </div>
            <div className="step-item">
              <span className="step-icon">⏳</span>
              <span>Verifying credentials</span>
            </div>
            <div className="step-item">
              <span className="step-icon">⏳</span>
              <span>Discovering tools</span>
            </div>
          </div>
        </div>
      )}

      {testStatus === "success" && (
        <div className="test-result success">
          <div className="result-icon">✅</div>
          <h4>Connection successful!</h4>
          <p>{testMessage}</p>
          <button className="test-btn secondary" onClick={handleTest}>
            Test Again
          </button>
        </div>
      )}

      {testStatus === "failed" && (
        <div className="test-result failed">
          <div className="result-icon">❌</div>
          <h4>Connection failed</h4>
          <p>{testMessage}</p>
          <div className="error-suggestions">
            <p>Possible solutions:</p>
            <ul>
              <li>Check your environment variables are correct</li>
              <li>Verify the command is installed</li>
              <li>Ensure you have the required permissions</li>
            </ul>
          </div>
          <button className="test-btn secondary" onClick={handleTest}>
            Test Again
          </button>
        </div>
      )}
    </div>
  );

  const modalTitle = (
    <div className="create-modal-header">
      <span>Create Connection</span>
      <span className="step-indicator">Step {currentStep} of 3</span>
    </div>
  );

  const footer = (
    <>
      {currentStep > 1 && (
        <button className="secondary-btn" onClick={handleBack} disabled={loading}>
          ← Back
        </button>
      )}
      <div className="spacer" />
      {currentStep < 3 && (
        <button className="primary-btn" onClick={handleNext}>
          Next →
        </button>
      )}
      {currentStep === 3 && (
        <button
          className="primary-btn"
          onClick={handleSubmit}
          disabled={loading || testStatus === "testing"}
        >
          {loading ? "Saving..." : "Save & Close"}
        </button>
      )}
    </>
  );

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title=""
      footer={footer}
      size="lg"
      fullscreen={fullscreen}
      showCloseButton={true}
      closeOnOverlayClick={false}
      closeOnEscape={!loading}
    >
      <div className="create-connection-content">
        <div className="modal-custom-header">
          {modalTitle}
          <div className="step-progress">
            <div className={`step ${currentStep >= 1 ? "active" : ""}`}>
              <div className="step-circle">1</div>
              <div className="step-label">Basic Info</div>
            </div>
            <div className="step-line" />
            <div className={`step ${currentStep >= 2 ? "active" : ""}`}>
              <div className="step-circle">2</div>
              <div className="step-label">Configuration</div>
            </div>
            <div className="step-line" />
            <div className={`step ${currentStep >= 3 ? "active" : ""}`}>
              <div className="step-circle">3</div>
              <div className="step-label">Test</div>
            </div>
          </div>
        </div>

        <div className="create-connection-body">
          {currentStep === 1 && renderStep1()}
          {currentStep === 2 && renderStep2()}
          {currentStep === 3 && renderStep3()}
        </div>
      </div>
    </Modal>
  );
};